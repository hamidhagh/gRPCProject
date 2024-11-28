using Auth;
using FirstGrpc.Interceptors;
using FirstGrpc.Services;
using Grpc.Net.Compression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));


// Add services to the container.
builder.Services.AddGrpc(option =>
{
    option.Interceptors.Add<ServerLoggingInterceptor>();

});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o => o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateActor = false,
                    ValidateLifetime = false,
                    IssuerSigningKey = JwtHelper.SecurityKey
                });

builder.Services.AddAuthorization(o => o.AddPolicy(JwtBearerDefaults.AuthenticationScheme,
    p =>
    {
        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        p.RequireClaim(ClaimTypes.Name);
    }

    ));

builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHealthChecks(o =>
{

}).AddCheck("my cool service", () => HealthCheckResult.Healthy(), new[] { "grpc", "live" });
var app = builder.Build();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FirstService>();
app.MapGrpcHealthChecksService();
app.MapGrpcReflectionService();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

public partial class Program { }