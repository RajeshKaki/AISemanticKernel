
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PPP.AI.Validators;
namespace PPP.AI.ProjectValidator
{
    static class StartUp
    {
        public static ServiceProvider ConfigureServices()
        {
            // Initialize service collection if not provided
            IServiceCollection services= new ServiceCollection();

            // Add logging services
            services.AddLogging(configure => 
            {
                configure.AddConsole();
                //configure.AddDebug();
                configure.SetMinimumLevel(LogLevel.Information);
            });
           
           IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            AppSettings appSettings = new AppSettings();
            var section = configuration.GetSection("AzureAI");
                section.Bind(appSettings.AzureAI);
            services.AddSingleton(appSettings);

            services.AddSingleton<PersistentAgentsClient>(sp =>
            {
                return new PersistentAgentsClient(appSettings.AzureAI.ProjectEndpoint,
                new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions() { TenantId = appSettings.AzureAI.TenantId }));
            });
            // Register ProjectValidator as a service
            services.AddSingleton<PPP.AI.Validators.ProjectValidator>();

            return services.BuildServiceProvider();
        }

    }
}
