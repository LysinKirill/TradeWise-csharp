using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using TradeWiseBackend.Api.Extensions;
using TradeWiseBackend.Bll.Extensions;
using TradeWiseBackend.Dal.DatabaseSettings;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Dal.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddIdentityServices();
builder.Services.AddBllServices();

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection(nameof(DbSettings)));
var config = builder.Configuration.GetRequiredSection("DbSettings").Get<DbSettings>()!;
builder.Services.AddDalRepositories().AddDalInfrastructure(config);

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TradeWise", Version = "v1"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();