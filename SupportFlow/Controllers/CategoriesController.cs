using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportFlow.DTOs;
using SupportFlow.Services;


namespace SupportFlow.Controllers
{
    [ApiController]
    [Route("api/Categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _categoryService.GetCategories());
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryById(id);
            if (category == null) return NotFound();
            return Ok(category);
        }
        [HttpPost]
        [Authorize (Roles="Admin")]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDto category)
        {
            try
            {
                var createdCategory = await _categoryService.AddCategory(category);
                return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto category)
        {
            try
            {
                var updatedCategory = await _categoryService.UpdateCategory(id, category);
                if (updatedCategory == null)
                {
                    return NotFound($"Category with ID {id} not found.");
                }
                return Ok(updatedCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategory(id);
            if (!result)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            return NoContent();
        }
    }
}
