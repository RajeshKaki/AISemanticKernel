using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PPP.AI.ProjectValidator;
using PPP.AI.Validators;

// Configure services using the StartUp class
var serviceProvider = StartUp.ConfigureServices();

// Get logger for the program
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

#if DEBUG
logger.LogInformation("PPP.AI.ProjectValidator started");
#endif

// Get the ProjectValidator service
var projectValidator = serviceProvider.GetRequiredService<ProjectValidator>();

#if DEBUG
logger.LogInformation("PPP.AI.ProjectValidator completed");
#endif

projectValidator.Validate();
// Dispose of the service provider
serviceProvider.Dispose();
