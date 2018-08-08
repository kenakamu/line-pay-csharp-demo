namespace LinePayCSharpDemo.Models
{
    public class AppSettings
    {
      public LinePay LinePay { get; set; }
      public LineBot LineBot { get; set; }
      public string ServerUri{ get; set; }
    }
    public class LinePay
    {
        public string ChannelId { get; set; }
        public string ChannelSecret { get; set; }
        public bool IsSandbox { get; set; }
    }    
    public class LineBot
    {
        public string ChannelSecret { get; set; }
        public string ChannelAccessToken { get; set; }
    }    
}