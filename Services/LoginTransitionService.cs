
public class LoginTransitionService
{
    public string? Username { get; set; }
    public int ProfilId { get; set; }
    
    // Nettoyage pour libérer la mémoire
    public void Clear() { Username = null; ProfilId = 0; }
}