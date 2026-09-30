using Microsoft.Extensions.DependencyInjection;
using SupportFlow.Data;
using SupportFlow.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SupportFlowTests
{
    public class CategoriesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public CategoriesIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private SupportFlowDbContext GetDbContext()
        {
            var scope = _factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<SupportFlowDbContext>();
        }

        private async Task<string> RegisterAndLoginAsAdminAsync(string email)
        {
            var context = GetDbContext();

            var registerDto = new { Name = "Admin User", Email = email, Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerDto);

            // Promote the freshly created user to Admin directly in the DB
            var user = context.Users.First(u => u.Email == email);
            user.Role = SupportFlow.Enums.UserRole.Admin;
            await context.SaveChangesAsync();

            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new { Email = email, Password = "password123" });
            var token = (await loginResponse.Content.ReadAsStringAsync()).Trim('"');
            return token;
        }

        [Fact]
        public async Task GetCategories_ReturnsUnauthorized_WhenNoTokenProvided()
        {
            var response = await _client.GetAsync("/api/Categories");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PostCategory_ReturnsCreated_WhenAdmin()
        {
            var token = await RegisterAndLoginAsAdminAsync("admin1@test.com");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dto = new { Name = "Networking" };
            var response = await _client.PostAsJsonAsync("/api/Categories", dto);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task PostCategory_ReturnsForbidden_WhenCustomer()
        {
            var registerDto = new { Name = "Regular User", Email = "regular1@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerDto);

            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new { Email = "regular1@test.com", Password = "password123" });
            var token = (await loginResponse.Content.ReadAsStringAsync()).Trim('"');
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dto = new { Name = "Should Fail" };
            var response = await _client.PostAsJsonAsync("/api/Categories", dto);

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetCategories_ReturnsOk_WhenAuthenticated()
        {
            var registerDto = new { Name = "Regular User", Email = "regular2@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerDto);

            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new { Email = "regular2@test.com", Password = "password123" });
            var token = (await loginResponse.Content.ReadAsStringAsync()).Trim('"');
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Categories");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}