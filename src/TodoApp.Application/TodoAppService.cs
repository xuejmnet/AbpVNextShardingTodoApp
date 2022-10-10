using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TodoApp.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectExtending;

namespace TodoApp
{
    public class TodoAppService : ApplicationService, ITodoAppService
    {
        private readonly IRepository<TodoItem, Guid> _todoItemRepository;
        private readonly TodoAppDbContext _dbContext;

        public TodoAppService(IRepository<TodoItem, Guid> todoItemRepository, TodoAppDbContext dbContext)
        {
            _todoItemRepository = todoItemRepository;
            _dbContext = dbContext;
        }
        
        public async Task<List<TodoItemDto>> GetListAsync()
        {
            var list =await _dbContext.TodoItems.ToListAsync();
            var changeTrackerFactory = _dbContext.GetService<IChangeTrackerFactory>();
            var changeTracker = changeTrackerFactory.Create();
            var type = _dbContext.ChangeTracker.GetType();
            var entityEntries = _dbContext.ChangeTracker.Entries<TodoItem>().ToList();
            var properties = ObjectExtensionManager.Instance
                .GetProperties(typeof(TodoItem));

            var items = await _todoItemRepository.GetListAsync();
            return items
                .Select(item => new TodoItemDto
                {
                    Id = item.Id,
                    Text = item.Text
                }).ToList();
        }

        public async Task<TodoItemDto> CreateAsync(string text)
        {
            var item = new TodoItem { Text = text };
            item.SetProperty("MyProperty", text);
            item.SetProperty("MyProperty1", text);
            var todoItem = await _todoItemRepository.InsertAsync(
                item
            );

            return new TodoItemDto
            {
                Id = todoItem.Id,
                Text = todoItem.Text
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            await _todoItemRepository.DeleteAsync(id);
        }
    }
}