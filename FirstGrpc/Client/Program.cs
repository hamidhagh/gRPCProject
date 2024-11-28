using Basics;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Grpc.Reflection.V1Alpha;
using System.Reflection;
using static Grpc.Core.Metadata;
using ServerReflectionClient = Grpc.Reflection.V1Alpha.ServerReflection.ServerReflectionClient;

Console.WriteLine("Hello, World!");

using var channelReflect = GrpcChannel.ForAddress("https://localhost:7057");
var client = new ServerReflectionClient(channelReflect); ;
ServerReflectionRequest request = new ServerReflectionRequest() { ListServices = "" };

using var call = client.ServerReflectionInfo();
await call.RequestStream.WriteAsync(request);
await call.RequestStream.CompleteAsync();

while (await call.ResponseStream.MoveNext())
{
    var response = call.ResponseStream.Current;
    foreach (var item in response.ListServicesResponse.Service)
    {
        Console.WriteLine("- " + item.Name);
    }
}











var retryPolicy = new MethodConfig
{
    Names = { MethodName.Default },
    RetryPolicy = new RetryPolicy()
    {
        MaxAttempts = 5,
        BackoffMultiplier = 1,
        InitialBackoff = TimeSpan.FromSeconds(0.5),
        MaxBackoff = TimeSpan.FromSeconds(0.5),
        RetryableStatusCodes = { StatusCode.Internal }
    }
};

var hedging = new MethodConfig
{
    Names = { MethodName.Default },
    HedgingPolicy = new HedgingPolicy()
    {
        MaxAttempts = 5,
        NonFatalStatusCodes = { StatusCode.Internal },
        HedgingDelay = TimeSpan.FromSeconds(0.5),
    }
};

var options = new GrpcChannelOptions()
{
    ServiceConfig = new ServiceConfig()
    {
        MethodConfigs = { hedging }
    }

};


using var channel = GrpcChannel.ForAddress("https://localhost:7057", options);

//health checks
var healthClient = new Health.HealthClient(channel);
var healthResult = await healthClient.CheckAsync(new HealthCheckRequest());
Console.WriteLine($"Health status : {healthResult.Status}");

//var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);
//Unary(client);

Console.ReadLine();

void Unary(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var request = new Request() { Content = "Hello you!" };

    var response = client.Unary(request);
    // var response = client.Unary(request, deadline: DateTime.UtcNow.AddMilliseconds(3));

    Console.WriteLine($"Response:{response.Message}");
}


async void ServerStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    try
    {
        var cancellationToken = new CancellationTokenSource();
        var metadata = new Metadata();
        metadata.Add(new Entry("my-first-key", "my-first-value"));
        metadata.Add(new Entry("my-second-key", "my-second-value"));

        using var streamingCall = client.ServerStream(
            new Request() { Content = "Hello!" },
            headers: metadata
            );

        await foreach (var response in streamingCall.ResponseStream.ReadAllAsync(cancellationToken.Token))
        {
            Console.WriteLine(response.Message);
            if (response.Message.Contains("2"))
            {
                // cancellationToken.Cancel();
            }
        }

        var myTrailers = streamingCall.GetTrailers();
        var myValue = myTrailers.GetValue("a-trailer");
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
    {
    }
}

async void ClientStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var call = client.ClientStream();
    for (var i = 0; i < 1000; i++)
    {
        await call.RequestStream.WriteAsync(new Request() { Content = i.ToString() });
    }
    await call.RequestStream.CompleteAsync();
    Response response = await call;
    Console.WriteLine($"{response.Message}");
}

async void BiDirectionalStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using (var call = client.BiDirectionalStream())
    {
        var request = new Request();
        for (var i = 0; i < 100; i++)
        {
            request.Content = i.ToString();
            Console.WriteLine(request.Content);
            await call.RequestStream.WriteAsync(request);
        }

        while (await call.ResponseStream.MoveNext())
        {
            var message = call.ResponseStream.Current;
            Console.WriteLine(message);

        }

        await call.RequestStream.CompleteAsync();
    }
}

