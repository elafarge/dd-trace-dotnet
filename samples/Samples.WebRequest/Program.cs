using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Datadog.Core.Tools;
using Datadog.Trace;

namespace Samples.WebRequest
{
    public static class Program
    {
        private const string RequestContent = "PING";
        private const string ResponseContent = "PONG";
        private static readonly Encoding Utf8 = Encoding.UTF8;
        private static Thread listenerThread;

        private static string Url;
        private static string ManagedProfilerDirectory;

        public static void Main(string[] args)
        {
            // Set the minimum permissions needed to run code in the new AppDomain
            PermissionSet permSet = new PermissionSet(PermissionState.None);
            // SecurityPermissionFlag.SkipVerification
            // SecurityPermissionFlag.UnmanagedCode
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution | SecurityPermissionFlag.SkipVerification));
            // permSet.AddPermission(new System.Net.WebPermission(PermissionState.Unrestricted)); // Needed for WebRequest sample AND Tracer
            permSet.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted)); // Needed to get DD_TRACER_HOME variable
            // permSet.AddPermission(new FileIOPermission(PermissionState.Unrestricted)); // Needed to load assemblies from disk
            permSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess | ReflectionPermissionFlag.RestrictedMemberAccess)); // Needed for dynamic method builder

            // Allow Sigil to be run with FullTrust
            var sigilStrongName = new StrongName(new StrongNamePublicKeyBlob(new byte[] { 0x2d, 0x06, 0xc3, 0x49, 0x43, 0x41, 0xc8, 0xab }), "Sigil", new Version(4, 8, 41, 0));

            var remote = AppDomain.CreateDomain("Remote", null, AppDomain.CurrentDomain.SetupInformation, permSet, sigilStrongName);

            // Add AssemblyResolve event
            remote.AssemblyResolve += AssemblyResolve_ManagedProfilerDependencies;

            try
            {
                remote.DoCallBack(RunWebRequestSync);
            }
            finally
            {
                AppDomain.Unload(remote);
            }
        }

        public static void RunWebRequestSync()
        {
            // LoaderStartupPath();
            for (int i = 0; i < 5; i++)
            {
                EmitCustomSpans();
                Thread.Sleep(2000);
            }

            // RunWebRequest().GetAwaiter().GetResult();
        }

        private static void EmitCustomSpans()
        {
            using (Tracer.Instance.StartActive("custom-span"))
            {
                Thread.Sleep(1500);

                using (Tracer.Instance.StartActive("inner-span"))
                {
                    Thread.Sleep(1500);
                }
            }
        }

        private static string ReadEnvironmentVariable(string key)
        {
            try
            {
                return Environment.GetEnvironmentVariable(key);
            }
            catch (Exception ex)
            {
                // StartupLogger.Log(ex, "Error while loading environment variable " + key);
            }

            return null;
        }

        private static string ResolveManagedProfilerDirectory()
        {
            // We currently build two assemblies targeting .NET Framework.
            // If we're running on the .NET Framework, load the highest-compatible assembly
            string corlibFileVersionString = ((AssemblyFileVersionAttribute)typeof(object).Assembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version;
            string corlib461FileVersionString = "4.6.1055.0";

            // This will throw an exception if the version number does not match the expected 2-4 part version number of non-negative int32 numbers,
            // but mscorlib should be versioned correctly
            var corlibVersion = new Version(corlibFileVersionString);
            var corlib461Version = new Version(corlib461FileVersionString);
            var tracerFrameworkDirectory = corlibVersion < corlib461Version ? "net45" : "net461";

            var tracerHomeDirectory = ReadEnvironmentVariable("DD_DOTNET_TRACER_HOME") ?? string.Empty;
            return Path.Combine(tracerHomeDirectory, tracerFrameworkDirectory);
        }

        public static Assembly AssemblyResolve_ManagedProfilerDependencies(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name;

            // On .NET Framework, having a non-US locale can cause mscorlib
            // to enter the AssemblyResolve event when searching for resources
            // in its satellite assemblies. Exit early so we don't cause
            // infinite recursion.
            if (string.Equals(assemblyName, "mscorlib.resources", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(assemblyName, "System.Net.Http", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var path = Path.Combine(ManagedProfilerDirectory, $"{assemblyName}.dll");
            if (File.Exists(path))
            {
                // StartupLogger.Debug("Loading {0}", path);
                return Assembly.LoadFrom(path);
            }

            return null;
        }

        private static void LoaderStartupPath()
        {
            ManagedProfilerDirectory = ResolveManagedProfilerDirectory();

            try
            {
                var assembly = Assembly.Load("Datadog.Trace.ClrProfiler.Managed, Version=1.19.2.0, Culture=neutral, PublicKeyToken=def86d061d0d2eeb");

                if (assembly != null)
                {
                    // call method Datadog.Trace.ClrProfiler.Instrumentation.Initialize()
                    var type = assembly.GetType("Datadog.Trace.ClrProfiler.Instrumentation", throwOnError: false);
                    var method = type?.GetRuntimeMethod("Initialize", parameters: new Type[0]);
                    method?.Invoke(obj: null, parameters: null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when loading managed assemblies.");
                Console.WriteLine(ex);
            }
        }

        public static async Task RunWebRequest()
        {
            // bool tracingDisabled = args.Any(arg => arg.Equals("TracingDisabled", StringComparison.OrdinalIgnoreCase));
            bool tracingDisabled = false;
            Console.WriteLine($"TracingDisabled {tracingDisabled}");

            // string port = args.FirstOrDefault(arg => arg.StartsWith("Port="))?.Split('=')[1] ?? "9000";
            string port = "9000";
            Console.WriteLine($"Port {port}");

            using (var listener = StartHttpListenerWithPortResilience(port))
            {
                Console.WriteLine();
                Console.WriteLine($"Starting HTTP listener at {Url}");

                // send http requests using WebClient
                Console.WriteLine();
                Console.WriteLine("Sending request with WebClient.");
                await RequestHelpers.SendWebClientRequests(tracingDisabled, Url, RequestContent);
                await RequestHelpers.SendWebRequestRequests(tracingDisabled, Url, RequestContent);

                Console.WriteLine();
                Console.WriteLine("Stopping HTTP listener.");
                listener.Stop();
            }

            // Force process to end, otherwise the background listener thread lives forever in .NET Core.
            // Apparently listener.GetContext() doesn't throw an exception if listener.Stop() is called,
            // like it does in .NET Framework.
            // Environment.Exit(0);
        }

        public static HttpListener StartHttpListenerWithPortResilience(string port, int retries = 5)
        {
            // try up to 5 consecutive ports before giving up
            while (true)
            {
                Url = $"http://localhost:{port}/Samples.WebRequest/";

                // seems like we can't reuse a listener if it fails to start,
                // so create a new listener each time we retry
                var listener = new HttpListener();
                listener.Prefixes.Add(Url);

                try
                {
                    listener.Start();

                    listenerThread = new Thread(HandleHttpRequests);
                    listenerThread.Start(listener);

                    return listener;
                }
                catch (HttpListenerException) when (retries > 0)
                {
                    // only catch the exception if there are retries left
                    port = TcpPortProvider.GetOpenPort().ToString();
                    retries--;
                }

                // always close listener if exception is thrown,
                // whether it was caught or not
                listener.Close();
            }
        }

        private static void HandleHttpRequests(object state)
        {
            var listener = (HttpListener)state;

            while (listener.IsListening)
            {
                try
                {
                    var context = listener.GetContext();

                    Console.WriteLine("[HttpListener] received request");

                    // read request content and headers
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        string requestContent = reader.ReadToEnd();
                        Console.WriteLine($"[HttpListener] request content: {requestContent}");

                        foreach (string headerName in context.Request.Headers)
                        {
                            string headerValue = context.Request.Headers[headerName];
                            Console.WriteLine($"[HttpListener] request header: {headerName}={headerValue}");
                        }
                    }

                    // write response content
                    byte[] responseBytes = Utf8.GetBytes(ResponseContent);
                    context.Response.ContentEncoding = Utf8;
                    context.Response.ContentLength64 = responseBytes.Length;
                    context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);

                    // we must close the response
                    context.Response.Close();
                }
                catch (HttpListenerException)
                {
                    // listener was stopped,
                    // ignore to let the loop end and the method return
                }
            }
        }
    }
}
