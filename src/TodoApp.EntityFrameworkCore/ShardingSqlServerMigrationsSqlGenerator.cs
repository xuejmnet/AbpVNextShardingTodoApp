using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;

namespace TodoApp
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/7/6 14:09:51
    /// Email: 326308290@qq.com
    public class ShardingSqlServerMigrationsSqlGenerator: SqlServerMigrationsSqlGenerator
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public ShardingSqlServerMigrationsSqlGenerator(IShardingRuntimeContext shardingRuntimeContext,[NotNull] MigrationsSqlGeneratorDependencies dependencies, [NotNull] IRelationalAnnotationProvider migrationsAnnotations) : base(dependencies, migrationsAnnotations)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }

        protected override void Generate(
            MigrationOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            var oldCmds = builder.GetCommandList().ToList();
            base.Generate(operation, model, builder);
            var newCmds = builder.GetCommandList().ToList();
            var addCmds = newCmds.Where(x => !oldCmds.Contains(x)).ToList();

            MigrationHelper.Generate(_shardingRuntimeContext,operation, builder, Dependencies.SqlGenerationHelper, addCmds);
        }
    }
}
