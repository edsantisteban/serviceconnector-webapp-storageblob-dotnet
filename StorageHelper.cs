using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace WebStorageSample
{
    public class StorageHelper
    {
        private static TokenCredential GetCredential()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return env == "Development"
                ? new AzureCliCredential()          // local: usa tu sesión de az login
                : new DefaultAzureCredential();     // en Azure: usa la identidad administrada
        }

        // Sube datos binarios y rechaza nombres de blob ya existentes de forma atómica.
        static public async Task UploadBlob(string containerEndpoint, string containerName, string blobName,
            Stream contents, CancellationToken cancellationToken = default)
        {
            var blobContainerUri = new Uri(new Uri(containerEndpoint), containerName);
            var containerClient = new BlobContainerClient(blobContainerUri, GetCredential());
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(contents, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "application/octet-stream" },
                Conditions = new BlobRequestConditions { IfNoneMatch = Azure.ETag.All }
            }, cancellationToken);
        }

        static public async Task UploadBlob(string containerEndpoint, string containerName, string blobName, string blobContents)
        {
            var blobContainerUri = new Uri(new Uri(containerEndpoint), containerName);
            BlobContainerClient containerClient = new BlobContainerClient(blobContainerUri, GetCredential());

            try
            {
                await containerClient.CreateIfNotExistsAsync();

                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                byte[] byteArray = Encoding.ASCII.GetBytes(blobContents);

                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        static public async Task<string> GetBlob(string containerEndpoint, string containerName, string blobName)
        {
            var blobContainerUri = new Uri(new Uri(containerEndpoint), containerName);
            BlobContainerClient containerClient = new BlobContainerClient(blobContainerUri, GetCredential());

            try
            {
                await containerClient.CreateIfNotExistsAsync();

                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                if (await blobClient.ExistsAsync())
                {
                    var response = await blobClient.DownloadAsync();
                    using (var streamReader = new StreamReader(response.Value.Content))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            var line = await streamReader.ReadLineAsync();
                            Console.WriteLine(line);
                            return line;
                        }
                    }
                }
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}