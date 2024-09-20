using G1.health.IdentityService.Roles;
using G1.health.IdentityService.Users;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.Authorizations;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.OpenIddict.Scopes;
using Volo.Abp.OpenIddict.Tokens;
//using Volo.Chat.Conversations;
//using Volo.Chat.EntityFrameworkCore;
//using Volo.Chat.Messages;
//using Volo.Chat.Users;

namespace G1.health.IdentityService.EntityFrameworkCore;

[ConnectionStringName(IdentityServiceDbProperties.ConnectionStringName)]
public class IdentityServiceDbContext : AbpDbContext<IdentityServiceDbContext>, IIdentityServiceDbContext, IOpenIddictDbContext //IChatDbContext IIdentityDbContext
{
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<OpenIddictApplication> Applications { get; set; }
    public DbSet<OpenIddictAuthorization> Authorizations { get; set; }
    public DbSet<OpenIddictScope> Scopes { get; set; }
    public DbSet<OpenIddictToken> Tokens { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    public DbSet<UserTenantAssociation> UserTenantAssociations { get; set; }
    public DbSet<TenantRolesAssociation> TenantRolesAssociations { get; set; }

    //public DbSet<Message> ChatMessages { get; set; }

    //public DbSet<ChatUser> ChatUsers { get; set; }

    //public DbSet<UserMessage> ChatUserMessages { get; set; }

    //public DbSet<Conversation> ChatConversations { get; set; }

    public IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.ConfigureWarnings(w => w.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS));
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureIdentityPro();
        builder.ConfigureOpenIddictPro();

        /* Define mappings for your custom entities here...
        modelBuilder.Entity<MyEntity>(b =>
        {
            b.ToTable(IdentityServiceDbProperties.DbTablePrefix + "MyEntities", IdentityServiceDbProperties.DbSchema);
            b.ConfigureByConvention();
            //TODO: Configure other properties, indexes... etc.
        });
        */
        //builder.ConfigureChat();
        builder.Entity<UserTenantAssociation>(b =>
        {
            b.ToTable("user_tenant_association");
            b.ConfigureByConvention();
            b.Property(x => x.UserId);
            b.Property(x => x.TenantId);
            b.Property(x => x.IsActive).IsRequired();
            b.HasQueryFilter(x => x.IsActive);
            b.HasQueryFilter(e => e.TenantId == CurrentTenant.Id);
        });

        builder.Entity<TenantRolesAssociation>(b =>
        {
            b.ToTable("TenantRolesAssociation");
            b.ConfigureByConvention();
            b.Property(x => x.RoleId);
            b.Property(x => x.TenantId);
            b.Property(x => x.IsDefault);
            b.Property(x => x.IsPublic);
            b.HasQueryFilter(e => e.TenantId == CurrentTenant.Id);
        });
    }
}
