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

namespace Samples.SandboxTracing
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Set the minimum permissions needed to run code in the new AppDomain
            PermissionSet permSet = new PermissionSet(PermissionState.None);
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permSet.AddPermission(new WebPermission(PermissionState.Unrestricted)); // If enabled, the Tracer will send Traces. If disabled, the Tracer will not send Traces but should not crash.

            var remote = AppDomain.CreateDomain("Remote", null, AppDomain.CurrentDomain.SetupInformation, permSet);

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
            for (int i = 0; i < 5; i++)
            {
                EmitCustomSpans();
                Thread.Sleep(2000);
            }
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
    }
}
