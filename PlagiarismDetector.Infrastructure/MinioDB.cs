// using PlagiarismDetector.Domain.Interfaces;
// using Minio;
// using Minio.DataModel.Args;
// using System.Text;

// namespace PlagiarismDetector.Infrastructure;

// public class MinioStorageRepository : IStorageRepository
// {
//     private readonly IMinioClient _minioClient;
//     public MinioStorageRepository(IMinioClient minioClient)
//     {
//         _minioClient = minioClient;
//     }

//     public async Task UploadFileAsync(string bucketName, string fileName, string jsonContent, CancellationToken cancellationToken = default)
//     {
//         var bktExistArgs = new BucketExistsArgs().WithBucket(bucketName);
//         bool found = await _minioClient.BucketExistsAsync(bktExistArgs, cancellationToken);

//         if (!found)
//         {
//             var mkBktArgs = new MakeBucketArgs().WithBucket(bucketName);
//             await _minioClient.MakeBucketAsync(mkBktArgs, cancellationToken);
//         }

//         byte[] bytes = Encoding.UTF8.GetBytes(jsonContent);
//         using var stream = new MemoryStream(bytes);

//         var putObjectArgs = new PutObjectArgs().WithBucket(bucketName).WithObject(fileName).WithStreamData(stream).WithObjectSize(bytes.Length).WithContentType("application/json");
//         await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);
//     }
// }