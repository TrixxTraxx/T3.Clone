﻿using System.Text.Json;
using LLMLab.Server.Configuration;
using LLMLab.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LLMLab.Server.Seeder;

public class Seeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IOptions<Appsettings> _appSettings;
    private readonly Seeds _seeds;

    public Seeder(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<Appsettings> appSettings)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _appSettings = appSettings;
        var seedFileContent = File.ReadAllText("./Seeder/Seeds.json");
        _seeds = JsonSerializer.Deserialize<Seeds>(seedFileContent, new JsonSerializerOptions() {
            AllowTrailingCommas = true
        })!;
    }


    public void Seed()
    {
        SeedRoles();
        SeedAdminUser();
        if (_appSettings.Value.EnableModelSeeding)
        {
            SeedModels();
        }
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
            ProfilePictureUrl = "",
            DisplayName = "Admin",
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

    private void SeedModels()
    {
        foreach (var model in _seeds.Models)
        {
            var existingModel = _context.AiModels.FirstOrDefault(m => m.Name == model.Name && m.ModelId == model.ModelId);
            if (existingModel == null)
            {
                _context.AiModels.Add(model);
            }
            else
            {
                existingModel.Description = model.Description;
                existingModel.IsDefault = model.IsDefault;
                existingModel.HasImageSupport = model.HasImageSupport;
                existingModel.SupportedContentTypes = model.SupportedContentTypes;
                existingModel.HasImageGenerationSupport = model.HasImageGenerationSupport;
                existingModel.HasThinkingSupport = model.HasThinkingSupport;
                existingModel.RequireThinking = model.RequireThinking;
                
                existingModel.SystemPrompt = model.SystemPrompt;
                existingModel.Provider = model.Provider;
                existingModel.ApiUrl = model.ApiUrl;
                existingModel.ApiKey = model.ApiKey;
                existingModel.RequiredApiKey = model.RequiredApiKey;
                
                existingModel.InputTokenCost = model.InputTokenCost;
                existingModel.OutputTokenCost = model.OutputTokenCost;
                existingModel.ThinkingCost = model.ThinkingCost;
                existingModel.ImageGenerationCost = model.ImageGenerationCost;
                existingModel.MaxInputTokens = model.MaxInputTokens;
                existingModel.MaxOutputTokens = model.MaxOutputTokens;
            }
        }
        _context.SaveChanges();
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
    
    public List<AiModel> Models { get; set; } = new();
}