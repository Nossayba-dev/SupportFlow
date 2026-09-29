using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.DTOs;
using SupportFlow.Enums;
using SupportFlow.Models;
using SupportFlow.Services;

namespace SupportFlowTests
{
    public class TicketServiceTests
    {
        // Helper method: creates a fresh, isolated in-memory context for each test
        private SupportFlowDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SupportFlowDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            return new SupportFlowDbContext(options);
        }

        // ---------- GetTickets ----------

        [Fact]
        public async Task GetTickets_ReturnsOnlyOwnTickets_WhenCustomer()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var user2 = new User { Id = 2, Name = "User B", Email = "b@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.AddRange(user1, user2);
            context.Categories.Add(category);
            context.Tickets.Add(new Ticket { Title = "Ticket A", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "A's issue" });
            context.Tickets.Add(new Ticket { Title = "Ticket B", User = user2, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "B's issue" });
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var result = await service.GetTickets(1, UserRole.Customer);

            Assert.Single(result);
            Assert.Equal("Ticket A", result[0].Title);
        }

        [Fact]
        public async Task GetTickets_ReturnsAllTickets_WhenAgent()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var user2 = new User { Id = 2, Name = "User B", Email = "b@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.AddRange(user1, user2);
            context.Categories.Add(category);
            context.Tickets.Add(new Ticket { Title = "Ticket A", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "A's issue" });
            context.Tickets.Add(new Ticket { Title = "Ticket B", User = user2, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "B's issue" });
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var result = await service.GetTickets(999, UserRole.Agent); // 999 = agent's own id, irrelevant here

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTickets_ReturnsAllTickets_WhenAdmin()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.Add(user1);
            context.Categories.Add(category);
            context.Tickets.Add(new Ticket { Title = "Ticket A", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "A's issue" });
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var result = await service.GetTickets(999, UserRole.Admin);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetTickets_ReturnsEmptyList_WhenNoTicketsExist()
        {
            var context = CreateContext();
            var service = new TicketService(context);

            var result = await service.GetTickets(1, UserRole.Admin);

            Assert.Empty(result);
        }

        // ---------- GetTicketById ----------

        [Fact]
        public async Task GetTicketById_ReturnsNull_WhenTicketDoesNotExist()
        {
            var context = CreateContext();
            var service = new TicketService(context);

            var result = await service.GetTicketById(999, 1, UserRole.Admin);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTicketById_ReturnsTicket_WhenCustomerRequestsOwnTicket()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.Add(user1);
            context.Categories.Add(category);
            var ticket = new Ticket { Title = "Ticket A", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "A's issue" };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var result = await service.GetTicketById(ticket.Id, 1, UserRole.Customer);

            Assert.NotNull(result);
            Assert.Equal("Ticket A", result.Title);
        }

        [Fact]
        public async Task GetTicketById_ThrowsUnauthorized_WhenCustomerRequestsSomeoneElsesTicket()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var user2 = new User { Id = 2, Name = "User B", Email = "b@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.AddRange(user1, user2);
            context.Categories.Add(category);
            var ticket = new Ticket { Title = "Ticket A", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "A's issue" };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var service = new TicketService(context);

            // User 2 (Customer) tries to view User 1's ticket
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.GetTicketById(ticket.Id, 2, UserRole.Customer)
            );
        }

        [Fact]
        public async Task GetTicketById_ReturnsTicket_WhenAgentRequestsAnyTicket()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.Add(user1);
            context.Categories.Add(category);
            var ticket = new Ticket { Title = "Ticket A", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "A's issue" };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var result = await service.GetTicketById(ticket.Id, 999, UserRole.Agent);

