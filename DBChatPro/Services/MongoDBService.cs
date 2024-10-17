using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenAI;

namespace DBChatPro.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;
        private readonly string _connectionString;
        private readonly string _databaseName;

        public MongoDBService(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(dbName);
        }

        // verifica la conexión a la base de datos
        public void TestConnection()
        {
            try
            {
                var result = _database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                Console.WriteLine("Pinged MongoDB successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to connect to MongoDB: " + ex.Message);
            }
        }

        public async Task InsertDocument<T>(string collectionName, T document)
        {
            var collection = _database.GetCollection<T>(collectionName);
            await collection.InsertOneAsync(document);
        }

        // para realizar la consulta basada en embeddings para RAG
        public async Task<List<T>> QueryVectorCollection<T>(string collectionName, FilterDefinition<T> filter)
        {
            var collection = _database.GetCollection<T>(collectionName);
            return await collection.Find(filter).ToListAsync();
        }
    }
}
