using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Option 1: Simple registration (recommended for most cases)
builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var config = builder.Configuration.GetSection("AzureBlobStorage");
    var accountName = config["AccountName"];
    
    // Use DefaultAzureCredential (best for dev + prod)
    var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");
    return new BlobServiceClient(blobUri, new DefaultAzureCredential());
});

// Option 2: Even cleaner with Microsoft.Extensions.Azure (if you prefer fluent style)
// builder.Services.AddAzureClients(clientBuilder =>
// {
//     clientBuilder.AddBlobServiceClient(new Uri("https://yourstorageaccountname.blob.core.windows.net"))
//                  .WithCredential(new DefaultAzureCredential());
// });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();