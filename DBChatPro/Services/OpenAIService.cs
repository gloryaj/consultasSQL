using static MudBlazor.CategoryTypes;
using System.Text.Json;
using System.Text;
using OpenAI.Chat;
using OpenAI;
using DBChatPro.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using OpenAI.Embeddings;
using MongoDB.Driver;

namespace DBChatPro.Services
{
    public class OpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly OpenAIClient _client;
        private readonly string _apiKey;

        public OpenAIService(IConfiguration configuration, MongoDBService mongoService)
        {
            _configuration = configuration;

            _apiKey = _configuration["OpenAI:ApiKey"];
            _client = new OpenAIClient(_apiKey);
        }

        public async Task<float[]> GenerateEmbeddingsAsync(string text)
        {
            var embeddingClient = _client.GetEmbeddingClient("text-embedding-ada-002");

            OpenAIEmbedding embedding = await embeddingClient.GenerateEmbeddingAsync(text);

            // Convertir la incrustación a un arreglo de flotantes
            return embedding.ToFloats().ToArray();
        }

        public async Task<float[]> GenerateEmbeddingsFromFile(string filePath)
        {
            FileService fileService = new FileService();

            string text = fileService.ReadTextFile(filePath);

            return await GenerateEmbeddingsAsync(text);
        }

        public async Task SaveEmbeddingsToMongo(string filePath, string collectionName)
        {
            float[] embeddings = await GenerateEmbeddingsFromFile(filePath);
            Console.WriteLine("Embeddings: " + embeddings);

            string connectionString = _configuration["MongoDB:ConnectionString"];
            string dbName = _configuration["MongoDB:Database"];
            Console.WriteLine("string conection:" + connectionString);
            Console.WriteLine("nombre base de datos: " + dbName);

            var mongoService = new MongoDBService(connectionString, dbName);

            // Crear un documento para almacenar en la colección
            var document = new
            {
                FilePath = filePath,
                Embeddings = embeddings,
                CreatedAt = DateTime.UtcNow
            };

            //Insertar el documento en la colección de MongoDB
            await mongoService.InsertDocument(collectionName, document);
            Console.WriteLine("Embeddings guardados exitosamente en MongoDB. y document es: "+ document);
        }

        public async Task<AIQuery> GetAISQLQuery(string userPrompt, AIConnection aiConnection)
        {
            // Obtener la clave de OpenAI del archivo de configuración
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("OpenAI API Key not found in configuration.");
            }

            var api = new OpenAIClient(apiKey);
            // Crear el cliente de chat de OpenAI
            var chatClient = api.GetChatClient("gpt-4o-mini");

            List<ChatMessage> chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

             // Prompt para la generación de la consulta SQL
             builder.AppendLine("You are an expert assistant who knows how to write expert queries in Microsoft SQL Server. The commands to use must work fine in SQL Server. All the queries must use T-SQL commands. Do not respond with any information unrelated to databases or queries.");
             builder.AppendLine("Use the following database schema when creating your answers:");
            
            foreach (var table in aiConnection.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine("Include column name headers in the query results.");
            builder.AppendLine("Always provide your answer in the JSON format below:");
            builder.AppendLine(@"{ ""summary"": ""your-summary"", ""query"":  ""your-query"" }");
            builder.AppendLine("Output ONLY JSON formatted on a single line. Do not use new line characters.");
            builder.AppendLine(@"In the preceding JSON response, substitute ""your-query"" with a Microsoft SQL Server Query to retrieve the requested data.");
            builder.AppendLine("Do not use MySQL or PostgreSQL syntax.");
            builder.AppendLine("Include only the most important and relevant table columns and details.");

            chatHistory.Add(new SystemChatMessage(builder.ToString()));
            chatHistory.Add(new UserChatMessage(userPrompt));

            // Enviar la solicitud a OpenAI para generar la consulta SQL
            var response = await chatClient.CompleteChatAsync(chatHistory);
            var responseContent = response.Value.Content[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", "");

            try
            {
                return JsonSerializer.Deserialize<AIQuery>(responseContent);
            }
            catch (Exception e)
            {
              throw new Exception("Failed to parse AI response as a SQL Query. The AI response was: " + response.Value.Content[0].Text);
            }
        }

        public async Task<AIQueryAnalysis> AnalyzeQueryResults(string userPrompt, string queryResult)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("OpenAI API Key not found in configuration.");
            }

            var api = new OpenAIClient(apiKey);
            var chatClient = api.GetChatClient("gpt-4o-mini");

            List<ChatMessage> chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("The user provided the following request for analysis: ");
            builder.AppendLine(userPrompt);  
            builder.AppendLine("Now that I have the data from the SQL query:");
            builder.AppendLine(queryResult);
            builder.AppendLine("Analyze the following data in Spanish, strictly focusing on what the user has specifically requested. The analysis must be based solely on the data provided, without adding information or making assumptions that cannot be directly inferred from the data.");
            builder.AppendLine(@"The result must be formatted as:");
            builder.AppendLine(@"{ ""AnalysisSummary"": ""The analysis based on the user's specific request will appear here."" }");
            builder.AppendLine("Output ONLY this JSON structure in a single line with the actual analysis replacing the placeholder text in the 'AnalysisSummary' property.");

            chatHistory.Add(new SystemChatMessage(builder.ToString()));


            chatHistory.Add(new SystemChatMessage(builder.ToString()));
            //chatHistory.Add(new UserChatMessage(userPrompt));

            var analysisResponse = await chatClient.CompleteChatAsync(chatHistory);
            var analysisContent = analysisResponse.Value.Content[0].Text.Replace("\n", "").Replace("\\n", "");

            Console.WriteLine("AI Response Content: " + analysisContent);

            try
            {
                return JsonSerializer.Deserialize<AIQueryAnalysis>(analysisContent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to parse JSON: " + e.Message);
                throw new Exception("Failed to parse AI response as an analysis. The AI response was: " + analysisResponse.Value.Content[0].Text);
            }
        }
    }
}
