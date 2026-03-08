import os
from openai import AzureOpenAI
from dotenv import load_dotenv

load_dotenv()

api_key = os.getenv("AZURE_OPENAI_API_KEY")
api_version = os.getenv("AZURE_OPENAI_API_VERSION", "2023-07-01-preview")
azure_endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")

missing_vars = [
    name
    for name, value in {
        "AZURE_OPENAI_API_KEY": api_key,
        "AZURE_OPENAI_ENDPOINT": azure_endpoint,
        "AZURE_OPENAI_DEPLOYMENT_NAME": deployment_name,
    }.items()
    if not value
]

if missing_vars:
    missing_list = ", ".join(missing_vars)
    raise ValueError(f"Missing required environment variables: {missing_list}")

client = AzureOpenAI(
    api_key=api_key,
    api_version=api_version,
    azure_endpoint=azure_endpoint,
)

# The secret to multi-turn: a persistent history list
history = [{"role": "system", "content": "You are a helpful assistant."}]

print("--- Chatbot Started (Type 'exit' to quit) ---")
while True:
    user_input = input("You: ")
    if user_input.lower() in ["exit", "quit"]: break

    # 1. Add user message to history
    history.append({"role": "user", "content": user_input})

    # 2. Send the ENTIRE history to Azure
    response = client.chat.completions.create(
        model=deployment_name,
        messages=history
    )

    ai_message = response.choices[0].message.content
    print(f"AI: {ai_message}")

    # 3. Add AI's response to history so it remembers next time
    history.append({"role": "assistant", "content": ai_message})