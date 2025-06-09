using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Seeder;

public class Seeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly Seeds _seeds;

    public Seeder(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        var seedFileContent = File.ReadAllText("./Seeder/Seeds.json");
        _seeds = JsonSerializer.Deserialize<Seeds>(seedFileContent, new JsonSerializerOptions() {
            AllowTrailingCommas = true
        })!;
    }


    public void Seed()
    {
        SeedRoles();
        SeedAdminUser();
    }

    private void SeedRoles()
    {
        foreach (var role in _seeds.Roles)
        {
            var roleExists = _roleManager.RoleExistsAsync(role.Name).Result;
            if (!roleExists)
            {
                _roleManager.CreateAsync(role).Wait();
            }
        }
        _context.SaveChanges();
    }
    private void SeedAdminUser()
    {
        var adminUser = new ApplicationUser
        {
            UserName = _seeds.AdminUsername,
            Email = _seeds.AdminEmail,
            EmailConfirmed = true,
            ProfilePictureUrl = ""
        };
        var user = _userManager.FindByNameAsync(adminUser.UserName).Result;
        if (user == null)
        {
            var createPowerUser = _userManager.CreateAsync(adminUser, _seeds.AdminPassword).Result;
            if (createPowerUser.Succeeded)
            {
                _userManager.AddToRoleAsync(adminUser, "Admin").Wait();
            }
            else
            {
                throw new Exception("Failed to create admin user: " + createPowerUser.Errors.FirstOrDefault()?.Description ?? "Unknown error");
            }
        }
    }
}

public class Seeds
{
    //Roles
    public IdentityRole[] Roles { get; set; }
    // Users
    public string AdminUsername { get; set; }
    public string AdminPassword { get; set; }
    public string AdminEmail { get; set; }
}