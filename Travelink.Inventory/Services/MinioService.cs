using Minio;
using Minio.DataModel.Args;

namespace Travelink.Inventory.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _configuration;

        public MinioService(IMinioClient minioClient, IConfiguration configuration)
        {
            _minioClient = minioClient;
            _configuration = configuration;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string bucketName = "hotel-images")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("El archivo está vacío");

            // Validar que sea una imagen
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Formato de imagen no válido");

            // Crear el bucket si no existe
            await EnsureBucketExistsAsync(bucketName);

            // Generar nombre único para el archivo
            var fileName = $"{Guid.NewGuid()}{extension}";

            // Subir archivo
            using (var stream = file.OpenReadStream())
            {
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType);

                await _minioClient.PutObjectAsync(putObjectArgs);
            }

            // Retornar URL de acceso
            var minioEndpoint = _configuration["MinIO:Endpoint"];
            var minioPort = _configuration["MinIO:Port"];
            return $"http://{minioEndpoint}:{minioPort}/{bucketName}/{fileName}";
        }

        public async Task<bool> DeleteImageAsync(string imageUrl, string bucketName = "hotel-images")
        {
            try
            {
                // Extraer el nombre del archivo de la URL
                var fileName = imageUrl.Split('/').Last();

                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files, string bucketName = "hotel-images")
        {
            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var url = await UploadImageAsync(file, bucketName);
                    uploadedUrls.Add(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error subiendo {file.FileName}: {ex.Message}");
                }
            }

            return uploadedUrls;
        }

        private async Task EnsureBucketExistsAsync(string bucketName)
        {
            var bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(bucketName);

            bool found = await _minioClient.BucketExistsAsync(bucketExistsArgs);

            if (!found)
            {
                var makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(bucketName);

                await _minioClient.MakeBucketAsync(makeBucketArgs);

                // Hacer el bucket público para lectura
                var policy = $@"{{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {{
                            ""Effect"": ""Allow"",
                            ""Principal"": {{""AWS"": [""*""]}},
                            ""Action"": [""s3:GetObject""],
                            ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
                        }}
                    ]
                }}";

                var setPolicyArgs = new SetPolicyArgs()
                    .WithBucket(bucketName)
                    .WithPolicy(policy);

                await _minioClient.SetPolicyAsync(setPolicyArgs);
            }
        }
    }
}
