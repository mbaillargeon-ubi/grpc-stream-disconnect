using Grpc.Core;
using Grpc.Net.Client;

namespace GrpcClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string SERVER_ADDRESS = "http://myserver:62012";

            try
            {
                Console.WriteLine("Press a key to start...");
                Console.ReadKey(true);
                var handler = new SocketsHttpHandler
                {
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                };
                using var channel = GrpcChannel.ForAddress(SERVER_ADDRESS, new GrpcChannelOptions { HttpHandler = handler });
                var client = new Greeter.GreeterClient(channel);

                Console.WriteLine("Calling SayHelloAsync...");
                var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
                Console.WriteLine("Greeting: " + reply.Message);

                Console.WriteLine("Calling SayHelloWithServerStreaming...");
                using (AsyncServerStreamingCall<HelloReply> serverStreaming = client.SayHelloWithServerStreaming(new HelloRequest() { Name = "GreeterClientStreaming" }))
                {
                    while (await serverStreaming.ResponseStream.MoveNext())
                    {
                        HelloReply streamReply = serverStreaming.ResponseStream.Current;
                        Console.WriteLine("Stream Reply: " + streamReply.Message);
                    }
                    Console.WriteLine("Server stream terminated");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
