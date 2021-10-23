using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.VirtualRoutes.Mods;

namespace TodoApp.VirtualRoutes
{
    public class ToDoItemVirtualTableRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<TodoItem>
    {
        public ToDoItemVirtualTableRoute() : base(2, 5)
        {
        }
    }
}
