using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AIFounday_SemanticKernel_1.Services;

namespace AIFounday_SemanticKernel_1
{
    public class DocumentAnalyzerService(Kernel kernel, IDocumentAnalyzerAgent documentAnalyzerAgent, ILogger<DocumentAnalyzerService> logger) : IDocumentAnalyzerService
    {
        private readonly Kernel _kernel = kernel;
        private readonly IDocumentAnalyzerAgent _documentAnalyzerAgent = documentAnalyzerAgent;
        private readonly ILogger<DocumentAnalyzerService> _logger = logger;

        public async Task<string> AnalyzeDocumentAsync(string documentContent, string query)
        {
            try
            {
                _logger.LogInformation("DocumentAnalyzerService: Delegating analysis to AI Agent");

                // Use the AI Agent for document analysis
                var result = await _documentAnalyzerAgent.ProcessDocumentAnalysisAsync(
                    documentContent, 
                    query, 
                    "CarInsuranceAnalyzer");

                _logger.LogInformation("DocumentAnalyzerService: Analysis completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentAnalyzerService: Error analyzing document");
                return $"An error occurred while analyzing the document: {ex.Message}";
            }
        }

        public async Task<string> SummarizeDocumentAsync(string documentContent)
        {
            try
            {
                _logger.LogInformation("DocumentAnalyzerService: Delegating summarization to AI Agent");

                // Use the AI Agent for document summarization
                var result = await _documentAnalyzerAgent.ProcessDocumentSummaryAsync(
                    documentContent, 
                    "DocumentSummarizer");

                _logger.LogInformation("DocumentAnalyzerService: Summarization completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentAnalyzerService: Error summarizing document");
                return $"An error occurred while summarizing the document: {ex.Message}";
            }
        }

        // Additional method to demonstrate advanced agent usage
        public async Task<string> PerformAdvancedAnalysisAsync(string documentContent, string analysisType, Dictionary<string, object>? parameters = null)
        {
            try
            {
                _logger.LogInformation("DocumentAnalyzerService: Performing advanced analysis with type: {AnalysisType}", analysisType);

                var agentRequest = new AgentRequest
                {
                    TaskType = analysisType,
                    Content = documentContent,
                    AgentRole = "AdvancedDocumentAnalyzer",
                    Parameters = parameters ?? new Dictionary<string, object>()
                };

                var response = await _documentAnalyzerAgent.ExecuteAgentTaskAsync(agentRequest);
                
                if (response.Success)
                {
                    _logger.LogInformation("DocumentAnalyzerService: Advanced analysis completed by agent {AgentId}", response.AgentId);
                    return response.Result;
                }
                else
                {
                    _logger.LogWarning("DocumentAnalyzerService: Advanced analysis failed: {Result}", response.Result);
                    return response.Result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentAnalyzerService: Error in advanced analysis");
                return $"An error occurred during advanced analysis: {ex.Message}";
            }
        }
    }
}