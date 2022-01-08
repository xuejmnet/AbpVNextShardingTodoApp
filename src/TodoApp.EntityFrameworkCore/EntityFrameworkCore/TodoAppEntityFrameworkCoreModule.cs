using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore;
using ShardingCore.Bootstrapers;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.DIExtensions;
using ShardingCore.Helpers;
using ShardingCore.TableExists;
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
                    //简洁
                    DIExtension.UseDefaultSharding<TodoAppDbContext>(context1.ServiceProvider, context1.DbContextOptions);


                    ////也可以选择这个配置和上述一样进行了封装
                    //var virtualDataSource = context1.ServiceProvider.GetRequiredService<IVirtualDataSourceManager<TodoAppDbContext>>().GetCurrentVirtualDataSource();
                    //var connectionString = virtualDataSource.GetConnectionString(virtualDataSource.DefaultDataSourceName);
                    //virtualDataSource.ConfigurationParams.UseDbContextOptionsBuilder(connectionString, context1.DbContextOptions);
                    //context1.DbContextOptions.UseSharding<TodoAppDbContext>();



                    ////如果你只有单配置可以选择这个配置但是链接字符串必须和AddConfig内部一样
                    //context1.DbContextOptions.UseSqlServer("Server=.;Database=TodoApp;Trusted_Connection=True").UseSharding<TodoAppDbContext>();
                });
            });
            context.Services.AddShardingConfigure<TodoAppDbContext>()
                .AddEntityConfig(op =>
                {
                    op.CreateShardingTableOnStart = true;
                    op.EnsureCreatedWithOutShardingTable = true;
                    op.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr).UseLoggerFactory(efLogger);
                    });
                    op.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
                    });
                    op.AddShardingTableRoute<ToDoItemVirtualTableRoute>();
                })
                .AddConfig(op =>
                {
                    op.ConfigId = "c1";
                    op.AddDefaultDataSource("ds0", "Server=.;Database=TodoApp;Trusted_Connection=True");
                    op.ReplaceTableEnsureManager(sp => new SqlServerTableEnsureManager<TodoAppDbContext>());
                }).EnsureConfig();
        }



        public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            base.OnPostApplicationInitialization(context);
            context.ServiceProvider.GetRequiredService<IShardingBootstrapper>().Start();
        }
    }
}
