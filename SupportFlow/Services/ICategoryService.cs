using SupportFlow.DTOs; 

namespace SupportFlow.Services
{
    public interface ICategoryService
    {
        public Task<List<CategoryResponseDto>> GetCategories();
        public Task<CategoryResponseDto?> GetCategoryById(int id);
        public Task<CategoryResponseDto> AddCategory(CategoryDto category);
        public Task<CategoryResponseDto?> UpdateCategory(int id, CategoryDto category);
        public Task<bool> DeleteCategory(int id);
    }
}
