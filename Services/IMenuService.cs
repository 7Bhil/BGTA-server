using BGTA.Server.Models;

namespace BGTA.Server.Services;

public interface IMenuService
{
    // Supprimez le paramètre int profilId
    Task<List<MenuItem>> GetMenuItemsAsync(string? applicationId = null);
}


//public interface IMenuService
//{
//    Task<List<MenuItem>> GetMenuItemsAsync(int profilId, string? applicationId = null);
//}

