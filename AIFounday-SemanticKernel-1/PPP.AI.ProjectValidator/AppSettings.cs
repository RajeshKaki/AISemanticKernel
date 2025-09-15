using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPP.AI.ProjectValidator
{
    public class AppSettings
    {
        public AzureAISettings AzureAI { get; set; } = new AzureAISettings();
    }

    public class AzureAISettings
    {
        public string ProjectEndpoint { get; set; } = string.Empty;
        public string ModelDeploymentName { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
    }
}
