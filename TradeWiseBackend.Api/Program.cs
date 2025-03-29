using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
builder.Services.AddIdentityServices();
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

app.Run();