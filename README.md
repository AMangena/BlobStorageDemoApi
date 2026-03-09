# Blob Storage Demo API

A simple ASP.NET Core Web API for interacting with Azure Blob Storage.  
Supports uploading files with custom metadata, listing blobs, downloading, and deleting files.

Built with .NET 8/9, Azure.Storage.Blobs, and passwordless authentication using DefaultAzureCredential.

## Features

- List all blobs in a container (with metadata, size, last modified, content type)
- Upload files (with optional virtual folder and custom metadata like uploadedBy, location, timestamp)
- Download blobs as files
- Delete blobs (including snapshots if present)
- Auto-creates container if missing
- Swagger UI for interactive testing
- Passwordless auth (DefaultAzureCredential: Azure CLI locally, managed identity in Azure)

## Technologies

- ASP.NET Core Web API (.NET 8 / .NET 9)
- Azure.Storage.Blobs (v12+)
- Azure.Identity (for DefaultAzureCredential)
- Swashbuckle.AspNetCore (Swagger/OpenAPI)

## Prerequisites

- .NET 8 or 9 SDK
- Azure Storage Account
- Azure CLI (`az login`) for local development
- (Optional) Visual Studio Code or Visual Studio

