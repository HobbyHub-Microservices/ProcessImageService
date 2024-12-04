using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageProcessingService.Services.Interfaces;

namespace ImageProcessingService.Services;

public class BlobsManagement : IBlobsManagement
{
    public async Task<string> UploadFile(string containerName, string fileName, byte[] file, string connectionString)
    {
        //create container reference
        var container = new BlobContainerClient(connectionString, containerName);
        await container.CreateIfNotExistsAsync();
        
        //TODO: Set SAS token, for now its available publicly
        await container.SetAccessPolicyAsync(PublicAccessType.Blob);
        
        var blob = container.GetBlobClient(fileName);
        
        //We need to convert the array of bytes
        Stream stream = new MemoryStream(file);
        await blob.UploadAsync(stream);

        return blob.Uri.AbsoluteUri;
    }
}