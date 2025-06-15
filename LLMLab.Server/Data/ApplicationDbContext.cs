using LLMLab.Server.Configuration;
using LLMLab.Server.Data.Encryption;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using T3.Clone.Server.Migrations;

namespace LLMLab.Server.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private IEncryptionProvider _provider;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IOptions<Appsettings> settings) : base(options)
    {
        this._provider = new AesEncryptionProvider(settings.Value.EncryptionKey);
    }

    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageThread> MessageThreads { get; set; }
    public DbSet<MessageAttachment> MessageAttachments { get; set; }
    public DbSet<AiModel> AiModels { get; set; }
    public DbSet<AiModelKeys> AiModelKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Message>()
            .Property(m => m.Text)
            .HasColumnType("nvarchar(-1)")
            .HasConversion(
                x => _provider.Encrypt(x),
                x => _provider.Decrypt(x)
            );
        builder.Entity<Message>()
            .Property(m => m.ModelResponse)
            .HasColumnType("nvarchar(-1)")
            .HasConversion(
                x => _provider.Encrypt(x),
                x => _provider.Decrypt(x)
            );
        builder.Entity<Message>()
            .Property(m => m.ThinkingResponse)
            .HasColumnType("nvarchar(-1)")
            .HasConversion(
                x => _provider.Encrypt(x),
                x => _provider.Decrypt(x)
            );
        
        builder.Entity<AiModelKeys>()
            .Property(m => m.ApiKey)
            .HasColumnType("nvarchar(-1)")
            .HasConversion(
                x => _provider.Encrypt(x),
                x => _provider.Decrypt(x)
            );
        
        base.OnModelCreating(builder);
    }
}
