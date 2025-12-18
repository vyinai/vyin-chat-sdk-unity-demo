namespace VyinChatSdk
{
    public class VcBaseMessage
    {
        public long MessageId { get; set; }
        public string Message { get; set; }
        public string ChannelUrl { get; set; }
        public string SenderId { get; set; }
        public string SenderNickname { get; set; }
        public long CreatedAt { get; set; }
    }
}