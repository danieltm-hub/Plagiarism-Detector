using PlagiarismDetector.WebAPI.Extensions;
using PlagiarismDetector.Application.Interfaces;
using PlagiarismDetector.Application.UseCases;
using PlagiarismDetector.Infrastructure;

using Microsoft.Build.Locator;

MSBuildLocator.RegisterDefaults();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApiServices();

// builder.Services.AddSingleton<IMongoClient>(sp => 
//     new MongoClient(builder.Configuration["MongoDb:ConnectionString"]));

// builder.Services.AddScoped<IMongoDatabase>(sp =>
// {
//     var client = sp.GetRequiredService<IMongoClient>();
//     return client.GetDatabase(builder.Configuration["MongoDb:DatabaseName"]);
// });

string baseStoragePath = builder.Configuration["LocalStorage:BasePath"] 
                         ?? Path.Combine(Path.GetTempPath(), "PlagiarismData");

builder.Services.AddScoped<IStorageRepository>(provider =>
    new LocalFileStorageRepository(baseStoragePath)
);
builder.Services.AddScoped<IListProcessedFilesUseCase, ListProcessedFilesUseCase>();
builder.Services.AddScoped<IProcessFolderUseCase, ProcessFolderUseCase>();
builder.Services.AddScoped<IGetRandomPairsUseCase, GetRandomPairsUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApiServices(); 
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();