using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AIFounday_SemanticKernel_1
{
    public class DocumentAnalyzerPlugin
    {
        [KernelFunction, Description("Analyzes a document to extract key information")]
        public async Task<string> AnalyzeDocument(
            [Description("The content of the document to analyze")] string documentContent,
            [Description("The specific question or information to extract from the document")] string query,
            Kernel kernel)
        {
            // Create a prompt for document analysis
            var prompt = @$"
You are an AI assistant that specializes in document analysis.

DOCUMENT:
{{documentContent}}

QUERY:
{{query}}

Analyze the document carefully and respond to the query based ONLY on the information in the document.
If the information isn't in the document, clearly state that you cannot find that information.
";

            // Create the arguments for the prompt
            var arguments = new KernelArguments
            {
                ["documentContent"] = documentContent,
                ["query"] = query
            };

            // Execute the prompt
            var result = await kernel.InvokePromptAsync(prompt, arguments);
            return result.GetValue<string>() ?? "Failed to analyze the document.";
        }

        [KernelFunction, Description("Summarizes a document")]
        public async Task<string> SummarizeDocument(
            [Description("The content of the document to summarize")] string documentContent,
            Kernel kernel)
        {
            // Create a prompt for document summarization
            var prompt = @$"
You are an AI assistant that specializes in summarizing documents.

DOCUMENT:
{{documentContent}}

Provide a concise summary of the key points in this document in a structured format with bullet points.
";

            // Create the arguments for the prompt
            var arguments = new KernelArguments
            {
                ["documentContent"] = documentContent
            };

            // Execute the prompt
            var result = await kernel.InvokePromptAsync(prompt, arguments);
            return result.GetValue<string>() ?? "Failed to summarize the document.";
        }
    }
}