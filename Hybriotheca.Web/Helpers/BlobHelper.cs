using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Hybriotheca.Web.Helpers.Interfaces;

namespace Hybriotheca.Web.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        readonly BlobServiceClient _blobClient;
		private readonly IConfiguration _configuration;

		public BlobHelper(IConfiguration configuration)
        {
            _blobClient = new BlobServiceClient(configuration["ConnectionStrings:Blob"]);
			_configuration = configuration;
		}


        public async Task DeleteBlobAsync(string blobName, string containerName)
        {
            var container = await GetContainerAsync(containerName);

            await container.DeleteBlobAsync(blobName);
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


        async Task<BlobContainerClient> GetContainerAsync(string containerName)
        {
            var container = _blobClient.GetBlobContainerClient(containerName);

            await container.CreateIfNotExistsAsync();

            return container;
        }

        async Task<Guid> UploadStreamAsync(Stream stream, string containerName)
        {
            var guid = Guid.NewGuid();

            var container = await GetContainerAsync(containerName);

            await container.UploadBlobAsync(guid.ToString(), stream);

            stream.Close();

            return guid;
        }

        public async Task<Guid> UploadEPUBAsync(IFormFile file, string containerName)
        {
            Stream stream = file.OpenReadStream();

            var guid = Guid.NewGuid();

            var container = _blobClient.GetBlobContainerClient(containerName);

            await container.CreateIfNotExistsAsync();

            var blobClient = container.GetBlobClient(guid.ToString() + ".epub");

            var blobUploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "application/epub+zip"
                }
            };

            await blobClient.UploadAsync(stream, blobUploadOptions);

            stream.Close();

            return guid;
        }

        public async Task DeleteEPUBAsync(string ePubName, string containerName)
        {
            var container = _blobClient.GetBlobContainerClient(containerName);

            var blobClient = container.GetBlobClient(ePubName + ".epub");

            await blobClient.DeleteAsync();
        }


    }
}
