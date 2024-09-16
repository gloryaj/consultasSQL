//using Azure.AI.OpenAI;
//using Azure;
using static MudBlazor.CategoryTypes;
using System.Text.Json;
using System.Text;
using OpenAI.Chat;
using OpenAI;

namespace DBChatPro.Services
{
    public class OpenAIService
    {
        public static async Task<AIQuery> GetAISQLQuery(string userPrompt, AIConnection aiConnection)
        {
            // Configure OpenAI client
            //string openAIEndpoint = "your-openai-endpoint";
            //string openAIKey = "your-openai-key";
            //string openAIDeploymentName = "your-model-deployment-name";

            // Crear el cliente de OpenAI con la clave
            var api = new OpenAIClient("your-openai-key");

            // Crear el cliente de chat de OpenAI
            var chatClient = api.GetChatClient("gpt-4o-mini");

            List<ChatMessage> chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("You are a expert assistant who knows how to write expert queries in Microsoft SQL Server. The commands to use must work fine in SQL Server. All the queries must use T-SQL commands. Do not respond with any information unrelated to databases or queries. Use the following database schema when creating your answers:");

            foreach(var table in aiConnection.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine("Include column name headers in the query results.");
            builder.AppendLine("Always provide your answer in the JSON format below:");
            builder.AppendLine(@"{ ""summary"": ""your-summary"", ""query"":  ""your-query"" }");
            builder.AppendLine("Output ONLY JSON formatted on a single line. Do not use new line characters.");
            builder.AppendLine(@"In the preceding JSON response, substitute ""your-query"" with Microsoft SQL Server Query to retrieve the requested data.");
            builder.AppendLine(@"In the preceding JSON response, substitute ""your-summary"" with a detailed analysis of the key insights in Spanish. The summary should include specific metrics such as totals, averages, the most frequent or highest values, and any other relevant patterns or trends regardless of the data type. Be specific and provide numerical values where applicable. For example, 'El artículo más vendido fue X con un total de Y ventas, representando Z% del total de ventas.'");
            builder.AppendLine("Do not use MySQL or PostgreSQL syntax.");
            //builder.AppendLine("Always include all of the table columns and details.");
            
            // builder.AppendLine("Always limit the SQL Query to 100 rows.");


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
    }
}
