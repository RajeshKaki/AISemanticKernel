using Azure;
using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Logging;
using PPP.AI.ProjectValidator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PPP.AI.Validators
{
    public class ProjectValidator
    {
        private readonly ILogger _logger;
        private readonly PersistentAgentsClient _agentsClient;
        private readonly PersistentAgent _agent;
        public ProjectValidator(AppSettings settings, ILogger<ProjectValidator> logger, PersistentAgentsClient agentsClient)
        {
            _logger = logger;
            _agentsClient = agentsClient;
            _logger.LogInformation(settings.AzureAI.ProjectEndpoint);
            _agent = agentsClient.Administration.CreateAgent(
                        model: settings.AzureAI.ModelDeploymentName,
                        name: $"ProjectValidator-{Guid.NewGuid().ToString()}",
                        instructions: @"You are a Project release planner who validates project configuration that is provided by the user in a text file.
                                        First row in the text file contains column names seperated by comma and subsequent rows contain values which are also seperated by comma.
                                        Validate the values provided in the text file based on below rules and return error if there are any validation errors.
                                        Rule 1: Product Name column should not be empty. If any row is empty then return failed with Rule number and messsage as Product Name should not be empty.
                                        Rule 2: PKPN column should be unique. If there are duplicates return failed Rule number and message as PKPN should be unique.
                                        Rule 3: There should be no overlapping between Start No and End No across the project. If there is overlapping then return failed with Rule number and message.
                                        After validating all rules, Return the result in below JSON format only.
                                        { 
                                            ""ValidationResult"": ""Passed/Failed"",
                                            ""Errors"": [ { ""Rule"": ""Rule number"", ""Message"": ""Error message"", ""Failed at"": ""Row No"" } ]
                                        }
                                       "
    ).Value;
        }

        public ValidationResult Validate()
        {
            ValidationResult result = new ValidationResult();
            bool fileUploaded = false;

            // create a new thread
            PersistentAgentThread thread = _agentsClient.Threads.CreateThread();

            string? userInput = string.Empty;
            _logger.LogInformation("Agent Initializing...... ");

            ThreadRun run = _agentsClient.Runs.CreateRun(thread.Id, _agent.Id, 
                                additionalInstructions: @"Greet user with short welcome message and ask user to provide path for project file to validate");
            
               ShowResponse(run, thread);

            while (!fileUploaded)
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
                        continue;
                    }
                    else
                    {
                        fileUploaded = UploadFiles(thread, _agentsClient, _agent, userInput);
                    }
 
                userInput = "File uploaded";
                run = _agentsClient.Runs.CreateRun(thread.Id, _agent.Id, additionalInstructions: "Uploaded project excel. validate the excel against rules.");

                ShowResponse(run, thread);

            }
            return result;
        }

        private void ShowResponse(ThreadRun run, PersistentAgentThread thread)
        {
            while (run.Status != RunStatus.Completed && run.Status != RunStatus.Failed)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
                run = _agentsClient.Runs.GetRun(thread.Id, run.Id);
            }

            Pageable<PersistentThreadMessage> messages = _agentsClient.Messages.GetMessages(threadId: run.ThreadId, run.Id, order: ListSortOrder.Ascending);
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
        }

        private bool UploadFiles(PersistentAgentThread thread, PersistentAgentsClient agentClient, PersistentAgent agent, string filePath)
        {

            // Upload local sample file to the agent
            PersistentAgentFileInfo uploadedAgentFile = agentClient.Files.UploadFile(
                filePath: filePath,
                purpose: PersistentAgentFilePurpose.Agents
            );

#if DEBUG
            _logger.LogInformation($"File uploaded: {uploadedAgentFile.Id}, {uploadedAgentFile.Filename}");
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
            }
                ;
#if DEBUG
            Console.WriteLine($"Vector Store Created: {vectorStore.Id} and Vector Store Status: {vectorStore.Status}");
#endif
            ToolResources resources = new ToolResources();
            resources.FileSearch = new FileSearchToolResource();
            resources.FileSearch.VectorStoreIds.Add(vectorStore.Id);
            agentClient.Administration.UpdateAgent(agent.Id, tools: new List<ToolDefinition>() { new FileSearchToolDefinition() }, toolResources: resources);
            //agentClient.Threads.UpdateThread(thread.Id, resources);



#if DEBUG
            Console.WriteLine($"Vector Store Created: {vectorStore.Id}");
#endif
            return true;
        }
    }
        public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
