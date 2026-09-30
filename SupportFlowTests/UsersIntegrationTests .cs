using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SupportFlowTests
{
    public class UsersIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public UsersIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private class IdResponse
        {
            public int Id { get; set; }
        }

        [Fact]
        public async Task PostUser_ReturnsCreated_WhenDataIsValid()
        {
            var dto = new { Name = "New User", Email = "newuser1@test.com", Password = "password123" };
            var response = await _client.PostAsJsonAsync("/api/Users", dto);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task PostUser_ReturnsBadRequest_WhenEmailAlreadyExists()
        {
            var dto = new { Name = "Dup User", Email = "dup1@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", dto);

            var response = await _client.PostAsJsonAsync("/api/Users", dto);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsOk_WithValidCredentials()
        {
            var registerDto = new { Name = "Login User", Email = "loginuser1@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerDto);

            var loginDto = new { Email = "loginuser1@test.com", Password = "password123" };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WithWrongPassword()
        {
            var registerDto = new { Name = "Login User2", Email = "loginuser2@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerDto);

            var loginDto = new { Email = "loginuser2@test.com", Password = "wrongpassword" };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_ReturnsForbidden_WhenCustomerUpdatesSomeoneElse()
        {
            var registerA = new { Name = "User A", Email = "userax@test.com", Password = "password123" };
            var createA = await _client.PostAsJsonAsync("/api/Users", registerA);

            var registerB = new { Name = "User B", Email = "userbx@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerB);

            var loginB = await _client.PostAsJsonAsync("/api/Auth/login", new { Email = "userbx@test.com", Password = "password123" });
            var tokenB = (await loginB.Content.ReadAsStringAsync()).Trim('"');
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

            var userAId = (await createA.Content.ReadFromJsonAsync<IdResponse>()).Id;
            var updateDto = new { Name = "Hacked", Email = "userax@test.com", Password = "password123" };
            var response = await _client.PutAsJsonAsync($"/api/Users/{userAId}", updateDto);

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}