
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TestABan.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<ClienteContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MainConnString")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(sc => 
    {
        sc.SwaggerDoc("v1", new OpenApiInfo 
        {
            Version = "v1",
            Title = "API de Clientes",
            Description = "ABM de Clientes a través de esta API"
        });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        sc.IncludeXmlComments(xmlPath);
    });

builder.Services.AddCors(options => options.AddPolicy(
    name: "_myAllowSpecificOrigins",
    builder =>
    {
        builder.WithOrigins("http://localhost:4200", "*").AllowAnyHeader().AllowAnyMethod();
    })
);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
