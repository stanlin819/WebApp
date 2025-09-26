using Moq;
using WebApp.Models.Repositories;
using WebApp.Models.Service;
using WebApp.Models.UserModel;
using Xunit;

namespace WebApp.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepo;

        private readonly Mock<ITransactionRepository> _mockTransRepo;

        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockTransRepo = new Mock<ITransactionRepository>();

            _service = new UserService(_mockRepo.Object, _mockTransRepo.Object);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnUsers()
        {
            // Arrange
            var expected = new List<User> { new User { Username = "Test", Email = "test@mail" } };
            _mockRepo.Setup(r => r.GetAll()).ReturnsAsync(expected);

            // Act
            var result = await _service.GetAllUsers();

            // Assert
            Assert.Equal(expected, result);
            _mockRepo.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetUser_ShouldReturnUser()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, Username = "Test", Email = "test@mail" };
            _mockRepo.Setup(r => r.Get(userId)).ReturnsAsync(user);

            // Act
            var result = await _service.GetUser(userId);

            // Assert
            Assert.Equal(user, result);
            _mockRepo.Verify(r => r.Get(userId), Times.Once);
        }

        [Fact]
        public async Task AddUsers_AllNewUsers_ShouldImportAll()
        {
            // Arrange
            var existingUsers = new List<User>(); // 空的代表目前系統沒有使用者
            _mockRepo.Setup(r => r.GetAll()).ReturnsAsync(existingUsers);

            var inputUsers = new List<User>
            {
                new User { Username = "alice", Email = "alice@test.com" },
                new User { Username = "bob", Email = "bob@test.com" }
            };

            // Act
            var result = await _service.AddUsers(inputUsers);

            // Assert
            _mockRepo.Verify(r => r.AddUsers(It.Is<IEnumerable<User>>(u => u.Count() == 2)), Times.Once);

            Assert.True(result.ContainsKey("Message"));
            Assert.Equal("2 users imported successfully.", result["Message"]);
            Assert.False(result.ContainsKey("Warning"));
        }

        [Fact]
        public async Task AddUsers_WithDuplicates_ShouldReturnWarning()
        {
            // Arrange
            var existingUsers = new List<User>
            {
                new User { Username = "alice", Email = "alice@existing.com" }
            };
            _mockRepo.Setup(r => r.GetAll()).ReturnsAsync(existingUsers);

            var inputUsers = new List<User>
            {
                new User { Username = "alice", Email = "alice@new.com" }, // duplicate
                new User { Username = "charlie", Email = "charlie@test.com" } // new
            };

            // Act
            var result = await _service.AddUsers(inputUsers);

            // Assert
            _mockRepo.Verify(r => r.AddUsers(It.Is<IEnumerable<User>>(u => u.Count() == 1 && u.First().Username == "charlie")), Times.Once);

            Assert.True(result.ContainsKey("Message"));
            Assert.Equal("1 users imported successfully.", result["Message"]);
            Assert.True(result.ContainsKey("Warning"));
            Assert.Contains("duplicate users", result["Warning"]);
            Assert.Contains("alice", result["Warning"]);
        }
    }
}