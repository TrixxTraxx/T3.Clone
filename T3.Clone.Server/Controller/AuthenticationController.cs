using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using T3.Clone.Server.Data;
using T3.Clone.Server.Mappers;

namespace T3.Clone.Server.Controller;

[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    [HttpGet("user")]
    public async Task<IActionResult> GetUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(UserMapper.Map(user));
    }
    
    
    [HttpGet("profilePicture")]
    public async Task<IActionResult> GetProfilePicture()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }
        
        //proxy the profile picture URL and cache it
        if (string.IsNullOrEmpty(user.ProfilePictureUrl))
        {
            return NotFound("Profile picture not found.");
        }
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(user.ProfilePictureUrl);
        if (!response.IsSuccessStatusCode)
        {
            return NotFound("Profile picture not found.");
        }
        var content = await response.Content.ReadAsByteArrayAsync();
        return File(content, "image/jpeg"); // Assuming the profile picture is in JPEG format
    }
}