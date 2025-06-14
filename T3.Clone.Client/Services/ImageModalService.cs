namespace T3.Clone.Client.Services;

public class ImageModalService
{
    public event Action<string>? OnShow;
    public event Action? OnHide;

    public void Show(string imageUrl)
    {
        OnShow?.Invoke(imageUrl);
    }
    public void Hide()
    {
        OnHide?.Invoke();
    }
}