using System;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

/*
https://github.com/microsoft/ApplicationInsights-ServiceFabric/blob/master/src/ApplicationInsights.ServiceFabric.Native.Shared/DependencyTrackingModule/ServiceRemotingClientEventListener.cs
https://github.com/microsoft/ApplicationInsights-ServiceFabric/blob/master/src/ApplicationInsights.ServiceFabric.Native.Shared/RequestTrackingModule/ServiceRemotingServerEventListener.cs
*/

namespace Datadog.Trace.ServiceFabric
{
    /// <summary>
    /// Provides methods used start and stop tracing Service Remoting requests.
    /// </summary>
    public static class Remoting
    {
        private const string IntegrationName = "ServiceRemoting";
        private const string SpanNamePrefix = "service-remoting";

        private static int _enabled;

        /// <summary>
        /// Start tracing Service Remoting requests.
        /// </summary>
        public static void StartTracing()
        {
            if (Interlocked.CompareExchange(ref _enabled, 1, 0) == 0)
            {
                // client
                ServiceRemotingClientEvents.SendRequest += ServiceRemotingClientEvents_SendRequest;
                ServiceRemotingClientEvents.ReceiveResponse += ServiceRemotingClientEvents_ReceiveResponse;

                // server
                ServiceRemotingServiceEvents.ReceiveRequest += ServiceRemotingServiceEvents_ReceiveRequest;
                ServiceRemotingServiceEvents.SendResponse += ServiceRemotingServiceEvents_SendResponse;
            }
        }

        /// <summary>
        /// Stop tracing Service Remoting requests.
        /// </summary>
        public static void StopTracing()
        {
            if (Interlocked.CompareExchange(ref _enabled, 0, 1) == 1)
            {
                // client
                ServiceRemotingClientEvents.SendRequest -= ServiceRemotingClientEvents_SendRequest;
                ServiceRemotingClientEvents.ReceiveResponse -= ServiceRemotingClientEvents_ReceiveResponse;

                // server
                ServiceRemotingServiceEvents.ReceiveRequest -= ServiceRemotingServiceEvents_ReceiveRequest;
                ServiceRemotingServiceEvents.SendResponse -= ServiceRemotingServiceEvents_SendResponse;
            }
        }

