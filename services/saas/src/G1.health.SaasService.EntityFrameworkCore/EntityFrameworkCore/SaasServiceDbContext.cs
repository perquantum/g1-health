using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Payment.EntityFrameworkCore;
using Volo.Payment.Plans;
using Volo.Payment.Requests;
using Volo.Saas.Editions;
using Volo.Saas.EntityFrameworkCore;
using Volo.Saas.Tenants;

namespace G1.health.SaasService.EntityFrameworkCore;

[ConnectionStringName(SaasServiceDbProperties.ConnectionStringName)]
public class SaasServiceDbContext : AbpDbContext<SaasServiceDbContext>, ISaasDbContext, IPaymentDbContext
{
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Edition> Editions { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    public DbSet<PaymentRequest> PaymentRequests { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<GatewayPlan> GatewayPlans { get; set; }

    public SaasServiceDbContext(DbContextOptions<SaasServiceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSaas();
        builder.ConfigurePayment();
        
        /* Define mappings for your custom entities here...
        modelBuilder.Entity<MyEntity>(b =>
        {
            b.ToTable(IdentityServiceDbProperties.DbTablePrefix + "MyEntities", IdentityServiceDbProperties.DbSchema);
            b.ConfigureByConvention();
            //TODO: Configure other properties, indexes... etc.
        });
        */
    }
}
