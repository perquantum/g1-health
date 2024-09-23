using G1.health.SaasService.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Uow;
using Volo.Saas;
using Volo.Saas.Host;
using Volo.Saas.Host.Dtos;
using Volo.Saas.Tenants;

namespace G1.health.SaasService.Application
{
    [Authorize(SaasHostPermissions.Tenants.Default)]
    public class TenantOverrideAppService : ApplicationService, ITenantOverrideAppService
    {
        protected ITenantOverrideRepository TenantIdRepository;
        protected AbpDbConnectionOptions DbConnectionOptions { get; }
        protected ITenantManager TenantManager { get; }
        protected ITenantRepository TenantRepository { get; }
        protected IDistributedEventBus DistributedEventBus { get; }

        public TenantOverrideAppService(
            ITenantOverrideRepository tenantIdRepository,
            IOptions<AbpDbConnectionOptions> dbConnectionOptions,
            ITenantManager tenantManager,
            ITenantRepository tenantRepository,
            IDistributedEventBus distributedEventBus)
        {
            TenantIdRepository = tenantIdRepository;
            DbConnectionOptions = dbConnectionOptions.Value;
            TenantManager = tenantManager;
            TenantRepository = tenantRepository;
            DistributedEventBus = distributedEventBus;
        }

        public virtual async Task<TenantDto> GetTenantIdByName(string name)
        {
            var result = await TenantIdRepository.GetTenantIdByName(name);
            TenantDto resultDto = new TenantDto() { Id = result.Id, Name = result.Name };
            return resultDto;
        }

        public virtual async Task<List<TenantDto>> GetTenantNamesList()
        {
            var result = await TenantIdRepository.GetTenantNamesList();
            List<TenantDto> resultDto = new List<TenantDto>();
            foreach (var tenant in result)
            {
                resultDto.Add(new TenantDto() { Id = tenant.Id, Name = tenant.Name });
            }
            return resultDto;
        }

        public virtual async Task<SaasTenantDto> CreateTenant(SaasTenantCreateDto input)
        {

            input.ConnectionStrings = await NormalizedConnectionStringsAsync(input.ConnectionStrings);

            Tenant tenant = null;

            async Task CreateTenantAsync()
            {
                tenant = await TenantManager.CreateAsync(input.Name, input.EditionId);

                if (!input.ConnectionStrings.Default.IsNullOrWhiteSpace())
                {
                    tenant.SetDefaultConnectionString(input.ConnectionStrings.Default);
                }

                if (input.ConnectionStrings.Databases != null)
                {
                    foreach (var database in input.ConnectionStrings.Databases)
                    {
                        tenant.SetConnectionString(database.DatabaseName, database.ConnectionString);
                    }
                }

                input.MapExtraPropertiesTo(tenant);

                tenant.SetActivationState(input.ActivationState);
                if (tenant.ActivationState == TenantActivationState.ActiveWithLimitedTime)
                {
                    tenant.SetActivationEndDate(input.ActivationEndDate);
                }
                /* Auto saving to ensure TenantCreatedEto handler can get the tenant! */
                await TenantRepository.InsertAsync(tenant, autoSave: true);
            }

            if (input.ConnectionStrings.Default.IsNullOrWhiteSpace() &&
                input.ConnectionStrings.Databases.IsNullOrEmpty())
            {
                /* Creating the tenant in the current UOW */
                await CreateTenantAsync();
            }
            else
            {
                /* Creating the tenant in a separate UOW to ensure it is created
                 * before creating the database.
                 * TODO: We should remove inner UOW once https://github.com/abpframework/abp/issues/6126 is done
                 */
                using (var uow = UnitOfWorkManager.Begin(requiresNew: true))
                {
                    await CreateTenantAsync();
                    await uow.CompleteAsync();
                }
            }

            await DistributedEventBus.PublishAsync(
                new TenantAdminCreateEto
                {
                    Id = tenant.Id,
                    Name = tenant.Name,
                    Properties =
                    {
                        {"AdminEmail", input.AdminEmailAddress},
                        {"AdminPassword", input.AdminPassword}
                    }
                }
            );

            return ObjectMapper.Map<Tenant, SaasTenantDto>(tenant);
        }

        protected virtual Task<SaasTenantConnectionStringsDto> NormalizedConnectionStringsAsync(SaasTenantConnectionStringsDto input)
        {
            if (input == null)
            {
                input = new SaasTenantConnectionStringsDto();
            }
            else if (!input.Databases.IsNullOrEmpty())
            {
                input.Databases = input.Databases
                    .Where(x => DbConnectionOptions.Databases.Any(d => d.Key == x.DatabaseName && d.Value.IsUsedByTenants))
                    .Where(x => !x.ConnectionString.IsNullOrWhiteSpace())
                    .GroupBy(x => x.DatabaseName)
                    .Select(x => x.First())
                    .ToList();
            }

            return Task.FromResult(input);
        }
    }
}
