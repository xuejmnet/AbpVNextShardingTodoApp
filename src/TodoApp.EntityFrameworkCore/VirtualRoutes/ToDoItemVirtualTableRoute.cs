using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace TodoApp.VirtualRoutes
{
    public class ToDoItemVirtualTableRoute: AbstractSimpleShardingModKeyGuidVirtualTableRoute<TodoItem>
    {
        public ToDoItemVirtualTableRoute() : base(2, 5)
        {
        }
        public override void Configure(EntityMetadataTableBuilder<TodoItem> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }
}
