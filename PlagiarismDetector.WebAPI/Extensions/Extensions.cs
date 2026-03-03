using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace PlagiarismDetector.WebAPI.Extensions
{
    public static class OpenApiServiceExtensions
    {
        public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
        {
            services.AddOpenApi("v1", options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info = new()
                    {
                        Title = "Plagiarism Detector API",
                        Version = "v1",
                        Description = "Core API para la detección de plagio."
                    };
                    return Task.CompletedTask;
                });
            });

            return services;
        }

        public static WebApplication UseOpenApiServices(this WebApplication app)
        {
            app.MapOpenApi();

            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("Plagiarism Detector API Reference")
                    .WithTheme(ScalarTheme.DeepSpace)
                    .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
            });

            return app;
        }
    }
}