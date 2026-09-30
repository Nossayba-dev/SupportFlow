using Microsoft.Extensions.DependencyInjection;
using SupportFlow.Data;
using SupportFlow.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SupportFlowTests
{
    public class TicketsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public TicketsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private SupportFlowDbContext GetDbContext()
        {
            var scope = _factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<SupportFlowDbContext>();
        }

        private class IdResponse
        {
            public int Id { get; set; }
        }

        [Fact]
        public async Task GetTickets_ReturnsUnauthorized_WhenNoTokenProvided()
        {
            var response = await _client.GetAsync("/api/Tickets");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetTickets_ReturnsOk_WhenValidTokenProvided()
        {
            var registerDto = new { Name = "Test User", Email = "integration@test.com", Password = "password123" };
            var registerResponse = await _client.PostAsJsonAsync("/api/Users", registerDto);
            Assert.Equal(System.Net.HttpStatusCode.Created, registerResponse.StatusCode);

            var loginDto = new { Email = "integration@test.com", Password = "password123" };
            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            Assert.Equal(System.Net.HttpStatusCode.OK, loginResponse.StatusCode);

            var token = (await loginResponse.Content.ReadAsStringAsync()).Trim('"');
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Tickets");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetTickets_ReturnsOnlyOwnTickets_WhenCustomer()
        {
            var context = GetDbContext();
            var category = new Category { Name = "Hardware" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var registerA = new { Name = "User A", Email = "usera@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerA);
            var loginA = await _client.PostAsJsonAsync("/api/Auth/login", new { Email = "usera@test.com", Password = "password123" });
            var tokenA = (await loginA.Content.ReadAsStringAsync()).Trim('"');

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            var ticketDtoA = new { Title = "A's ticket", CategoryId = category.Id, Priority = 0, Comments = "test" };
            var createResponseA = await _client.PostAsJsonAsync("/api/Tickets", ticketDtoA);
            Assert.Equal(System.Net.HttpStatusCode.Created, createResponseA.StatusCode);

            var registerB = new { Name = "User B", Email = "userb@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerB);
            var loginB = await _client.PostAsJsonAsync("/api/Auth/login", new { Email = "userb@test.com", Password = "password123" });
            var tokenB = (await loginB.Content.ReadAsStringAsync()).Trim('"');

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
            var ticketDtoB = new { Title = "B's ticket", CategoryId = category.Id, Priority = 0, Comments = "test" };
            await _client.PostAsJsonAsync("/api/Tickets", ticketDtoB);

            var response = await _client.GetAsync("/api/Tickets");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("B's ticket", content);
            Assert.DoesNotContain("A's ticket", content);
        }

        [Fact]
        public async Task PostTicket_ReturnsCreated_WhenValidDataAndAuthenticated()
        {
            var context = GetDbContext();
            var category = new Category { Name = "Hardware" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var registerA = new { Name = "User A", Email = "usera2@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerA);
            var loginA = await _client.PostAsJsonAsync("/api/Auth/login", new { Email = "usera2@test.com", Password = "password123" });
            var tokenA = (await loginA.Content.ReadAsStringAsync()).Trim('"');

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            var ticketDto = new { Title = "A's ticket", CategoryId = category.Id, Priority = 0, Comments = "test" };
            var createResponse = await _client.PostAsJsonAsync("/api/Tickets", ticketDto);

            Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        }

        [Fact]
        public async Task PostTicket_ReturnsUnauthorized_WhenNoToken()
        {
            var ticketDto = new { Title = "Some ticket", CategoryId = 1, Priority = 0, Comments = "test" };
            var response = await _client.PostAsJsonAsync("/api/Tickets", ticketDto);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetTicketById_ReturnsForbidden_WhenCustomerRequestsSomeoneElsesTicket()
        {
            var context = GetDbContext();
            var category = new Category { Name = "Hardware" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            // User A creates a ticket
            var registerA = new { Name = "User A", Email = "usera3@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerA);
            var loginA = await _client.PostAsJsonAsync("/api/Auth/login", new { Email = "usera3@test.com", Password = "password123" });
            var tokenA = (await loginA.Content.ReadAsStringAsync()).Trim('"');

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            var ticketDto = new { Title = "A's private ticket", CategoryId = category.Id, Priority = 0, Comments = "test" };
            var createResponse = await _client.PostAsJsonAsync("/api/Tickets", ticketDto);
            var createdTicket = await createResponse.Content.ReadFromJsonAsync<IdResponse>();

            // User B logs in and tries to access User A's ticket
            var registerB = new { Name = "User B", Email = "userb3@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerB);
            var loginB = await _client.PostAsJsonAsync("/api/Auth/login", new { Email = "userb3@test.com", Password = "password123" });
            var tokenB = (await loginB.Content.ReadAsStringAsync()).Trim('"');

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
            var response = await _client.GetAsync($"/api/Tickets/{createdTicket.Id}");

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTicket_ReturnsNoContent_WhenCustomerDeletesOwnTicket()
        {
            var context = GetDbContext();
            var category = new Category { Name = "Hardware" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var registerA = new { Name = "User A", Email = "usera4@test.com", Password = "password123" };
            await _client.PostAsJsonAsync("/api/Users", registerA);
            var loginA = await _client.PostAsJsonAsync("/api/Auth/login", new { Email = "usera4@test.com", Password = "password123" });
            var tokenA = (await loginA.Content.ReadAsStringAsync()).Trim('"');

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            var ticketDto = new { Title = "To delete", CategoryId = category.Id, Priority = 0, Comments = "test" };
            var createResponse = await _client.PostAsJsonAsync("/api/Tickets", ticketDto);
            var createdTicket = await createResponse.Content.ReadFromJsonAsync<IdResponse>();

            var deleteResponse = await _client.DeleteAsync($"/api/Tickets/{createdTicket.Id}");

            Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }
    }
}