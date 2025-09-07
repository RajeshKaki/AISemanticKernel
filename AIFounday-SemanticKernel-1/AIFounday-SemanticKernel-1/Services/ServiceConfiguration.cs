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

            // Configure Semantic Kernel with enhanced agent-like capabilities
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

            // Configure Document Analyzer Agent - Enhanced service with agent-like behavior
            services.AddSingleton<IDocumentAnalyzerAgent, DocumentAnalyzerAgent>();
            services.AddSingleton<IDocumentAnalyzerService, DocumentAnalyzerService>();
            
            return services;
        }
    }

    // Define agent interface for better separation of concerns
    public interface IDocumentAnalyzerAgent
    {
        Task<string> ProcessDocumentAnalysisAsync(string documentContent, string query, string agentRole = "DocumentAnalyzer");
        Task<string> ProcessDocumentSummaryAsync(string documentContent, string agentRole = "DocumentSummarizer");
        Task<AgentResponse> ExecuteAgentTaskAsync(AgentRequest request);
    }

    // Agent request/response models for structured communication
    public class AgentRequest
    {
        public string TaskType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string AgentRole { get; set; } = "DocumentAnalyzer";
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class AgentResponse
    {
        public bool Success { get; set; }
        public string Result { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    // Enhanced Document Analyzer with Agent-like behavior
    public class DocumentAnalyzerAgent : IDocumentAnalyzerAgent
    {
        private readonly Kernel _kernel;
        private readonly ILogger<DocumentAnalyzerAgent> _logger;
        private readonly string _agentId;

        public DocumentAnalyzerAgent(Kernel kernel, ILogger<DocumentAnalyzerAgent> logger)
        {
            _kernel = kernel;
            _logger = logger;
            _agentId = $"DocumentAgent_{Guid.NewGuid():N}";
            
            _logger.LogInformation("Document Analyzer Agent initialized with ID: {AgentId}", _agentId);
        }

        public async Task<string> ProcessDocumentAnalysisAsync(string documentContent, string query, string agentRole = "DocumentAnalyzer")
        {
            _logger.LogInformation("Agent {AgentId} processing document analysis with role: {AgentRole}", _agentId, agentRole);

            var request = new AgentRequest
            {
                TaskType = "DocumentAnalysis",
                Content = documentContent,
                Query = query,
                AgentRole = agentRole
            };

            var response = await ExecuteAgentTaskAsync(request);
            return response.Result;
        }

        public async Task<string> ProcessDocumentSummaryAsync(string documentContent, string agentRole = "DocumentSummarizer")
        {
            _logger.LogInformation("Agent {AgentId} processing document summary with role: {AgentRole}", _agentId, agentRole);

            var request = new AgentRequest
            {
                TaskType = "DocumentSummary",
                Content = documentContent,
                AgentRole = agentRole
            };

            var response = await ExecuteAgentTaskAsync(request);
            return response.Result;
        }

        public async Task<AgentResponse> ExecuteAgentTaskAsync(AgentRequest request)
        {
            try
            {
                _logger.LogInformation("Agent {AgentId} executing task: {TaskType}", _agentId, request.TaskType);

                string prompt = request.TaskType switch
                {
                    "DocumentAnalysis" => CreateAnalysisPrompt(request),
                    "DocumentSummary" => CreateSummaryPrompt(request),
                    _ => throw new ArgumentException($"Unsupported task type: {request.TaskType}")
                };

                var arguments = new KernelArguments
                {
                    ["documentContent"] = request.Content,
                    ["query"] = request.Query ?? string.Empty,
                    ["agentRole"] = request.AgentRole
                };

                var result = await _kernel.InvokePromptAsync(prompt, arguments);
                
                return new AgentResponse
                {
                    Success = true,
                    Result = result.GetValue<string>() ?? "No result generated.",
                    AgentId = _agentId,
                    Metadata = new Dictionary<string, object>
                    {
                        ["TaskType"] = request.TaskType,
                        ["AgentRole"] = request.AgentRole,
                        ["ProcessingTime"] = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Agent {AgentId} failed to execute task: {TaskType}", _agentId, request.TaskType);
                
                return new AgentResponse
                {
                    Success = false,
                    Result = $"Agent processing failed: {ex.Message}",
                    AgentId = _agentId,
                    Metadata = new Dictionary<string, object>
                    {
                        ["Error"] = ex.Message,
                        ["TaskType"] = request.TaskType
                    }
                };
            }
        }

        private string CreateAnalysisPrompt(AgentRequest request)
        {
            return $@"
You are an AI Agent with the role of '{request.AgentRole}' specialized in document analysis.

Agent ID: {_agentId}
Task: Document Analysis
Role: {request.AgentRole}

DOCUMENT CONTENT:
{{{{documentContent}}}}

USER QUERY:
{{{{query}}}}

As an AI Agent, please analyze the document and answer the user's query based ONLY on the information provided in the document. 
If the answer cannot be determined from the document, state that clearly.

Provide your response in a structured format with clear reasoning.
";
        }

        private string CreateSummaryPrompt(AgentRequest request)
        {
            return $@"
You are an AI Agent with the role of '{request.AgentRole}' specialized in document summarization.

Agent ID: {_agentId}
Task: Document Summary
Role: {request.AgentRole}

DOCUMENT CONTENT:
{{{{documentContent}}}}

As an AI Agent, please provide a concise summary of the key points in this document.
Use bullet points and structured formatting for clarity.

Your summary should be professional, accurate, and capture the most important information.
";
        }
    }
}