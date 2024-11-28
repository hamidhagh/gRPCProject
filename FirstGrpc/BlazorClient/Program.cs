using Basics;
using BlazorClient;
using BlazorClient.Components;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<BlazorClient.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddGrpcClient<FirstServiceDefinition.FirstServiceDefinitionClient>(
    o =>
    {
        o.Address = new Uri("https://localhost:7057");
    }).ConfigureChannel(o => o.HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler()));

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
