namespace AIFounday_SemanticKernel_1
{
    public class AppSettings
    {
        public AzureAISettings AzureAI { get; set; } = new AzureAISettings();
    }

    public class AzureAISettings
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string DeploymentName { get; set; } = "gpt-4o";
    }
}