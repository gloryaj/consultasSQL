using static MudBlazor.CategoryTypes;
using System.Text.Json;
using System.Text;
using OpenAI.Chat;
using OpenAI;

namespace DBChatPro.Services
{
    public class OpenAIService
    {
        private readonly IConfiguration _configuration;

        public OpenAIService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<AIQuery> GetAISQLQueryWithAnalysis(string userPrompt, AIConnection aiConnection, string queryResult = null)
        {
<<<<<<< Updated upstream
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

            if (queryResult == null)
            {
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

                // Añadir el prompt del usuario
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
            else
            {
                builder.AppendLine("Now that I have the data from the SQL query:");
                builder.AppendLine("Analyze the following data in Spanish, focusing on the most significant insights, patterns, and trends that you can infer based on the provided information.Tailor the analysis to address the user's original request or any specific goals they may have, while also highlighting any other notable observations from the data. The analysis should consider factors such as trends over time, patterns in product types or categories, and areas of opportunity for improvement. The output must be in the following JSON format:");
                builder.AppendLine(@"{ ""AnalysisSummary"": ""Provide a brief summary of insights here."", ""KeyTrends"": [""List key trends or insights here as an array.""], ""Recommendations"": [""List any recommendations here.""] }");
                builder.AppendLine("Output ONLY JSON formatted on a single line.");
                builder.AppendLine(queryResult);
                chatHistory.Add(new SystemChatMessage(builder.ToString()));

                // Enviar la solicitud para el análisis de datos
                var analysisResponse = await chatClient.CompleteChatAsync(chatHistory);
                var analysisContent = analysisResponse.Value.Content[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", "");


                // Verificar el contenido de la respuesta antes de deserializar
                Console.WriteLine("AI Response Content: " + analysisContent);
=======
            var api = InitializeOpenAIClient();
            var chatClient = api.GetChatClient("gpt-4o-mini");

            var prompt = CreateSQLQueryPrompt(aiConnection.SchemaRaw);
            List<ChatMessage> chatHistory = new List<ChatMessage>
            {
                new SystemChatMessage(prompt),
                new UserChatMessage(userPrompt)
            };

            var response = await chatClient.CompleteChatAsync(chatHistory);
            var responseContent = CleanAIResponse(response.Value.Content[0].Text);

            try
            {
                var aiQuery = JsonSerializer.Deserialize<AIQuery>(responseContent);

                // Revisar la consulta con la IA antes de ejecutarla
                bool isSafe = await ReviewSQLQueryWithAI(aiQuery.query);
                if (!isSafe)
                {
                    throw new Exception("The AI review determined that the SQL query is not safe to execute.");
                }

                // Ejecutar la consulta aquí si es segura
                //var queryResult = await ExecuteSQLQuery(aiQuery.query);
                //Console.WriteLine("Sí se validó la consulta");

                return aiQuery;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to parse AI response as a SQL Query. The AI response was: " + responseContent);
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
>>>>>>> Stashed changes

                try
                {
                    return JsonSerializer.Deserialize<AIQuery>(analysisContent); 
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to parse JSON: " + e.Message);
                    throw new Exception("Failed to parse AI response as an analysis. The AI response was: " + analysisResponse.Value.Content[0].Text);
                }
            }
        }

        private OpenAIClient InitializeOpenAIClient()
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("OpenAI API Key not found in configuration.");
            }
            return new OpenAIClient(apiKey);
        }

        private string CreateSQLQueryPrompt(IEnumerable<string> schemaRaw)
        {
            var builder = new StringBuilder();
            builder.AppendLine("You are an expert assistant who knows how to write expert queries in Microsoft SQL Server.");
            builder.AppendLine("The commands to use must work fine in SQL Server. All the queries must use Transac SQL commands.");
            builder.AppendLine("Do not respond with ANY OTHER information unrelated to databases or queries.");
            builder.AppendLine("Use the following database schema when creating your answers:");
            builder.AppendLine("Do not use MySQL or PostgreSQL syntax.");
            builder.AppendLine("ONLY RETURN THE QUERY, DO NOT INCLUDE PREFIXES.");

            foreach (var table in schemaRaw)
            {
                builder.AppendLine(table);
            }

            //builder.AppendLine("Include column name headers in the query results.");
            //builder.AppendLine("Always provide your answer in the JSON format below:");
            //builder.AppendLine(@"{ ""summary"": ""your-summary"", ""query"":  ""your-query"" }");
            //builder.AppendLine("Output ONLY JSON formatted on a single line. Do not use new line characters.");
            //builder.AppendLine(@"In the preceding JSON response, substitute ""your - query"" with a Microsoft SQL Server Query to retrieve the requested data.");
            
            //builder.AppendLine("Do not use MySQL or PostgreSQL syntax.");
            
            //builder.AppendLine("Include only the most important and relevant table columns and details.");
            //builder.AppendLine("For quantity and numeric values, ALWAYS format numbers using numeric notation with thousands and 2 decimals.");

            return builder.ToString();
        }

        private async Task<bool> ReviewSQLQueryWithAI(string sqlQuery)
        {
            var api = InitializeOpenAIClient();
            var chatClient = api.GetChatClient("gpt-4o-mini");

            // Crear el prompt para que la IA revise la consulta SQL generada
            var builder = new StringBuilder();
            builder.AppendLine("You are an expert in SQL Server and database security. Please review the following SQL query for potential security issues or inefficiencies.");
            builder.AppendLine("Consider if the query has possible SQL injection risks, dangerous commands (such as DELETE or DROP), or could lead to performance issues.");
            builder.AppendLine("Provide your answer in the following JSON format:");
            builder.AppendLine(@"{ ""is_safe"": true/false, ""issues"": ""A summary of the issues found (or a message that the query is safe)."" }");
            builder.AppendLine("Output ONLY this JSON structure in a single line with the actual analysis.");

            List<ChatMessage> chatHistory = new List<ChatMessage>
            {
                new SystemChatMessage(builder.ToString()),
                new UserChatMessage(sqlQuery)
            };

            var response = await chatClient.CompleteChatAsync(chatHistory);
            var responseContent = response.Value.Content[0].Text.Replace("\n", "").Replace("\\n", "");

            try
            {
                // Deserializar la respuesta de la IA
                var aiReview = JsonSerializer.Deserialize<AIReviewResponse>(responseContent);
                if (aiReview != null && aiReview.IsSafe)
                {
                    return true; // La consulta se considera segura
                }
                else
                {
                    Console.WriteLine($"AI Review found issues: {aiReview?.Issues}");
                    return false; // La consulta tiene problemas
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to parse AI review response: " + e.Message);
                throw new Exception("Failed to parse AI review response. The AI response was: " + responseContent);
            }
        }

        private string CleanAIResponse(string responseContent)
        {
            return responseContent.Replace("```json", "").Replace("```", "").Replace("\n", "").Replace("\\n", "");
        }
    }

    public class AIReviewResponse
    {
        public bool IsSafe { get; set; }
        public string Issues { get; set; }
    }
}



//using static MudBlazor.CategoryTypes;
//using System.Text.Json;
//using System.Text;
//using OpenAI.Chat;
//using OpenAI;
//using DBChatPro.Models;

//namespace DBChatPro.Services
//{


//    public class OpenAIService
//    {
//        private readonly IConfiguration _configuration;

//        public OpenAIService(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public async Task<AIQuery> GetAISQLQuery(string userPrompt, AIConnection aiConnection)
//        {
//            // Obtener la clave de OpenAI del archivo de configuración
//            var apiKey = _configuration["OpenAI:ApiKey"];

//            if (string.IsNullOrEmpty(apiKey))
//            {
//                throw new Exception("OpenAI API Key not found in configuration.");
//            }

//            var api = new OpenAIClient(apiKey);
//            // Crear el cliente de chat de OpenAI
//            var chatClient = api.GetChatClient("gpt-4o-mini");

//            List<ChatMessage> chatHistory = new List<ChatMessage>();
//            var builder = new StringBuilder();

//            // Prompt para la generación de la consulta SQL
//            builder.AppendLine("You are an expert assistant who knows how to write expert queries in Microsoft SQL Server. The commands to use must work fine in SQL Server. All the queries must use T-SQL commands. Do not respond with any information unrelated to databases or queries.");
//            builder.AppendLine("For quantity and numeric values, ALWAYS format numbers using numeric notation with thousands and 2 decimals.");
//            builder.AppendLine("Use the following database schema when creating your answers:");

//            foreach (var table in aiConnection.SchemaRaw)
//            {
//                builder.AppendLine(table);
//            }

//            builder.AppendLine("Include column name headers in the query results.");
//            builder.AppendLine("Always provide your answer in the JSON format below:");
//            builder.AppendLine(@"{ ""summary"": ""your-summary"", ""query"":  ""your-query"" }");
//            builder.AppendLine("Output ONLY JSON formatted on a single line. Do not use new line characters.");
//            builder.AppendLine(@"In the preceding JSON response, substitute ""your-query"" with a Microsoft SQL Server Query to retrieve the requested data.");
//            builder.AppendLine("Do not use MySQL or PostgreSQL syntax.");
//            builder.AppendLine("Include only the most important and relevant table columns and details.");

//            chatHistory.Add(new SystemChatMessage(builder.ToString()));
//            chatHistory.Add(new UserChatMessage(userPrompt));

//            // Enviar la solicitud a OpenAI para generar la consulta SQL
//            var response = await chatClient.CompleteChatAsync(chatHistory);
//            var responseContent = response.Value.Content[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", "");

//            try
//            {
//                return JsonSerializer.Deserialize<AIQuery>(responseContent);
//            }
//            catch (Exception e)
//            {
//              throw new Exception("Failed to parse AI response as a SQL Query. The AI response was: " + response.Value.Content[0].Text);
//            }
//        }

//        public async Task<AIQueryAnalysis> AnalyzeQueryResults(string userPrompt, string queryResult)
//        {
//            var apiKey = _configuration["OpenAI:ApiKey"];

//            if (string.IsNullOrEmpty(apiKey))
//            {
//                throw new Exception("OpenAI API Key not found in configuration.");
//            }

//            var api = new OpenAIClient(apiKey);
//            var chatClient = api.GetChatClient("gpt-4o-mini");

//            List<ChatMessage> chatHistory = new List<ChatMessage>();
//            var builder = new StringBuilder();

//            builder.AppendLine("The user provided the following request for analysis: ");
//            builder.AppendLine(userPrompt);  
//            builder.AppendLine("Now that I have the data from the SQL query:");
//            builder.AppendLine(queryResult);
//            builder.AppendLine("Analyze the following data in Spanish, strictly focusing on what the user has specifically requested. The analysis must be based solely on the data provided, without adding information or making assumptions that cannot be directly inferred from the data.");
//            builder.AppendLine(@"The result must be formatted as:");
//            builder.AppendLine(@"{ ""AnalysisSummary"": ""The analysis based on the user's specific request will appear here."" }");
//            builder.AppendLine("Output ONLY this JSON structure in a single line with the actual analysis replacing the placeholder text in the 'AnalysisSummary' property.");

//            chatHistory.Add(new SystemChatMessage(builder.ToString()));


//            chatHistory.Add(new SystemChatMessage(builder.ToString()));
//            //chatHistory.Add(new UserChatMessage(userPrompt));

//            var analysisResponse = await chatClient.CompleteChatAsync(chatHistory);
//            var analysisContent = analysisResponse.Value.Content[0].Text.Replace("\n", "").Replace("\\n", "");

//            Console.WriteLine("AI Response Content: " + analysisContent);

//            try
//            {
//                return JsonSerializer.Deserialize<AIQueryAnalysis>(analysisContent);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("Failed to parse JSON: " + e.Message);
//                throw new Exception("Failed to parse AI response as an analysis. The AI response was: " + analysisResponse.Value.Content[0].Text);
//            }
//        }
//    }
//}
