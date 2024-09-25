using System.Text.Json.Serialization;

namespace DBChatPro.Services
{
    public class ValidationResult
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("feedback")]
        public string Feedback { get; set; }
    }
}
