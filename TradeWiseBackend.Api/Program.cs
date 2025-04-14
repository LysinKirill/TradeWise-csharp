using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Api.Configuration;
using TradeWiseBackend.Api.Extensions;
using TradeWiseBackend.Bll.Extensions;
using TradeWiseBackend.Dal;
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

var certThumbprint = builder.Configuration["Grpc:CertThumbprint"]
                     ?? Environment.GetEnvironmentVariable("Grpc__CertThumbprint");
if (string.IsNullOrEmpty(certThumbprint))
    throw new InvalidOperationException("gRPC certificate thumbprint not configured. Set it in User Secrets.");

var cert = X509CertificateLoader.LoadCertificateFromFile("ssl/cert.pem");
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(cert);
handler.ServerCertificateCustomValidationCallback =
    (_, actualCert, _, _) => actualCert?.Thumbprint == certThumbprint;


//TODO: replace localhost with uri
builder.Services.AddGrpcClient<UserService.UserServiceClient>(options =>
    {
        options.Address = new Uri("https://python-backend:50051");
    })
    .ConfigurePrimaryHttpMessageHandler(() => handler);


// Add this to your builder configuration
builder.Configuration.AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();


var jwtKey = builder.Configuration["Jwt:Key"] ??
             Environment.GetEnvironmentVariable("JWT_KEY")!;

builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = jwtKey;
    options.Issuer = builder.Configuration["Jwt:Issuer"] ?? "TradeWiseBackend";
    options.Audience = builder.Configuration["Jwt:Audience"] ?? "TradeWiseClient";
    options.ExpiryInMinutes = int.Parse(builder.Configuration["Jwt:ExpiryInMinutes"] ?? "60");
});

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

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.MigrateAsync();


app.Run();