            Assert.NotNull(result);
        }

        // ---------- AddTicket ----------

        [Fact]
        public async Task AddTicket_CreatesTicket_WithOpenStatus_WhenCategoryIsValid()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.Add(user1);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var dto = new CreateTicketDto { Title = "New issue", CategoryId = category.Id, Priority = TicketPriority.Medium, Comments = "Something broke" };

            var result = await service.AddTicket(dto, user1.Id);

            Assert.Equal("New issue", result.Title);
            Assert.Equal(TicketStatus.Open, result.Status); // always defaults to Open
        }

        [Fact]
        public async Task AddTicket_ThrowsArgumentException_WhenCategoryDoesNotExist()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            context.Users.Add(user1);
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var dto = new CreateTicketDto { Title = "New issue", CategoryId = 999, Priority = TicketPriority.Medium, Comments = "Something broke" };

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.AddTicket(dto, user1.Id)
            );
        }

        // ---------- UpdateTicket ----------

        [Fact]
        public async Task UpdateTicket_ReturnsNull_WhenTicketDoesNotExist()
        {
            var context = CreateContext();
            var service = new TicketService(context);
            var dto = new UpdateTicketDto { Title = "Updated", UserId = 1, CategoryId = 1, Priority = TicketPriority.Low, Status = TicketStatus.Open, Comments = "..." };

            var result = await service.UpdateTicket(999, dto, 1, UserRole.Admin);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTicket_UpdatesFields_WhenCustomerUpdatesOwnTicket()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.Add(user1);
            context.Categories.Add(category);
            var ticket = new Ticket { Title = "Old title", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "Old comment" };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var dto = new UpdateTicketDto { Title = "New title", UserId = user1.Id, CategoryId = category.Id, Priority = TicketPriority.High, Status = TicketStatus.InProgress, Comments = "New comment" };

            var result = await service.UpdateTicket(ticket.Id, dto, user1.Id, UserRole.Customer);

            Assert.NotNull(result);
            Assert.Equal("New title", result.Title);
            Assert.Equal(TicketStatus.InProgress, result.Status);
        }

        [Fact]
        public async Task UpdateTicket_ThrowsUnauthorized_AndDoesNotChangeData_WhenCustomerUpdatesSomeoneElsesTicket()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var user2 = new User { Id = 2, Name = "User B", Email = "b@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.AddRange(user1, user2);
            context.Categories.Add(category);
            var ticket = new Ticket { Title = "Original title", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "Original comment" };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var dto = new UpdateTicketDto { Title = "Hacked title", UserId = user1.Id, CategoryId = category.Id, Priority = TicketPriority.High, Status = TicketStatus.Closed, Comments = "Hacked" };

            // User 2 tries to update User 1's ticket
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.UpdateTicket(ticket.Id, dto, user2.Id, UserRole.Customer)
            );

            // Confirm the ticket's data was NOT changed by the failed attempt
            var unchangedTicket = await context.Tickets.FindAsync(ticket.Id);
            Assert.Equal("Original title", unchangedTicket.Title);
        }

        [Fact]
        public async Task UpdateTicket_Succeeds_WhenAgentUpdatesAnyTicket()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.Add(user1);
            context.Categories.Add(category);
            var ticket = new Ticket { Title = "Old title", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "Old comment" };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var dto = new UpdateTicketDto { Title = "Agent updated", UserId = user1.Id, CategoryId = category.Id, Priority = TicketPriority.High, Status = TicketStatus.Resolved, Comments = "Fixed" };

            var result = await service.UpdateTicket(ticket.Id, dto, 999, UserRole.Agent);

            Assert.NotNull(result);
            Assert.Equal("Agent updated", result.Title);
        }

        [Fact]
        public async Task UpdateTicket_ThrowsArgumentException_WhenCategoryIsInvalid()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.Add(user1);
            context.Categories.Add(category);
            var ticket = new Ticket { Title = "Old title", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "Old comment" };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var dto = new UpdateTicketDto { Title = "Updated", UserId = user1.Id, CategoryId = 999, Priority = TicketPriority.High, Status = TicketStatus.Open, Comments = "..." };

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UpdateTicket(ticket.Id, dto, user1.Id, UserRole.Customer)
            );
        }

        // ---------- DeleteTicket ----------

        [Fact]
        public async Task DeleteTicket_ReturnsFalse_WhenTicketDoesNotExist()
        {
            var context = CreateContext();
            var service = new TicketService(context);

            var result = await service.DeleteTicket(999, 1, UserRole.Admin);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTicket_ReturnsTrue_AndRemovesTicket_WhenCustomerDeletesOwnTicket()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.Add(user1);
            context.Categories.Add(category);
            var ticket = new Ticket { Title = "To delete", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "..." };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var result = await service.DeleteTicket(ticket.Id, user1.Id, UserRole.Customer);

            Assert.True(result);
            Assert.Null(await context.Tickets.FindAsync(ticket.Id));
        }

        [Fact]
        public async Task DeleteTicket_ThrowsUnauthorized_AndDoesNotDelete_WhenCustomerDeletesSomeoneElsesTicket()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var user2 = new User { Id = 2, Name = "User B", Email = "b@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.AddRange(user1, user2);
            context.Categories.Add(category);
            var ticket = new Ticket { Title = "Protected", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "..." };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var service = new TicketService(context);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.DeleteTicket(ticket.Id, user2.Id, UserRole.Customer)
            );

            // Confirm the ticket still exists
            Assert.NotNull(await context.Tickets.FindAsync(ticket.Id));
        }

        [Fact]
        public async Task DeleteTicket_ReturnsTrue_WhenAdminDeletesAnyTicket()
        {
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var category = new Category { Id = 1, Name = "Test Category" };
            context.Users.Add(user1);
            context.Categories.Add(category);
            var ticket = new Ticket { Title = "To delete", User = user1, Category = category, Status = TicketStatus.Open, Priority = TicketPriority.Low, Comments = "..." };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var service = new TicketService(context);
            var result = await service.DeleteTicket(ticket.Id, 999, UserRole.Admin);

            Assert.True(result);
        }
    }
}