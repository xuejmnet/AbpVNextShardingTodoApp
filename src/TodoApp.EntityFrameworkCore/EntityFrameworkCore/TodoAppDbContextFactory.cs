﻿using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Bootstrapers;
using TodoApp.VirtualRoutes;

namespace TodoApp.EntityFrameworkCore
{
    /* This class is needed for EF Core console commands
    * (like Add-Migration and Update-Database commands) */
    public class TodoAppDbContextFactory : IDesignTimeDbContextFactory<TodoAppDbContext>
    {
        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TodoApp.DbMigrator/"))
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }

        static TodoAppDbContextFactory()
        {
            var services = new ServiceCollection();
            var configuration = BuildConfiguration();
            services.AddShardingDbContext<TodoAppDbContext>(
                    (conn, o) =>
                        o.UseSqlServer(conn,x=>x.MigrationsAssembly("TodoApp.EntityFrameworkCore"))
                            .ReplaceService<IMigrationsSqlGenerator, ShardingSqlServerMigrationsSqlGenerator<TodoAppDbContext>>()
                ).Begin(o =>
                {
                    o.CreateShardingTableOnStart = false;
                    o.EnsureCreatedWithOutShardingTable = false;
                    o.AutoTrackEntity = true;
                })
                .AddShardingTransaction((connection, builder) =>
                    builder.UseSqlServer(connection))
                .AddDefaultDataSource("ds0",
                    configuration.GetConnectionString("Default"))
                .AddShardingTableRoute(o =>
                {
                    o.AddShardingTableRoute<ToDoItemVirtualTableRoute>();
                }).End();
            services.AddLogging();
            var buildServiceProvider = services.BuildServiceProvider();
            ShardingContainer.SetServices(buildServiceProvider);
            new ShardingBootstrapper(buildServiceProvider).Start();
        }

        public TodoAppDbContext CreateDbContext(string[] args)
        {
            return ShardingContainer.GetService<TodoAppDbContext>();
        }
    }
}
