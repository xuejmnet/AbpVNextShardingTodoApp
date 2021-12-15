using System;
using Volo.Abp.Domain.Entities;

namespace TodoApp
{
    public class TodoItem : AggregateRoot<Guid>,IShardingKeyIsGuId
    {
        public override Guid Id { get; protected set; }
        public string Text { get; set; }
    }
}