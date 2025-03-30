using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Api.Configuration;
using TradeWiseBackend.Api.Extensions;
using TradeWiseBackend.Bll.Extensions;
using TradeWiseBackend.Dal.DatabaseSettings;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Dal.Extensions;
using User;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwagger();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddBllServices();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection(nameof(DbSettings)));
var config = builder.Configuration.GetRequiredSection("DbSettings").Get<DbSettings>()!;
builder.Services.AddDalRepositories().AddDalInfrastructure(config);

//TODO: use another way of setting secrets
builder.Configuration.AddUserSecrets<Program>();

var certThumbprint = builder.Configuration["Grpc:CertThumbprint"];
if (string.IsNullOrEmpty(certThumbprint))
{
    throw new InvalidOperationException("gRPC certificate thumbprint not configured. Set it in User Secrets.");
}

var cert = X509CertificateLoader.LoadCertificateFromFile("ssl/cert.pem");
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(cert);
handler.ServerCertificateCustomValidationCallback = 
    (_, actualCert, _, _) => actualCert?.Thumbprint == certThumbprint;


builder.Services.AddGrpcClient<UserService.UserServiceClient>(options =>
    {
        options.Address = new Uri("https://localhost:50051");
    })
    .ConfigurePrimaryHttpMessageHandler(() => handler);


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TradeWise API V1");
    options.RoutePrefix = string.Empty;
});

app
    .MapGroup("api/v1/")
    .MapIdentityApi<AccountEntity>();

app.UseAuthorization();

app.MapControllers();
// Add this to your builder configuration
builder.Configuration.AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// Update the AddIdentityServices call to pass configuration
builder.Services.AddIdentityServices(builder.Configuration);

// Add JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? 
             "2ddc811a5eee11f682c2205ea73326336945877604331a334eaf159e7d2f6d02596b01303c47741133efa7b67e78e35e2442a04acc9f56bdb255fd0d00147500896b45011f2dd6756bb5e012373986bd762077648fbb8cb2cbf66d062464333436ffe9d80e62622f48a68701f8eb190ef7bb71023834cbdb6d01d5ec8cf0c904bf65da75204494b838401dac1b624ae80f24702a997069fb3a9420a205b6b2e1a4a1b7835ef2e7cd321bd279b779f224a62c4c5b9e686f231eb87a3b64ffae20f7b218158151dcaca3ef6148d51a69213e2fa4abe99552590179cdbe8149fe15ab5fd52cd965245c2ec6e808978b2d092af9531dc304e974a7c74ef817a69f33";

builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = jwtKey;
    options.Issuer = builder.Configuration["Jwt:Issuer"] ?? "TradeWiseBackend";
    options.Audience = builder.Configuration["Jwt:Audience"] ?? "TradeWiseClient";
    options.ExpiryInMinutes = int.Parse(builder.Configuration["Jwt:ExpiryInMinutes"] ?? "60");
});

// Add this before app.Run()
app.UseAuthentication();
app.UseAuthorization();

app.Run();