using System.Windows;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class MessageTypeExt
    {
        public static string GetLocalizedString(this MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Error: return Resources.SID_Error_;
                case MessageType.Information: return Resources.SID_Information;
                case MessageType.Confirmation: return Resources.SID_Confirmation;
                default: return Resources.SID_Message;
            }
        }

        public static Visibility ShouldCancelBeVisible(this MessageType messageType)
        {
            return messageType == MessageType.Confirmation ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}