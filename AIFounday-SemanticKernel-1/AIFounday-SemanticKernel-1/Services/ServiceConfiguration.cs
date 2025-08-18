using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AIFounday_SemanticKernel_1.Services
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, AppSettings settings)
        {
            // Add logging
            services.AddLogging(configure => configure.AddConsole());

            // Configure Semantic Kernel
            services.AddSingleton(sp =>
            {
                var builder = Kernel.CreateBuilder()
                    .AddAzureOpenAIChatCompletion(
                        deploymentName: settings.AzureAI.DeploymentName,
                        endpoint: settings.AzureAI.Endpoint,
                        apiKey: settings.AzureAI.ApiKey);

                // Add plugins as needed
                builder.Plugins.AddFromType<DocumentAnalyzerPlugin>();

                // Add logging
                builder.Services.AddLogging(configure => configure.AddConsole());

                return builder.Build();
            });

            services.AddSingleton<IDocumentAnalyzerService, DocumentAnalyzerService>();
            return services;
        }
    }
}