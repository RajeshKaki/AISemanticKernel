namespace AIFoundry_Agents
{
    public class AppSettings
    {
        public AzureAISettings AzureAI { get; set; } = new AzureAISettings();
    }

    public class AzureAISettings
    {
        public string ProjectEndpoint { get; set; } = string.Empty;
        public string ModelDeploymentName { get; set; } = "gpt-4o";
        public string TenantId { get; set; } = string.Empty;
    }
}