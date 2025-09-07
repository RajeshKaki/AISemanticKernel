using AIFoundry_Agents;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;

var settings = new AppSettings();
bool fileUploaded = false;
var builder = Host.CreateApplicationBuilder();
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                     .AddEnvironmentVariables()
                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Configuration.Bind(settings);
string projectEndpoint = settings.AzureAI.ProjectEndpoint;
string modelDeploymentName = settings.AzureAI.ModelDeploymentName;
string tenantId = settings.AzureAI.TenantId;
#if DEBUG
Console.WriteLine($"Using Project Endpoint: {projectEndpoint}");
Console.WriteLine($"Using Model Deployment: {modelDeploymentName}");
Console.WriteLine($"Using Tenant ID: {tenantId}");
#endif
// Register PersistentAgentsClient as a singleton service
builder.Services.AddSingleton(sp =>
{
    // create persistent Agents client
    return new PersistentAgentsClient(
        settings.AzureAI.ProjectEndpoint,
        new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions() { TenantId = settings.AzureAI.TenantId }));
        
});
IHost host1 = builder.Build();

// Use the PersistentAgentsClient service
var agentsClient = host1.Services.GetRequiredService<PersistentAgentsClient>();

// check if the agent already exists
PersistentAgent? agent = null; // agentsClient.Administration.GetAgent("asst_kiXj9ErYUinX9ptoD5cXNL40").Value;
if (agent == null)
{
    // create a new agent
    agent = agentsClient.Administration.CreateAgent(
        model: modelDeploymentName,
        name: $"CarInsuranceAdvisor-{Guid.NewGuid().ToString()}",
        instructions: @"You are a knowledgeable and friendly car insurance advisor.
                        Your role is to review and analyze the insurance documents uploaded by the user and answer user queries."
    ).Value;
    #if DEBUG
    Console.WriteLine($"Agent created successfully: {agent.Id}");
    #endif
}
else
{
    #if DEBUG
    Console.WriteLine($"Agent already exists: {agent.Id}");
    #endif
}

// create a new thread
PersistentAgentThread thread = agentsClient.Threads.CreateThread();

string? userInput = string.Empty;
Console.WriteLine("Agent Initializing...... ");

ThreadRun run = agentsClient.Runs.CreateRun(thread.Id, agent.Id, additionalInstructions: @"Greet user with short welcome message and ask user to provide path to analyse the insurance documents");
 // Console.WriteLine($"Run Id: {run.Id}");
while (string.Compare(userInput,"Exit",true) != 0)
{
    while (run.Status == RunStatus.Queued
        || run.Status == RunStatus.InProgress
        || run.Status == RunStatus.RequiresAction)
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(500));
        run = agentsClient.Runs.GetRun(thread.Id, run.Id);
    }
    #if DEBUG
        Console.WriteLine(run.Status);
    #endif
    Pageable<PersistentThreadMessage> messages = agentsClient.Messages.GetMessages(threadId: thread.Id,run.Id,order: ListSortOrder.Ascending);
    // Console.WriteLine($"Total Messages in run {run.Id}: {messages.Count()}");
    foreach (PersistentThreadMessage message in messages)
    {
        foreach (MessageContent content in message.ContentItems)
        {
            MessageTextContent textContent = (MessageTextContent)content;
            if (textContent != null)
            {
                Console.WriteLine(value: $" {message.Role.ToString().ToUpper()} : {textContent.Text}");
            }
        }
    }

    if(!fileUploaded)
    {
        Console.Write($"Enter File Path: ");
        userInput = Console.ReadLine();
        if (string.Compare(userInput, "Exit", true) == 0)
        {
            break;
        }
        if (string.IsNullOrWhiteSpace(userInput) || !File.Exists(userInput))
        {
            Console.WriteLine("Please provide valid file path or type 'Exit' to quit.");
        }
        else
        {
           fileUploaded =  UploadFiles(thread,agentsClient,agent, userInput);           
        }
        userInput = "File uploaded";
        run = agentsClient.Runs.CreateRun(thread.Id, agent.Id, additionalInstructions: "Uploaded car insurance document. Analyze the content and answer to the user questions.");
        continue;
    }
    else
    {
        Console.Write("User Query: ");
        userInput = Console.ReadLine();
    }
  // Console.WriteLine("Sending message to agent.");
    PersistentThreadMessage Message = agentsClient.Messages.CreateMessage(thread.Id, MessageRole.User, content: userInput);
    run = agentsClient.Runs.CreateRun(thread.Id, agent.Id);

}

Console.WriteLine(" ##################################");
Console.WriteLine("Printing Conversation History");
Pageable<PersistentThreadMessage> finalMessages = agentsClient.Messages.GetMessages(thread.Id,order: ListSortOrder.Ascending);
Console.WriteLine($"Count: {finalMessages.Count()}");

static bool UploadFiles(PersistentAgentThread thread,PersistentAgentsClient agentClient,PersistentAgent agent, string filePath)
{

    // Upload local sample file to the agent
    PersistentAgentFileInfo uploadedAgentFile = agentClient.Files.UploadFile(
        filePath: filePath,
        purpose: PersistentAgentFilePurpose.Agents
    );
   
#if DEBUG
    Console.WriteLine($"File uploaded: {uploadedAgentFile.Id}, {uploadedAgentFile.Filename}");
#endif
    var uploadedFileIds = new List<string>();
      uploadedFileIds.Add(uploadedAgentFile.Id);

    PersistentAgentsVectorStore vectorStore = agentClient.VectorStores.CreateVectorStore(
        fileIds: uploadedFileIds,
        name: $"my_vectorstore_{Guid.NewGuid().ToString()}");
    while (vectorStore.Status == VectorStoreStatus.Expired
        || vectorStore.Status == VectorStoreStatus.InProgress)
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(500));
        vectorStore = agentClient.VectorStores.GetVectorStore(vectorStore.Id).Value;
    };
    #if DEBUG
    Console.WriteLine($"Vector Store Created: {vectorStore.Id} and Vector Store Status: {vectorStore.Status}");
    #endif
    ToolResources resources = new ToolResources();
    resources.FileSearch = new FileSearchToolResource();
    resources.FileSearch.VectorStoreIds.Add(vectorStore.Id);
    agentClient.Administration.UpdateAgent(agent.Id, tools: new List<ToolDefinition>() { new FileSearchToolDefinition() },toolResources: resources);
    //agentClient.Threads.UpdateThread(thread.Id, resources);
  
    

#if DEBUG
    Console.WriteLine($"Vector Store Created: {vectorStore.Id}");
#endif
    return true;
}