using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Api.Extensions;
using TradeWiseBackend.Bll.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
builder.Services.AddControllers();
builder.Services.AddBllServices();

var app = builder.Build();

{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TradeWise API V1");
        options.RoutePrefix = string.Empty;
    });
}

app.MapControllers();

app.Run();