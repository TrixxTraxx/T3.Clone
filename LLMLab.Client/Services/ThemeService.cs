namespace LLMLab.Client.Services;

public class ThemeService
{
    public event Action ThemeChanged;

    public void Update()
    {
        ThemeChanged?.Invoke();
    }
} 