        private static void ServiceRemotingClientEvents_SendRequest(object? sender, EventArgs e)
        {
            if (_enabled == 0)
            {
                return;
            }

            try
            {
                var eventArgs = e as ServiceRemotingRequestEventArgs;

                if (eventArgs == null)
                {
                    // TODO: log
                }

                var messageHeaders = eventArgs?.Request?.GetHeader();

                if (messageHeaders == null)
                {
                    // TODO: log
                }

                var tracer = Tracer.Instance;
                var span = CreateSpan(tracer, context: null, SpanKinds.Client, eventArgs, messageHeaders, enabledAnalyticsWithGlobalSetting: false);

                try
                {
                    // inject trace propagation headers for distributed tracing
                    if (messageHeaders != null)
                    {
                        if (!messageHeaders.TryGetHeaderValue(HttpHeaderNames.TraceId, out _))
                        {
                            messageHeaders.AddHeader(HttpHeaderNames.TraceId, BitConverter.GetBytes(span.TraceId));
                        }

                        if (!messageHeaders.TryGetHeaderValue(HttpHeaderNames.ParentId, out _))
                        {
                            messageHeaders.AddHeader(HttpHeaderNames.ParentId, BitConverter.GetBytes(span.SpanId));
                        }

                        if (!messageHeaders.TryGetHeaderValue(HttpHeaderNames.SamplingPriority, out _) &&
                            ulong.TryParse(span.GetTag(Tags.SamplingPriority), out ulong samplingPriority))
                        {
                            messageHeaders.AddHeader(HttpHeaderNames.SamplingPriority, BitConverter.GetBytes(samplingPriority));
                        }

                        if (!messageHeaders.TryGetHeaderValue(HttpHeaderNames.Origin, out _))
                        {
                            string origin = span.GetTag(Tags.Origin);

                            if (!string.IsNullOrEmpty(origin))
                            {
                                messageHeaders.AddHeader(HttpHeaderNames.Origin, Encoding.UTF8.GetBytes(origin));
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // TODO: log
                    throw;
                }

                tracer.ActivateSpan(span);
            }
            catch (Exception)
            {
                // TODO: log
                throw;
            }
        }

        private static void ServiceRemotingClientEvents_ReceiveResponse(object? sender, EventArgs e)
        {
            if (_enabled == 0)
            {
                return;
            }

            // var successfulResponseArg = e as ServiceRemotingResponseEventArgs;
            // var failedResponseArg = e as ServiceRemotingFailedResponseEventArgs;

            try
            {
                var scope = Tracer.Instance.ActiveScope;

                if (scope != null)
                {
                    if (e is ServiceRemotingFailedResponseEventArgs failedResponseArg && failedResponseArg.Error != null)
                    {
                        scope.Span?.SetException(failedResponseArg.Error);
                    }

                    scope.Dispose();
                }
            }
            catch (Exception)
            {
                // TODO: log
                throw;
            }
        }

        private static void ServiceRemotingServiceEvents_ReceiveRequest(object? sender, EventArgs e)
        {
            if (_enabled == 0)
            {
                return;
            }

            try
            {
                var eventArgs = e as ServiceRemotingRequestEventArgs;

                if (eventArgs == null)
                {
                    // TODO: log
                }

                IServiceRemotingRequestMessageHeader? messageHeaders = null;
                SpanContext? context = null;
                string? origin = null;

                try
                {
                    messageHeaders = eventArgs?.Request?.GetHeader();

                    if (messageHeaders == null)
                    {
                        // TODO: log
                    }
                    else
                    {
                        // extract trace propagation headers for distributed tracing
                        ulong? traceId = null;
                        ulong? parentId = null;
                        SamplingPriority? samplingPriority = null;

                        if (messageHeaders.TryGetHeaderValue(HttpHeaderNames.TraceId, out byte[] traceIdBytes) && traceIdBytes?.Length == sizeof(ulong))
                        {
                            traceId = BitConverter.ToUInt64(traceIdBytes, 0);
                        }

                        if (messageHeaders.TryGetHeaderValue(HttpHeaderNames.ParentId, out byte[] parentIdBytes) && parentIdBytes?.Length == sizeof(ulong))
                        {
                            parentId = BitConverter.ToUInt64(parentIdBytes, 0);
                        }

                        if (messageHeaders.TryGetHeaderValue(HttpHeaderNames.SamplingPriority, out byte[] samplingPriorityBytes) && samplingPriorityBytes?.Length == sizeof(int))
                        {
                            samplingPriority = (SamplingPriority)BitConverter.ToInt32(samplingPriorityBytes, 0);
                        }

                        if (messageHeaders.TryGetHeaderValue(HttpHeaderNames.Origin, out byte[] originBytes) && originBytes?.Length > 0)
                        {
                            origin = Encoding.UTF8.GetString(originBytes);
                        }

                        if (traceId != null && parentId != null)
                        {
                            context = new SpanContext(traceId, parentId.Value, samplingPriority);
                        }
                    }
                }
                catch (Exception)
                {
                    // TODO: log
                    throw;
                }

                var tracer = Tracer.Instance;
                var span = CreateSpan(tracer, context, SpanKinds.Server, eventArgs, messageHeaders, enabledAnalyticsWithGlobalSetting: true);

                if (!string.IsNullOrEmpty(origin))
                {
                    span.SetTag(Tags.Origin, origin);
                }

                tracer.ActivateSpan(span);
            }
            catch (Exception)
            {
                // TODO: log
                throw;
            }
        }

        private static void ServiceRemotingServiceEvents_SendResponse(object? sender, EventArgs e)
        {
            if (_enabled == 0)
            {
                return;
            }

            // var successfulResponseArg = e as ServiceRemotingResponseEventArgs;
            // var failedResponseArg = e as ServiceRemotingFailedResponseEventArgs;

            var scope = Tracer.Instance.ActiveScope;

            if (scope != null)
            {
                if (e is ServiceRemotingFailedResponseEventArgs failedResponseArg && failedResponseArg.Error != null)
                {
                    scope.Span?.SetException(failedResponseArg.Error);
                }

                scope.Dispose();
            }
        }

        private static Span CreateSpan(
            Tracer tracer,
            SpanContext? context,
            string spanKind,
            ServiceRemotingRequestEventArgs? eventArgs,
            IServiceRemotingRequestMessageHeader? messageHeader,
            bool enabledAnalyticsWithGlobalSetting)
        {
            string? methodName = null;
            string? resourceName = null;
            string? serviceUrl = eventArgs?.ServiceUri?.AbsoluteUri;

            if (eventArgs != null)
            {
                methodName = eventArgs.MethodName;

                if (string.IsNullOrEmpty(methodName))
                {
                    // use the numeric id as the method name
                    methodName = messageHeader == null ? "unknown" : messageHeader.MethodId.ToString(CultureInfo.InvariantCulture);
                }

                resourceName = serviceUrl == null ? methodName : $"{serviceUrl}/{methodName}";
            }

            Span span = tracer.StartSpan($"{SpanNamePrefix}.{spanKind}", context);
            span.ResourceName = resourceName ?? "unknown";
            span.SetTag(Tags.SpanKind, spanKind);

            if (serviceUrl != null)
            {
                span.SetTag(Tags.HttpUrl, serviceUrl);
            }

            if (methodName != null)
            {
                span.SetTag("method-name", methodName);
            }

            if (messageHeader != null)
            {
                span.SetTag("method-id", messageHeader.MethodId.ToString(CultureInfo.InvariantCulture));
                span.SetTag("interface-id", messageHeader.InterfaceId.ToString(CultureInfo.InvariantCulture));

                if (messageHeader.InvocationId != null)
                {
                    span.SetTag("invocation-id", messageHeader.InvocationId);
                }
            }

            double? analyticsSampleRate = GetAnalyticsSampleRate(tracer, enabledAnalyticsWithGlobalSetting);

            if (analyticsSampleRate != null)
            {
                span.SetTag(Tags.Analytics, analyticsSampleRate.Value.ToString(CultureInfo.InvariantCulture));
            }

            return span;
        }

        private static double? GetAnalyticsSampleRate(Tracer tracer, bool enabledWithGlobalSetting)
        {
            var integrationSettings = tracer.Settings.Integrations[IntegrationName];
            var analyticsEnabled = integrationSettings.AnalyticsEnabled ?? (enabledWithGlobalSetting && tracer.Settings.AnalyticsEnabled);
            return analyticsEnabled ? integrationSettings.AnalyticsSampleRate : (double?)null;
        }
    }
}
