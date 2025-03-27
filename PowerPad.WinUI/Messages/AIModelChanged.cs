using CommunityToolkit.Mvvm.Messaging.Messages;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models;

namespace PowerPad.WinUI.Messages
{
    public class AIModelChanged : ValueChangedMessage<AIModel>
    {
        public AIModelChanged(AIModel value) : base(value)
        {
        }
    }
}