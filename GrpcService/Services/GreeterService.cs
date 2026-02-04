using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GrpcService.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly TimeSpan _streamDelay = TimeSpan.FromMinutes(5);

        public GreeterService(ILogger<GreeterService> logger, IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _lifetime = lifetime;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Saying hello to {name}", request.Name);
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
        
        public override async Task SayHelloWithServerStreaming(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("SayHelloWithServerStreaming call started with request {name}", request.Name );
            try
            {
                int msgCount = 0;
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, _lifetime.ApplicationStopping);
                await responseStream.WriteAsync(new HelloReply { Message = $"Writing to message stream every {_streamDelay}" }, linkedCts.Token);
                while (!linkedCts.Token.IsCancellationRequested)
                {
                    msgCount++;
                    await Task.Delay(_streamDelay, linkedCts.Token);

                    _logger.LogInformation($"SayHelloWithServerStreaming writing message {msgCount}...");
                    await responseStream.WriteAsync(new HelloReply { Message = $"hello {request.Name} #{msgCount}" }, linkedCts.Token);
                }
                _logger.LogInformation("SayHelloWithServerStreaming call completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occured during SayHelloWithServerStreaming");
            }
        }
    }
}
