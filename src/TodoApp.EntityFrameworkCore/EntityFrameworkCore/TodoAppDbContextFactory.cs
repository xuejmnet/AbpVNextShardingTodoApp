using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore;
using TodoApp.Routes;

namespace TodoApp.EntityFrameworkCore
{
    /* This class is needed for EF Core console commands
    * (like Add-Migration and Update-Database commands) */
    public class TodoAppDbContextFactory : IDesignTimeDbContextFactory<TodoAppDbContext>
    {
        private static IServiceProvider _serviceProvider;
        static TodoAppDbContextFactory()
        {
            var services = new ServiceCollection();

            services.AddShardingDbContext<TodoAppDbContext>()
                .UseRouteConfig(op =>
                {
                    op.AddShardingDataSourceRoute<TodoDataSourceRoute>();
                    op.AddShardingTableRoute<TodoTableRoute>();
                })
                .UseConfig((sp, op) =>
                {
                    op.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr);
                    });
                    op.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseSqlServer(connection);
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
            _serviceProvider = services.BuildServiceProvider();
        }
        public TodoAppDbContext CreateDbContext(string[] args)
        {
            TodoAppEfCoreEntityExtensionMappings.Configure();



            return _serviceProvider.GetService<TodoAppDbContext>();
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TodoApp.DbMigrator/"))
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }
    }
}
