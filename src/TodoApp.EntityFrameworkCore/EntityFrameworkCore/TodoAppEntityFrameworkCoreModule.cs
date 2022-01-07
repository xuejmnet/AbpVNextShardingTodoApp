using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore;
using ShardingCore.Bootstrapers;
using ShardingCore.DIExtensions;
using ShardingCore.Helpers;
using TodoApp.VirtualRoutes;
using Volo.Abp;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.IdentityServer.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace TodoApp.EntityFrameworkCore
{
    [DependsOn(
        typeof(TodoAppDomainModule),
        typeof(AbpIdentityEntityFrameworkCoreModule),
        typeof(AbpIdentityServerEntityFrameworkCoreModule),
        typeof(AbpPermissionManagementEntityFrameworkCoreModule),
        typeof(AbpSettingManagementEntityFrameworkCoreModule),
        typeof(AbpEntityFrameworkCoreSqlServerModule),
        typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
        typeof(AbpAuditLoggingEntityFrameworkCoreModule),
        typeof(AbpTenantManagementEntityFrameworkCoreModule),
        typeof(AbpFeatureManagementEntityFrameworkCoreModule)
        )]
    public class TodoAppEntityFrameworkCoreModule : AbpModule
    {
        public static readonly ILoggerFactory efLogger = LoggerFactory.Create(builder =>
        {
            builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
        });
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            TodoAppEfCoreEntityExtensionMappings.Configure();
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<TodoAppDbContext>(options =>
            {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
                options.AddDefaultRepositories(includeAllEntities: true);
            });
            Configure<AbpDbContextOptions>(options =>
            {
                /* The main point to change your DBMS.
                 * See also TodoAppDbContextFactory for EF Core tooling. */
                options.UseSqlServer();
                options.Configure<TodoAppDbContext>(context1 =>
                {
                    context1.DbContextOptions.UseSqlServer("Server=.;Database=TodoApp;Trusted_Connection=True").UseSharding<TodoAppDbContext>();
                });
            });
            context.Services.AddShardingConfigure<TodoAppDbContext>((s, builder) =>
             {
                 builder.UseSqlServer(s).UseLoggerFactory(efLogger);
             }).Begin(o =>
                 {
                     o.CreateShardingTableOnStart = false;
                     o.EnsureCreatedWithOutShardingTable = false;
                 })
                 .AddShardingTransaction((connection, builder) =>
                     builder.UseSqlServer(connection).UseLoggerFactory(efLogger))
                 .AddDefaultDataSource("ds0", "Server=.;Database=TodoApp;Trusted_Connection=True")
                 .AddShardingTableRoute(o =>
                 {
                     o.AddShardingTableRoute<ToDoItemVirtualTableRoute>();
                 }).End(); 
        }



        public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            base.OnPostApplicationInitialization(context);
            context.ServiceProvider.GetRequiredService<IShardingBootstrapper>().Start();
        }
    }
}
