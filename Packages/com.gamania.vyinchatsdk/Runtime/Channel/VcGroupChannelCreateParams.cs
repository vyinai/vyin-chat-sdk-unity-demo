using System.Collections.Generic;

namespace VyinChatSdk
{
    public class VcGroupChannelCreateParams
    {
        public string Name { get; set; }
        public List<string> OperatorUserIds { get; set; }
        public List<string> UserIds { get; set; }
        public bool IsDistinct { get; set; }
    }
}