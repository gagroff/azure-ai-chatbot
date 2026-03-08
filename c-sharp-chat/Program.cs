using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

LoadDotEnvFiles();

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

string endpoint = configuration["AzureOpenAI:Endpoint"]
    ?? configuration["AZURE_OPENAI_ENDPOINT"]
    ?? throw new InvalidOperationException("Missing Azure OpenAI endpoint. Set AzureOpenAI:Endpoint with dotnet user-secrets or AZURE_OPENAI_ENDPOINT as an environment variable.");

string key = configuration["AzureOpenAI:Key"]
    ?? configuration["AZURE_OPENAI_API_KEY"]
    ?? throw new InvalidOperationException("Missing Azure OpenAI API key. Set AzureOpenAI:Key with dotnet user-secrets or AZURE_OPENAI_API_KEY as an environment variable.");

string deploymentName = configuration["AzureOpenAI:DeploymentName"]
    ?? configuration["AZURE_OPENAI_DEPLOYMENT_NAME"]
    ?? throw new InvalidOperationException("Missing Azure OpenAI deployment name. Set AzureOpenAI:DeploymentName with dotnet user-secrets or AZURE_OPENAI_DEPLOYMENT_NAME as an environment variable.");

var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
ChatClient chatClient = azureClient.GetChatClient(deploymentName);

var history = new List<ChatMessage>
{
    new SystemChatMessage("You are a helpful assistant.")
};

Console.WriteLine("--- Chatbot Started (Type 'exit' to quit) ---");
while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(input, "quit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    history.Add(new UserChatMessage(input));

    ChatCompletion response = await chatClient.CompleteChatAsync(history);
    string aiResponse = response.Content[0].Text;

    Console.WriteLine($"AI: {aiResponse}");
    history.Add(new AssistantChatMessage(aiResponse));
}

static void LoadDotEnvFiles()
{
    string currentDirectory = Directory.GetCurrentDirectory();
    string? parentDirectory = Directory.GetParent(currentDirectory)?.FullName;

    var candidateFiles = new[]
    {
        Path.Combine(currentDirectory, ".env"),
        Path.Combine(currentDirectory, "c-sharp-chat", ".env"),
        Path.Combine(currentDirectory, "python-chat", ".env"),
        parentDirectory is null ? null : Path.Combine(parentDirectory, ".env"),
        parentDirectory is null ? null : Path.Combine(parentDirectory, "python-chat", ".env")
    };

    foreach (string filePath in candidateFiles.Where(path => !string.IsNullOrWhiteSpace(path)).Distinct()!)
    {
        if (!File.Exists(filePath))
        {
            continue;
        }

        foreach (string rawLine in File.ReadAllLines(filePath))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            int separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            string name = line[..separatorIndex].Trim();
            string value = line[(separatorIndex + 1)..].Trim().Trim('"');

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }
}