using PlagiarismDetector.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApiServices(); 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApiServices(); 
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();