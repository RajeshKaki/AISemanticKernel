// See https://aka.ms/new-console-template for more information
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

Console.WriteLine("Hello, World!");
var builder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var agentsClient = AzureAIAgent.CreateAgentsClient("https://ai-foundry-002.services.ai.azure.com/api/projects/Analysis-project", new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions() { TenantId = "3d02bcad-0100-4e81-8e0b-bea672b7e7a6" }));
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var carInsuranceAgent = await agentsClient.Administration.CreateAgentAsync(
     model: "gpt-5-chat",
     name: "CarInsuranceAdvisor",
     instructions: @"You are a helpful car insurance advisor.
                     Your role is to review and analyze multiple insurance documents uploaded by the user from different providers.
                     You will compare the details across all documents and provide clear, accurate answers to any questions from the user.",
     description: "An agent that assists users with car insurance inquiries."
     );

var weatherAgent = await agentsClient.Administration.CreateAgentAsync(
     model: "gpt-5-chat",
     name: "WeatherAdvisor",
     instructions: "You are an agent designed to query and retrieve information about the weather of a given location. If you have been asked to provide information about two distinguish locations then you need to get the information appropriately. For now, just make it up, you do not have to call any service. Provide your response in the following JSON schema: {\"location\": \"\", \"temperature\": \"\", \"humidity\": \"\"}"
     );
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
AzureAIAgent agent = new(weatherAgent, agentsClient);
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
AzureAIAgentThread thread = new AzureAIAgentThread(agentsClient);
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var responses = agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, "What's the weather of Sydney right now?"), thread);

await foreach (var response in responses)
{
    Console.WriteLine($"{response.Message.Content}: {response.Message.Role}");
}


