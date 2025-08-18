using AIFounday_SemanticKernel_1;
using AIFounday_SemanticKernel_1.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

// Create and configure services
var serviceProvider = ConfigureServices();

// Get the document analyzer service
var documentAnalyzer = serviceProvider.GetRequiredService<IDocumentAnalyzerService>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

Console.WriteLine("=== AI Document Analysis Agent ===");
Console.WriteLine("This application can analyze documents and answer questions about them.");
Console.WriteLine();

while (true)
{
    Console.WriteLine("\nOptions:");
    Console.WriteLine("1. Analyze a document");
    Console.WriteLine("2. Summarize a document");
    Console.WriteLine("3. Exit");
    Console.Write("\nSelect an option (1-3): ");
    
    var option = Console.ReadLine();
    
    switch (option)
    {
        case "1":
            await AnalyzeDocumentAsync(documentAnalyzer, logger);
            break;
        case "2":
            await SummarizeDocumentAsync(documentAnalyzer, logger);
            break;
        case "3":
            Console.WriteLine("Exiting application. Goodbye!");
            return;
        default:
            Console.WriteLine("Invalid option. Please try again.");
            break;
    }
}

static ServiceProvider ConfigureServices()
{
    // Create a new service collection
    var services = new ServiceCollection();

    // Build configuration
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    // Bind configuration to settings
    var settings = new AppSettings();
    configuration.Bind(settings);

    // Check if Azure settings are configured
    if (string.IsNullOrEmpty(settings.AzureAI.Endpoint) || string.IsNullOrEmpty(settings.AzureAI.ApiKey))
    {
        Console.WriteLine("Warning: Azure OpenAI settings are not properly configured in appsettings.json.");
        
        if (string.IsNullOrEmpty(settings.AzureAI.Endpoint))
        {
            Console.Write("Enter your Azure OpenAI Endpoint URL: ");
            settings.AzureAI.Endpoint = Console.ReadLine() ?? "";
        }
        
        if (string.IsNullOrEmpty(settings.AzureAI.ApiKey))
        {
            Console.Write("Enter your Azure OpenAI API Key: ");
            settings.AzureAI.ApiKey = Console.ReadLine() ?? "";
        }
        
        if (string.IsNullOrEmpty(settings.AzureAI.DeploymentName))
        {
            Console.Write("Enter your Azure OpenAI Deployment Name (default: gpt-4o): ");
            var deploymentName = Console.ReadLine();
            if (!string.IsNullOrEmpty(deploymentName))
            {
                settings.AzureAI.DeploymentName = deploymentName;
            }
        }
        
        if (string.IsNullOrEmpty(settings.AzureAI.Endpoint) || string.IsNullOrEmpty(settings.AzureAI.ApiKey))
        {
            throw new InvalidOperationException("Azure OpenAI Endpoint and API Key are required to use this application.");
        }
    }
    else
    {
        Console.WriteLine($"Using Azure OpenAI configuration from appsettings.json");
        Console.WriteLine($"Endpoint: {settings.AzureAI.Endpoint}");
        Console.WriteLine($"Deployment: {settings.AzureAI.DeploymentName}");
    }

    // Add configuration to services
    services.AddSingleton<IConfiguration>(configuration);
    
    // Configure services
    services.ConfigureServices(settings);

    // Build the service provider
    return services.BuildServiceProvider();
}

static async Task AnalyzeDocumentAsync(IDocumentAnalyzerService documentAnalyzer, ILogger logger)
{
    try
    {
        // Get document content
        Console.WriteLine("\n=== Document Analysis ===");
        Console.WriteLine("Enter document content (or path to a text file):");
        var documentInput = Console.ReadLine() ?? "";
        
        string documentContent;
        if (File.Exists(documentInput))
        {
            documentContent = await File.ReadAllTextAsync(documentInput);
            Console.WriteLine($"Loaded document from file: {documentInput}");
        }
        else
        {
            documentContent = documentInput;
        }

        if (string.IsNullOrWhiteSpace(documentContent))
        {
            Console.WriteLine("Document content cannot be empty.");
            return;
        }

        // Get user query
        Console.WriteLine("\nEnter your question about the document:");
        var query = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(query))
        {
            Console.WriteLine("Query cannot be empty.");
            return;
        }

        Console.WriteLine("\nAnalyzing document...");
        var result = await documentAnalyzer.AnalyzeDocumentAsync(documentContent, query);
        
        Console.WriteLine("\n=== Analysis Result ===");
        Console.WriteLine(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during document analysis");
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}

static async Task SummarizeDocumentAsync(IDocumentAnalyzerService documentAnalyzer, ILogger logger)
{
    try
    {
        // Get document content
        Console.WriteLine("\n=== Document Summarization ===");
        Console.WriteLine("Enter document content (or path to a text file):");
        var documentInput = Console.ReadLine() ?? "";
        
        string documentContent;
        if (File.Exists(documentInput))
        {
            documentContent = await File.ReadAllTextAsync(documentInput);
            Console.WriteLine($"Loaded document from file: {documentInput}");
        }
        else
        {
            documentContent = documentInput;
        }

        if (string.IsNullOrWhiteSpace(documentContent))
        {
            Console.WriteLine("Document content cannot be empty.");
            return;
        }

        Console.WriteLine("\nSummarizing document...");
        var result = await documentAnalyzer.SummarizeDocumentAsync(documentContent);
        
        Console.WriteLine("\n=== Summary ===");
        Console.WriteLine(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during document summarization");
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}
