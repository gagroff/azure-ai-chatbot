import os
from pathlib import Path

from dotenv import load_dotenv
from fastapi import FastAPI
from pydantic import BaseModel
from openai import AzureOpenAI

BASE_DIR = Path(__file__).resolve().parent
load_dotenv(BASE_DIR / ".env")

app = FastAPI()

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

# This is your Pydantic model - it defines what the "Input" must look like
class ChatRequest(BaseModel):
    message: str

@app.get("/health")
def health_check():
    return {"status": "healthy"}

@app.post("/chat")
def chat_endpoint(request: ChatRequest):
    response = client.chat.completions.create(
        model=deployment_name,
        messages=[{"role": "user", "content": request.message}]
    )
    return {"response": response.choices[0].message.content}