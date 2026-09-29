using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.DTOs;
using SupportFlow.Models;
using SupportFlow.Services;

namespace SupportFlowTests
{
    public class CategoryServiceTests
    {
        private SupportFlowDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SupportFlowDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            return new SupportFlowDbContext(options);
        }

        // ---------- GetCategories ----------

        [Fact]
        public async Task GetCategories_ReturnsAllCategories()
        {
            var context = CreateContext();
            context.Categories.AddRange(
                new Category { Id = 1, Name = "Hardware" },
                new Category { Id = 2, Name = "Software" }
            );
            await context.SaveChangesAsync();

            var service = new CategoryService(context);
            var result = await service.GetCategories();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetCategories_ReturnsEmptyList_WhenNoCategoriesExist()
        {
            var context = CreateContext();
            var service = new CategoryService(context);

            var result = await service.GetCategories();

            Assert.Empty(result);
        }

        // ---------- GetCategoryById ----------

        [Fact]
        public async Task GetCategoryById_ReturnsNull_WhenCategoryDoesNotExist()
        {
            var context = CreateContext();
            var service = new CategoryService(context);

            var result = await service.GetCategoryById(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsCategory_WhenCategoryExists()
        {
            var context = CreateContext();
            var category = new Category { Id = 1, Name = "Hardware" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);
            var result = await service.GetCategoryById(1);

            Assert.NotNull(result);
            Assert.Equal("Hardware", result.Name);
        }

        // ---------- AddCategory ----------

        [Fact]
        public async Task AddCategory_CreatesCategory_WhenNameIsUnique()
        {
            var context = CreateContext();
            var service = new CategoryService(context);
            var dto = new CategoryDto { Name = "Networking" };

            var result = await service.AddCategory(dto);

            Assert.NotNull(result);
            Assert.Equal("Networking", result.Name);
        }

        [Fact]
        public async Task AddCategory_ThrowsArgumentException_WhenNameAlreadyExists()
        {
            var context = CreateContext();
            context.Categories.Add(new Category { Id = 1, Name = "Hardware" });
            await context.SaveChangesAsync();

            var service = new CategoryService(context);
            var dto = new CategoryDto { Name = "Hardware" };

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.AddCategory(dto)
            );
        }

        // ---------- UpdateCategory ----------

        [Fact]
        public async Task UpdateCategory_ReturnsNull_WhenCategoryDoesNotExist()
        {
            var context = CreateContext();
            var service = new CategoryService(context);
            var dto = new CategoryDto { Name = "Updated" };

            var result = await service.UpdateCategory(999, dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCategory_UpdatesName_WhenDataIsValid()
        {
            var context = CreateContext();
            var category = new Category { Id = 1, Name = "Old Name" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);
            var dto = new CategoryDto { Name = "New Name" };

            var result = await service.UpdateCategory(1, dto);

            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
        }

        [Fact]
        public async Task UpdateCategory_Succeeds_WhenNameUnchanged()
        {
            var context = CreateContext();
            var category = new Category { Id = 1, Name = "Hardware" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);
            var dto = new CategoryDto { Name = "Hardware" }; // same name, unchanged

            var result = await service.UpdateCategory(1, dto);

            Assert.NotNull(result);
            Assert.Equal("Hardware", result.Name);
        }

        [Fact]
        public async Task UpdateCategory_ThrowsArgumentException_WhenNameBelongsToAnotherCategory()
        {
            var context = CreateContext();
            context.Categories.AddRange(
                new Category { Id = 1, Name = "Hardware" },
                new Category { Id = 2, Name = "Software" }
            );
            await context.SaveChangesAsync();

            var service = new CategoryService(context);
            var dto = new CategoryDto { Name = "Hardware" }; // Category 2 trying to steal Category 1's name

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UpdateCategory(2, dto)
            );
        }

        // ---------- DeleteCategory ----------

        [Fact]
        public async Task DeleteCategory_ReturnsFalse_WhenCategoryDoesNotExist()
        {
            var context = CreateContext();
            var service = new CategoryService(context);

            var result = await service.DeleteCategory(999);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsTrue_AndRemovesCategory_WhenCategoryExists()
        {
            var context = CreateContext();
            var category = new Category { Id = 1, Name = "Hardware" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);
            var result = await service.DeleteCategory(1);

            Assert.True(result);
            Assert.Null(await context.Categories.FindAsync(1));
        }
    }
}