using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace T3.Clone.Server.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageThread> MessageThreads { get; set; }
    public DbSet<MessageAttachment> MessageAttachments { get; set; }
    public DbSet<AiModel> AiModels { get; set; }
}
