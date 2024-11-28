using Grpc.Core.Interceptors;

namespace MVCClient.Interceptors
{
    public class ClientLoggerInterceptor : Interceptor
    {
        private readonly ILogger<ClientLoggerInterceptor> logger;

        public ClientLoggerInterceptor(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<ClientLoggerInterceptor>();
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                logger.LogInformation($"starting the client call of type: {context.Method.FullName}, {context.Method.Type}");
                return continuation(request, context);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
