using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


// Create the builder that sets up the web application
var builder = WebApplication.CreateBuilder(args);

// Add controller support so the API can use controllers
builder.Services.AddControllers();

// Enables API endpoint discovery (needed for Swagger)
builder.Services.AddEndpointsApiExplorer();

// Adds Swagger so we can test the API from the browser
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<ConnectionStringOptions>()
    .BindConfiguration("ConnectionStrings")
    .ValidateDataAnnotations()
    .ValidateOnStart();
// Build the application
var app = builder.Build();

// Enable Swagger middleware
// This generates the API documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
// Map controller routes (like /api/positions)
app.MapControllers();

// Start the web server
app.Run();