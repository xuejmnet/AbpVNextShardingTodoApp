using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore;
using ShardingCore.EFCores;
using ShardingCore.EFCores.ChangeTrackers;
using TodoApp.Routes;
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
using Volo.Abp.TenantManagement;
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
        static TodoAppEntityFrameworkCoreModule()
        {
            TodoAppEfCoreEntityExtensionMappings.Configure();
        }
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
                options.Configure<TodoAppDbContext>(innerContext =>
                {
                    //innerContext.DbContextOptions
                    //    .ReplaceService<IChangeTrackerFactory, ShardingChangeTrackerFactory>();
                    innerContext.DbContextOptions.UseDefaultSharding<TodoAppDbContext>(innerContext.ServiceProvider);
                });
            });
            context.Services.AddShardingConfigure<TodoAppDbContext>()
                .UseRouteConfig(op =>
                {
                    op.AddShardingDataSourceRoute<TodoDataSourceRoute>();
                    op.AddShardingTableRoute<TodoTableRoute>();
                })
                .UseConfig((sp, op) =>
                {
                  
                    //var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    op.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr).UseLoggerFactory(efLogger);
                    });
                    op.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
                    });
                    op.UseShardingMigrationConfigure(builder =>
                    {
                        builder.ReplaceService<IMigrationsSqlGenerator, ShardingSqlServerMigrationsSqlGenerator>();
                    });
                    op.AddDefaultDataSource("ds0", "Server=.;Database=TodoApp;Trusted_Connection=True");
                    op.AddExtraDataSource(sp =>
                    {
                        return new Dictionary<string, string>()
                        {
                            { "ds1", "Server=.;Database=TodoApp1;Trusted_Connection=True" },
                            { "ds2", "Server=.;Database=TodoApp2;Trusted_Connection=True" }
                        };
                    });
                })
                .AddShardingCore();
        }

        public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            base.OnPostApplicationInitialization(context);
            //创建表的定时任务如果有按年月日系统默认路由的需要系统创建的记得开起来
            context.ServiceProvider.UseAutoShardingCreate();
            //补偿表 //自动迁移的话不需要
            //context.ServiceProvider.UseAutoTryCompensateTable();
        }
    }
}
