using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IIS.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;
using ShardingCore.Helpers;

namespace TodoApp.Routes
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/7/6 14:39:07
    /// Email: 326308290@qq.com
    public class TodoDataSourceRoute:AbstractShardingOperatorVirtualDataSourceRoute<TodoItem,string>
    {
        public override string ShardingKeyToDataSourceName(object shardingKey)
        {
            if (shardingKey == null) throw new InvalidOperationException("sharding key cant null");
            var stringHashCode = ShardingCoreHelper.GetStringHashCode(shardingKey.ToString());
            return $"ds{(Math.Abs(stringHashCode) % 3)}";//ds0,ds1,ds2
        }

        public override List<string> GetAllDataSourceNames()
        {
            return new List<string>()
            {
                "ds0", "ds1", "ds2"
            };
        }

        public override bool AddDataSourceName(string dataSourceName)
        {
            throw new NotImplementedException();
        }

        public override void Configure(EntityMetadataDataSourceBuilder<TodoItem> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }

        public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = ShardingKeyToDataSourceName(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
                    return tail => true;
                }
            }
        }
    }
}
