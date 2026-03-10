using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();

var endpoint = builder.Configuration["AzureOpenAI:Endpoint"];
var apiKey = builder.Configuration["AzureOpenAI:ApiKey"];
var deploymentName = builder.Configuration["AzureOpenAI:DeploymentName"];

var missingSettings = new List<string>();

if (string.IsNullOrWhiteSpace(endpoint))
{
    missingSettings.Add("AzureOpenAI:Endpoint");
}

if (string.IsNullOrWhiteSpace(apiKey))
{
    missingSettings.Add("AzureOpenAI:ApiKey");
}

if (string.IsNullOrWhiteSpace(deploymentName))
{
    missingSettings.Add("AzureOpenAI:DeploymentName");
}

if (missingSettings.Count > 0)
{
    throw new InvalidOperationException($"Missing required configuration values: {string.Join(", ", missingSettings)}");
}

var resolvedEndpoint = endpoint!;
var resolvedApiKey = apiKey!;
var resolvedDeploymentName = deploymentName!;

var endpointUri = new Uri(resolvedEndpoint);
var credential = new ApiKeyCredential(resolvedApiKey);
var azureOpenAiClient = new AzureOpenAIClient(endpointUri, credential);
var chatClient = azureOpenAiClient.GetChatClient(resolvedDeploymentName);

var app = builder.Build();

// Health Check
app.MapGet("/health", () => new { status = "healthy" });

// Chat Endpoint
app.MapPost("/chat", async (ChatRequest request) => {
    var completion = await chatClient.CompleteChatAsync(
        [
            new UserChatMessage(request.Message)
        ]);

    var responseText = string.Concat(completion.Value.Content.Select(part => part.Text));
    return Results.Ok(new { response = responseText });
});

app.Run();

// Define the input model
public record ChatRequest(string Message);