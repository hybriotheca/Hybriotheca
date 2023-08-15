namespace Hybriotheca.Web.Helpers.Interfaces
{
    public interface IBlobHelper
    {
        Task<Guid> UploadBlobAsync(byte[] image, string containerName);
        Task<Guid> UploadBlobAsync(IFormFile file, string containerName);
        Task<Guid> UploadBlobAsync(string image, string containerName);
    }
}