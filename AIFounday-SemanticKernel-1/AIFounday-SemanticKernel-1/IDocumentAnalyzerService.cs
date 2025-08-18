namespace AIFounday_SemanticKernel_1
{
    public interface IDocumentAnalyzerService
    {
        Task<string> AnalyzeDocumentAsync(string documentContent, string query);
        Task<string> SummarizeDocumentAsync(string documentContent);
    }
}