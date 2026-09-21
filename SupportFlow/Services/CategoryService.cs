using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.DTOs;
using SupportFlow.Models;

namespace SupportFlow.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly SupportFlowDbContext _context;
        public CategoryService(SupportFlowDbContext context)
        {
            _context = context;
        }
        public async Task<List<CategoryResponseDto>> GetCategories()
        {
            var categorys = await _context.Categories.Include(c => c.Tickets).ToListAsync();
            var result = new List<CategoryResponseDto>();
            foreach(var c in categorys)
            {
                var ticketDtos = new List<TicketSumaryDto>();
                foreach(var t in c.Tickets)
                {
                    ticketDtos.Add(new TicketSumaryDto { Id = t.Id, Title = t.Title });
                };
                var categoryDto = new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Tickets = ticketDtos
                };
                result.Add(categoryDto);
            }
            return result;
        }
        public async Task<CategoryResponseDto?> GetCategoryById(int id)
        {
            var c = await _context.Categories.Include(c => c.Tickets).FirstOrDefaultAsync(c => c.Id == id);
            if (c == null) return null;
            var ticketDtos = new List<TicketSumaryDto>();
            foreach(var t in c.Tickets)
            {
                ticketDtos.Add(new TicketSumaryDto { Id = t.Id, Title = t.Title });
            };
            var categoryDto = new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Tickets = ticketDtos
            };
            return categoryDto;
        }
        public async Task<CategoryResponseDto> AddCategory(CategoryDto category)
        {
            var newCategory = new Category
            {
                Name = category.Name
            };
            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();
            return new CategoryResponseDto
            {
                Id = newCategory.Id,
                Name = newCategory.Name,
                Tickets = new List<TicketSumaryDto>()
            };
        }
        public async Task<CategoryResponseDto?> UpdateCategory(int id, CategoryDto category)
        {
            var existingCategory = await _context.Categories.Include(c =>c.Tickets).FirstOrDefaultAsync(c => c.Id ==id);
            if (existingCategory == null) return null;
            existingCategory.Name = category.Name;
            await _context.SaveChangesAsync();
            var ticketDtos = new List<TicketSumaryDto>();
            foreach(var t in existingCategory.Tickets)
            {
                ticketDtos.Add(new TicketSumaryDto { Id = t.Id, Title = t.Title });
            };
            var categoryDto = new CategoryResponseDto
            {
                Id = existingCategory.Id,
                Name = existingCategory.Name,
                Tickets = ticketDtos
            };
            return categoryDto;

        }
        public async Task<bool> DeleteCategory(int id)
        {
            var c = await _context.Categories.FindAsync(id);
            if (c == null) return false;
            _context.Categories.Remove(c);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
