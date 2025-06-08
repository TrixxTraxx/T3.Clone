namespace T3.Clone.Client.Services;

public class ThemeService
{
    public event Action ThemeChanged;

    public void Update()
    {
        ThemeChanged?.Invoke();
    }
} 