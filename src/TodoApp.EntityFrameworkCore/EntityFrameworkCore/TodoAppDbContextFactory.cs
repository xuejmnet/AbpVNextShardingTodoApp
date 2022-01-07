using System.IO;
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
                )
                .AddEntityConfig(op =>
                {
                    op.CreateShardingTableOnStart = false;
                    op.EnsureCreatedWithOutShardingTable = false;
                    op.UseShardingQuery(
                        (conn, o) =>
                            o.UseSqlServer(conn, x => x.MigrationsAssembly("TodoApp.EntityFrameworkCore"))
                                .ReplaceService<IMigrationsSqlGenerator,
                                    ShardingSqlServerMigrationsSqlGenerator<TodoAppDbContext>>());
                    op.UseShardingTransaction((connection, builder) =>
                        builder.UseSqlServer(connection));
                    op.AddShardingTableRoute<ToDoItemVirtualTableRoute>();
                })
                .AddConfig(op =>
                {
                    op.ConfigId = "c1";
                    op.AddDefaultDataSource("ds0",
                        configuration.GetConnectionString("Default"));
                }).EnsureConfig();
            services.AddLogging();
            var buildServiceProvider = services.BuildServiceProvider();
            ShardingContainer.SetServices(buildServiceProvider);
            ShardingContainer.GetService<IShardingBootstrapper>().Start();
        }

        public TodoAppDbContext CreateDbContext(string[] args)
        {
            return ShardingContainer.GetService<TodoAppDbContext>();
        }
    }
}
