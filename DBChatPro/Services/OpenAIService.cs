using static MudBlazor.CategoryTypes;
using System.Text.Json;
using System.Text;
using OpenAI.Chat;
using OpenAI;
using Microsoft.Extensions.Logging;

namespace DBChatPro.Services
{
    public class OpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AIQuery> GetAISQLQuery(string userPrompt, AIConnection aiConnection)
        {
            // Obtener la clave de OpenAI del archivo de configuración
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("OpenAI API Key not found in configuration.");
            }

            // Crear el cliente de OpenAI con la clave desde appsettings.json
            var api = new OpenAIClient(apiKey);

            // Crear el cliente de chat de OpenAI
            var chatClient = api.GetChatClient("gpt-4o-mini");

            List<ChatMessage> chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("You are an expert assistant who knows how to write expert queries in Microsoft SQL Server. The commands to use must work fine in SQL Server. All the queries must use T-SQL commands. Do not respond with any information unrelated to databases or queries. Use the following database schema when creating your answers:");

            foreach (var table in aiConnection.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine("Include column name headers in the query results.");
            builder.AppendLine("Always provide your answer in the JSON format below:");
            builder.AppendLine(@"{ ""summary"": ""your-summary"", ""query"":  ""your-query"" }");
            builder.AppendLine("Output ONLY JSON formatted on a single line. Do not use new line characters.");
            builder.AppendLine(@"In the preceding JSON response, substitute ""your-query"" with Microsoft SQL Server Query to retrieve the requested data.");
            builder.AppendLine("Do not use MySQL or PostgreSQL syntax.");
            builder.AppendLine("Include only the most important and relevant table columns and details.");

            // Build the AI chat/prompts
            chatHistory.Add(new SystemChatMessage(builder.ToString()));
            chatHistory.Add(new UserChatMessage(userPrompt));

            // Send request to Azure OpenAI model
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

        public async Task<ValidationResult> ValidateSQLQuery(string sqlQuery)
        {
            // Obtener la clave de OpenAI del archivo de configuración
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("OpenAI API Key not found in configuration.");
            }

            // Crear el cliente de OpenAI con la clave desde appsettings.json
            var api = new OpenAIClient(apiKey);

            // Crear el cliente de chat de OpenAI
            var chatClient = api.GetChatClient("gpt-4o-mini");

            List<ChatMessage> chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("You are a meticulous assistant specialized in validating SQL queries and their results.");
            builder.AppendLine("Please review the following SQL query and confirm if it is correct and will run smoothly in Microsoft SQL Server. If there are any issues, provide detailed feedback.");

            builder.AppendLine("**SQL Query:**");
            builder.AppendLine(sqlQuery);

            // Opcional: Puedes agregar resultados de ejecución si los tienes
            // builder.AppendLine("**Execution Result:**");
            // builder.AppendLine(executionResult);

            // Define el formato de respuesta esperado
            builder.AppendLine("Respond in JSON format with the following structure:");
            builder.AppendLine(@"{ ""isValid"": true, ""feedback"": ""Your feedback here."" }");

            // Build the AI chat/prompts
            chatHistory.Add(new SystemChatMessage(builder.ToString()));
            chatHistory.Add(new UserChatMessage("Please validate the following SQL query:"));

            // Send request to Azure OpenAI model
            var response = await chatClient.CompleteChatAsync(chatHistory);
            var responseContent = response.Value.Content[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", "");

            try
            {
                return JsonSerializer.Deserialize<ValidationResult>(responseContent);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to parse AI response as a Validation Result. The AI response was: " + response.Value.Content[0].Text);
            }
        }
    }
}
