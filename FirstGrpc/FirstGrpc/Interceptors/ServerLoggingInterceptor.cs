using Grpc.Core;
using Grpc.Core.Interceptors;

namespace FirstGrpc.Interceptors
{
    public class ServerLoggingInterceptor : Interceptor
    {
        private readonly ILogger<ServerLoggingInterceptor> logger;

        public ServerLoggingInterceptor(ILogger<ServerLoggingInterceptor> logger)
        {
            this.logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context, 
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try {
                logger.LogInformation("Server intercepting here!!!");
                return await continuation(request, context);
            }
            catch (Exception ex){
                logger.LogError(ex, $"Error thrown by {context.Method}");
                throw;
            }
        }
    }
}
