namespace Travelink.Inventory.Services
{
    public interface IMinioService
    {
        Task<string> UploadImageAsync(IFormFile file, string bucketName = "hotel-images");
        Task<bool> DeleteImageAsync(string imageUrl, string bucketName = "hotel-images");
        Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files, string bucketName = "hotel-images");
    }
}
