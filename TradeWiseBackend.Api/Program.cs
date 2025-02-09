using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradeWiseBackend.Api.Extensions;
using TradeWiseBackend.Bll.Extensions;
using TradeWiseBackend.Dal.DatabaseSettings;
using TradeWiseBackend.Dal.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
builder.Services.AddControllers();
builder.Services.AddIdentityServices();
builder.Services.AddBllServices();

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection(nameof(DbSettings)));
var config = builder.Configuration.GetRequiredSection("DbSettings").Get<DbSettings>()!;
builder.Services.AddDalRepositories().AddDalInfrastructure(config);

var app = builder.Build();

// Middleware configuration
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TradeWise API V1");
    options.RoutePrefix = string.Empty;
});

app.MapControllers();

// Run the application
app.Run();