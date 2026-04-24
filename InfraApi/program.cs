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

// Build the application
var app = builder.Build();

// Enable Swagger middleware
// This generates the API documentation
app.UseSwagger();

// Enable the Swagger UI (the webpage where we test endpoints)
app.UseSwaggerUI();

// Map controller routes (like /api/positions)
app.MapControllers();

// Start the web server
app.Run();