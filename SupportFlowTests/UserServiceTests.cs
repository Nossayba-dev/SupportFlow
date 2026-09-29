using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.DTOs;
using SupportFlow.Enums;
using SupportFlow.Models;
using SupportFlow.Services;

namespace SupportFlowTests
{
    public class UserServiceTests
    {
        private SupportFlowDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SupportFlowDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            return new SupportFlowDbContext(options);
        }
        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            // Arrange: create context, add 2 users, save
            var context = CreateContext();
            var user1 = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var user2 = new User { Id = 2, Name = "User B", Email = "b@test.com", Password = "hashed", Role = UserRole.Customer };
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            // Act: call service.GetUsers()
            var service = new UserService(context);
            var result = await service.GetUsers();
            // Assert: result.Count == 2
            Assert.Equal(2, result.Count);            
        }

        [Fact]
        public async Task GetUsers_ReturnsEmptyList_WhenNoUsersExist()
        {
            // Arrange: empty context
            var context = CreateContext();
            // Act: call service.GetUsers()
            var service = new UserService(context);
            var result = await service.GetUsers();
            // Assert: result is empty
            Assert.Empty(result);
        }

        // ---------- GetUserById ----------

        [Fact]
        public async Task GetUserById_ReturnsNull_WhenUserDoesNotExist()
        {
            var context = CreateContext();
            var service = new UserService(context);

            var result = await service.GetUserById(999); // Non-existent ID

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserById_ReturnsUser_WhenUserExists()
        {
            var context = CreateContext();
            var user = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            
            context.Users.AddRange(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);
            var result = await service.GetUserById(1);
            Assert.NotNull(result);
            Assert.Equal("User A", result.Name);
        }


        // ---------- AddUser ----------

        [Fact]
        public async Task AddUser_CreatesUser_WithHashedPassword_WhenEmailIsUnique()
        {
            var context = CreateContext();
            var service = new UserService(context);
            var user = new UserDto
            {
                Name = "New User",
                Email = "new@test.com",
                Password = "newtest1",
            };
            var result = await service.AddUser(user);
            Assert.NotNull(result);
            Assert.Equal("New User", result.Name);

            var storedUser = await context.Users.FindAsync(result.Id);
            Assert.NotEqual("newtest1", storedUser.Password);

            Assert.True(BCrypt.Net.BCrypt.Verify("newtest1", storedUser.Password));
            
        }

        [Fact]
        public async Task AddUser_ThrowsArgumentException_WhenEmailAlreadyExists()
        {
            var context = CreateContext();
            var existingUser = new User { Id = 1, Name = "Existing User", Email = "ex@test.com", Password = "hashed", Role = UserRole.Customer };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var service = new UserService(context);
            var newUser = new UserDto
            {
                Name = "New User",
                Email = "ex@test.com", // Duplicate email
                Password = "newtest1",
            };

            await Assert.ThrowsAsync<ArgumentException>(
                 () => service.AddUser(newUser)
            );

        }

        [Fact]
        public async Task AddUser_SetsRoleToCustomer_ByDefault()
        {
            var context = CreateContext();
            var service = new UserService(context);
            var user = new UserDto
            {
                Name = "New User",
                Email = "new@test.com",
                Password = "newtest1"
            };
            await service.AddUser(user);
            Assert.Equal(UserRole.Customer, (await context.Users.FirstAsync()).Role);
        }

        // ---------- UpdateUser ----------

        [Fact]
        public async Task UpdateUser_ReturnsNull_WhenUserDoesNotExist()
        {
            var context = CreateContext();
            var service = new UserService(context);
            var updateDto = new UserDto
            {
                Name = "Updated Name",
                Email = "up@test.com",
                Password = "updatedpass"
            };

            var result = await service.UpdateUser(999, updateDto);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUser_UpdatesFields_WhenDataIsValid()
        {
            var context = CreateContext();
            var user = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };


            context.Users.AddRange(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);
            var updateUser = new UserDto
            {
                Name = "Updated Name",
                Email = "aa@test.com",
                Password = "updatedpass"
            };

            var result = await service.UpdateUser(1, updateUser);

            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
        }

        [Fact]
        public async Task UpdateUser_ThrowsArgumentException_WhenEmailBelongsToAnotherUser()
        {
            var context = CreateContext();
            var userA = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            var userB = new User { Id = 2, Name = "User B", Email = "b@test.com", Password = "hashed", Role = UserRole.Customer };


            context.Users.AddRange(userA,userB);
            await context.SaveChangesAsync();

            var service = new UserService(context);
            var updateUser = new UserDto
            {
                Name = "Updated Name",
                Email = "a@test.com",
                Password = "updatedpass"
            };
            
            await Assert.ThrowsAsync<ArgumentException>(
                 () => service.UpdateUser(2, updateUser)
            );

        }

        [Fact]
        public async Task UpdateUser_Succeeds_WhenEmailUnchanged()
        {
            // This tests the specific bug you fixed: updating without changing
            // your own email should NOT incorrectly throw a duplicate-email error
            var context = CreateContext();
            var user = new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };

            context.Users.AddRange(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);
            var updateUser = new UserDto
            {
                Name = "Updated Name",
                Email = "a@test.com",
                Password = "updatedpass"
            };
            var result = await service.UpdateUser(1, updateUser);
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
        }

        // ---------- DeleteUser ----------

        [Fact]
        public async Task DeleteUser_ReturnsFalse_WhenUserDoesNotExist()
        {
            var context = CreateContext();
            var service = new UserService(context);
            var result = await service.DeleteUser(999); // Non-existent ID
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsTrue_AndRemovesUser_WhenUserExists()
        {
            var context = CreateContext();
            var user= new User { Id = 1, Name = "User A", Email = "a@test.com", Password = "hashed", Role = UserRole.Customer };
            


            context.Users.AddRange(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);
            var result = await service.DeleteUser(1);

            Assert.Null(await context.Users.FindAsync(1));
            Assert.True(result);
        }
    }
}
