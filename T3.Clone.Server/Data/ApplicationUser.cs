using Microsoft.AspNetCore.Identity;

namespace T3.Clone.Server.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string ProfilePictureUrl { get; set; }
    
    public int ThreadVersion { get; set; } = 1;
    public string DisplayName { get; set; }
}

