using API.Data;
using API.Extensions;
using API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddIdentityServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed random data
using var scope = app.Services.CreateScope();
var serviceProviders = scope.ServiceProvider;
try 
{
    var context = serviceProviders.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedUsersAsync(context);
} 
catch (Exception exception)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(exception, "An error occured during migration");
}

await app.RunAsync();