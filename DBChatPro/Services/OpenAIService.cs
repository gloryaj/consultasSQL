﻿using static MudBlazor.CategoryTypes;
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
    }
}
