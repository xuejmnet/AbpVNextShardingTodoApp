using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace TodoApp.Routes
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/7/6 13:57:45
    /// Email: 326308290@qq.com
    public class TodoTableRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<TodoItem>
    {
        public TodoTableRoute() : base(2, 5)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<TodoItem> builder)
        {
            builder.ShardingProperty(o => o.Text);
        }
    }
}
