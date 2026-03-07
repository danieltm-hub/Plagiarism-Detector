using MongoDB.Bson;
using MongoDB.Driver;
using PlagiarismDetector.Domain.Interfaces;

namespace PlagiarismDetector.Infrastructure;

public class MongoStorageRepository : IStorageRepository
{
    private readonly IMongoDatabase _database;

    public MongoStorageRepository(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task UploadFileAsync(string bucketName, string fileName, string jsonContent, CancellationToken cancellationToken = default)
    {
        var collection = _database.GetCollection<BsonDocument>(bucketName);

        var document = BsonDocument.Parse(jsonContent);
        
        document.Set("OriginalFileName", fileName);
        document.Set("ProcessedAt", DateTime.UtcNow);

        await collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }
}