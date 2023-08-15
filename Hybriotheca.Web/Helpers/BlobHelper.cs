using Azure.Storage.Blobs;
using Hybriotheca.Web.Helpers.Interfaces;

namespace Hybriotheca.Web.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        readonly BlobServiceClient _blobClient;

        public BlobHelper(IConfiguration configuration)
        {
            _blobClient = new BlobServiceClient(configuration["ConnectionStrings:Blob"]);
        }


        public async Task<Guid> UploadBlobAsync(IFormFile file, string containerName)
        {
            Stream stream = file.OpenReadStream();
            return await UploadStreamAsync(stream, containerName);
        }

        public async Task<Guid> UploadBlobAsync(byte[] image, string containerName)
        {
            var stream = new MemoryStream(image);
            return await UploadStreamAsync(stream, containerName);
        }

        public async Task<Guid> UploadBlobAsync(string image, string containerName)
        {
            Stream stream = File.OpenRead(image);
            return await UploadStreamAsync(stream, containerName);
        }


        async Task<Guid> UploadStreamAsync(Stream stream, string containerName)
        {
            var guid = Guid.NewGuid();

            var container = _blobClient.GetBlobContainerClient(containerName);

            await container.CreateIfNotExistsAsync();

            await container.UploadBlobAsync(guid.ToString(), stream);

            return guid;
        }
    }
}
