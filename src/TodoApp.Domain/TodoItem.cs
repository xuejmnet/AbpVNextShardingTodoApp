using System;
using ShardingCore.Core;
using Volo.Abp.Domain.Entities;

namespace TodoApp
{
    public class TodoItem : BasicAggregateRoot<Guid>,IShardingTable,IShardingKeyIsGuId
    {
        [ShardingTableKey]
        public override Guid Id { get; protected set; }
        public string Text { get; set; }
    }
}