namespace BGTA.Server.Services;

public class NavigationState
{
    public string? CurrentFormName { get; private set; }
    public event Action? OnChange;

    public void RequestNavigation(string formName)
    {
        CurrentFormName = formName;
        OnChange?.Invoke(); // Notifie le MainLayout qu'il faut rafraÃ®chir
    }
}