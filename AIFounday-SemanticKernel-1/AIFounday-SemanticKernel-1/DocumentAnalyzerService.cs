using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text;

namespace AIFounday_SemanticKernel_1
{
    public class DocumentAnalyzerService : IDocumentAnalyzerService
    {
        private readonly Kernel _kernel;
        private readonly ILogger<DocumentAnalyzerService> _logger;

        public DocumentAnalyzerService(Kernel kernel, ILogger<DocumentAnalyzerService> logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        public async Task<string> AnalyzeDocumentAsync(string documentContent, string query)
        {
            try
            {
                _logger.LogInformation("Analyzing document with query: {Query}", query);

                // Create a prompt for document analysis with the user's query
                var prompt = @$"
You are an AI assistant that analyzes documents and answers questions about them.

DOCUMENT CONTENT:
{{documentContent}}

USER QUERY:
{{query}}

Please analyze the document and answer the user's query based ONLY on the information provided in the document. 
If the answer cannot be determined from the document, state that clearly.
";

                // Create the arguments for the prompt
                var arguments = new KernelArguments
                {
                    ["documentContent"] = documentContent,
                    ["query"] = query
                };

                // Execute the prompt
                var result = await _kernel.InvokePromptAsync(prompt, arguments);
                return result.GetValue<string>() ?? "Unable to analyze the document.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing document");
                return $"An error occurred while analyzing the document: {ex.Message}";
            }
        }

        public async Task<string> SummarizeDocumentAsync(string documentContent)
        {
            try
            {
                _logger.LogInformation("Summarizing document");

                // Create a prompt for document summarization
                var prompt = @$"
You are an AI assistant that summarizes documents.

DOCUMENT CONTENT:
{{documentContent}}

Please provide a concise summary of the key points in this document in bullet points.
";

                // Create the arguments for the prompt
                var arguments = new KernelArguments
                {
                    ["documentContent"] = documentContent
                };

                // Execute the prompt
                var result = await _kernel.InvokePromptAsync(prompt, arguments);
                return result.GetValue<string>() ?? "Unable to summarize the document.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error summarizing document");
                return $"An error occurred while summarizing the document: {ex.Message}";
            }
        }
    }
}