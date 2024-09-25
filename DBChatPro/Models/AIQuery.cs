using System.Text.Json.Serialization;

namespace DBChatPro.Services
{
    public class AIQuery
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("query")]
        public string Query { get; set; }
    }
}

