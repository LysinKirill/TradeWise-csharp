using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using DotNetEnv;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Api.Configuration;
using TradeWiseBackend.Api.Extensions;
using TradeWiseBackend.Api.PythonBackend;
using TradeWiseBackend.Bll.Extensions;
using TradeWiseBackend.Dal;
using TradeWiseBackend.Dal.DatabaseSettings;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Dal.Extensions;
using User;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Services.AddExceptionHandlingMiddleware(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwagger();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddBllServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


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
//TODO: move into extensions
builder.Services.Configure<PythonBackend>(builder.Configuration.GetSection(nameof(PythonBackend)));
var python_backend = builder.Configuration.GetRequiredSection("PythonBackend").Get<PythonBackend>()!;
builder.Services.AddGrpcClient<UserService.UserServiceClient>(options =>
    {
        options.Address = new Uri(python_backend.Url);
    })
    .ConfigurePrimaryHttpMessageHandler(() => handler);
builder.Services.AddGrpcClient<Invest.InvestService.InvestServiceClient>(options =>
    {
        options.Address = new Uri(python_backend.Url);
    })
    .ConfigurePrimaryHttpMessageHandler(() => handler);

var environment = builder.Environment;
builder.Configuration.AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true)
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

app.UseProblemDetails();
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    AllowStatusCode404Response = true,
    ExceptionHandler = async context =>
    {
        context.Response.ContentType = "application/json";

        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception == null)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var errorObj = new { error = "Internal Server Error" };
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorObj));
            return;
        }

        var handlers = context.RequestServices.GetServices<IExceptionHandler>();
        foreach (var handler in handlers)
        {
            var cancellationToken = context.RequestAborted;
            var handled = await handler.TryHandleAsync(context, exception, cancellationToken);
            if (handled) return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var fallbackError = new { error = "Internal Server Error", code = 500 };
        var fallbackJson = JsonSerializer.Serialize(fallbackError);

        await context.Response.WriteAsync(fallbackJson);
    }
});

// TODO: прокинуть везде CancellationToken ct

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
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
if (pendingMigrations.Any())
{
    Console.WriteLine("Applying migrations...");
    await dbContext.Database.MigrateAsync();
    Console.WriteLine("Migrations applied.");
}

app.Run();