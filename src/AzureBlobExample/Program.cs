using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobExample
{
    class Program
    {
        private static readonly string _connectionString = "";
        private static readonly string _blobContainerName = "sentences";

        static async Task Main(string[] args)
        {
            Console.WriteLine("This is a sample application to demonstrate how to use Azure Blob");
            Console.WriteLine("Uploading blob...");

            var blobName = await UploadBlobAsync();
            Console.WriteLine($"Blob has been created : {blobName}");

            Console.WriteLine($"Listing containers...");
            await ListContainerFilesAsync();

            Console.WriteLine($"Downloading blob {blobName} to sentence.txt...");
            await DownloadBlobAsync(blobName);

            Console.WriteLine($"Setting blob properties for {blobName}...");
            await SetBlobPropertiesAsync(blobName);

            Console.WriteLine($"Getting blob properties for {blobName}...");
            await GetBlobPropertiesAsync(blobName);

            Console.WriteLine($"Setting metadata for {blobName}...");
            await SetBlobMetadataAsync(blobName);

            Console.WriteLine($"Getting metadata for {blobName}...");
            await GetBlobMetadataAsync(blobName);

            Console.WriteLine($"Deleting blob {blobName}");
            await DeleteBlobAsync(blobName);

            Console.WriteLine($"Deleting container {_blobContainerName} ...");
            await DeleteContainerAsync();

            Console.WriteLine("Finished...");
        }


        private static async Task<string> UploadBlobAsync()
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);

            var blobName = Guid.NewGuid().ToString() + ".txt";
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_blobContainerName);

            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var sentence = "You can never plan the future by the past.";
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(sentence));

            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = "text/plain" });
            return blobName;
        }

        private static async Task ListContainerFilesAsync()
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            
            await foreach (var blobContainerItem in blobServiceClient.GetBlobContainersAsync())
            {
                Console.WriteLine($"\t{blobContainerItem.Name}");

                var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerItem.Name);

                await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
                {
                    Console.WriteLine($"\t - {blobItem.Name}");
                }
            }
        }

        private static async Task DownloadBlobAsync(string blobName)
        {
            string localFileName = "sentence.txt";

            BlobClient blobClient = new BlobClient(_connectionString, _blobContainerName, blobName);
            bool exists = await blobClient.ExistsAsync();
            if (exists)
            {
                BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();

                using FileStream fileStream = File.OpenWrite(localFileName);
                await blobDownloadInfo.Content.CopyToAsync(fileStream);
            }
        }

        private static async Task DeleteBlobAsync(string blobName)
        {
            BlobClient blobClient = new BlobClient(_connectionString, _blobContainerName, blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        private static async Task DeleteContainerAsync()
        {
            BlobContainerClient blobContainerClient = new BlobContainerClient(_connectionString, _blobContainerName);

            await blobContainerClient.DeleteIfExistsAsync();
        }

        private static async Task SetBlobPropertiesAsync(string blobName)
        {
            var blobClient = new BlobClient(_connectionString, _blobContainerName, blobName);

            BlobProperties blobProperties = await blobClient.GetPropertiesAsync();

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentLanguage = "en-us",

                CacheControl = blobProperties.CacheControl,
                ContentDisposition = blobProperties.ContentDisposition,
                ContentEncoding = blobProperties.ContentEncoding,
                ContentHash = blobProperties.ContentHash
            };

            await blobClient.SetHttpHeadersAsync(blobHttpHeaders);
        }

        private static async Task GetBlobPropertiesAsync(string blobName)
        {
            var blobClient = new BlobClient(_connectionString, _blobContainerName, blobName);

            BlobProperties blobProperties = await blobClient.GetPropertiesAsync();

            Console.WriteLine($"\t- ContentLanguage: {blobProperties.ContentLanguage}");
            Console.WriteLine($"\t- ContentType: {blobProperties.ContentType}");
            Console.WriteLine($"\t- Blob type: {blobProperties.BlobType}");
            Console.WriteLine($"\t- CreatedOn: {blobProperties.CreatedOn}");
            Console.WriteLine($"\t- LastModified: {blobProperties.LastModified}");
        }

        private static async Task SetBlobMetadataAsync(string blobName)
        {
            var blobClient = new BlobClient(_connectionString, _blobContainerName, blobName);

            var metadata = new Dictionary<string, string>();

            metadata.Add("author", "anonymous");
            metadata.Add("date", "unkown");

            await blobClient.SetMetadataAsync(metadata);
        }

        private static async Task GetBlobMetadataAsync(string blobName)
        {
            var blobClient = new BlobClient(_connectionString, _blobContainerName, blobName);

            BlobProperties blobProperties = await blobClient.GetPropertiesAsync();

            foreach (var metadataItem in blobProperties.Metadata)
            {
                Console.WriteLine($"\t{metadataItem.Key} : {metadataItem.Value}");
            }
        }
    }
}
