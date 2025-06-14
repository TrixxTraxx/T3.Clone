using Microsoft.AspNetCore.Components;

namespace T3.Clone.Client.Components.MessageParts;

public abstract class MessagePartComponent : ComponentBase
{
    public abstract Task OnTokenReceived(string fullContent);

}