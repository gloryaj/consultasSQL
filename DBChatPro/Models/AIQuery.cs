namespace DBChatPro.Models
{
    public class AIQuery
    {
        public string summary { get; set; }
        public string query { get; set; }
        public string AnalysisSummary { get; set; }
        public List<string> KeyTrends { get; set; }
        public List<string> Recommendations { get; set; }
    }
}
