using Microsoft.AspNetCore.Components;

namespace LLMLab.Client.Components.MessageParts;

public abstract class MessagePartComponent : ComponentBase
{
    public abstract Task OnTokenReceived(string fullContent);

}