using TaskDistribution.BLL;
using EtaiEcoSystem.BaseInitializers.Service;
using System.Reflection;
using Integration.Google.Maps;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddGoogleApi(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddTaskDistributionBLL(builder.Configuration);
builder.InitBaseService(opt =>
{
    opt.BusEventQueueName = "taskDistribution";
    opt.SwaggerGenOptions = c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskDistribution API", Version = "v1" });
    };
    opt.Assemblies = new Assembly[] { typeof(TaskDistribution.BLL.Configure).Assembly, };
});
var app = builder.Build();

app.UseBaseService(opt =>
{
    opt.SwaggerUIOptions = c =>
    {
        c.SwaggerEndpoint("../swagger/v1/swagger.json", "TaskDistribution API V1");
    };
});

app.MapControllers();

app.Run();
