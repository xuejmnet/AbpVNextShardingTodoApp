using System;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.TenantManagement;

namespace TodoApp
{
    //不做时间分片所以不需要提前赋值
    public class TodoItem : AggregateRoot<Guid>,IShardingKeyIsGuId//,IShardingKeyIsCreationTime
    {
        public string Text { get; set; }
    }
}