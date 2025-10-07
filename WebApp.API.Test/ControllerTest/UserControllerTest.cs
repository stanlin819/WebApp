using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApp.API.Service;
using Xunit;

namespace WebApp.API.Tests.Controllers
{
    /// <summary>
    /// UserAPIController 單元測試類別
    /// </summary>
    public class UserAPIControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserAPIController _controller;

        /// <summary>
        /// 測試初始化
        /// </summary>
        public UserAPIControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserAPIController(_mockUserService.Object);
        }

        #region GetAll Tests

        /// <summary>
        /// 測試 GetAll 方法 - 成功返回所有用戶
        /// </summary>
        [Fact]
        public async Task GetAll_ReturnsOkResult_WithUserList()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                new User { Id = 1, Username = "user1", Email = "user1@test.com" },
                new User { Id = 2, Username = "user2", Email = "user2@test.com" }
            };
            _mockUserService.Setup(s => s.GetAllUsers()).ReturnsAsync(expectedUsers);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
            Assert.Equal(2, users.Count());
        }

        #endregion

        #region Get Tests

        /// <summary>
        /// 測試 Get 方法 - 成功返回指定用戶
        /// </summary>
        [Fact]
        public async Task Get_ExistingUser_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            var expectedUser = new User { Id = userId, Username = "testuser", Email = "test@test.com" };
            _mockUserService.Setup(s => s.GetUser(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _controller.Get(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var user = Assert.IsType<User>(okResult.Value);
            Assert.Equal(userId, user.Id);
        }

        /// <summary>
        /// 測試 Get 方法 - 用戶不存在時返回 NotFound
        /// </summary>
        [Fact]
        public async Task Get_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            _mockUserService.Setup(s => s.GetUser(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Get(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region AddUser Tests

        /// <summary>
        /// 測試 AddUser 方法 - 成功新增用戶
        /// </summary>
        [Fact]
        public async Task AddUser_ValidUser_ReturnsOkResult()
        {
            // Arrange
            var newUser = new User { Username = "newuser", Email = "new@test.com" };
            var addedUser = new User { Id = 1, Username = "newuser", Email = "new@test.com" };
            _mockUserService.Setup(s => s.AddUser(It.IsAny<User>())).ReturnsAsync(addedUser);

            // Act
            var result = await _controller.AddUser(newUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var user = Assert.IsType<User>(okResult.Value);
            Assert.Equal(newUser.Username, user.Username);
        }

        /// <summary>
        /// 測試 AddUser 方法 - 用戶名重複時的處理
        /// </summary>
        [Fact]
        public async Task AddUser_DuplicateUsername_ReturnsOkResult()
        {
            // Arrange
            var duplicateUser = new User { Username = "existinguser", Email = "existing@test.com" };
            _mockUserService.Setup(s => s.AddUser(It.IsAny<User>())).ReturnsAsync((User)null);

            // Act
            var result = await _controller.AddUser(duplicateUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(duplicateUser, okResult.Value);
        }

        #endregion

        #region DeleteUser Tests

        /// <summary>
        /// 測試 DeleteUser 方法 - 成功刪除用戶
        /// </summary>
        [Fact]
        public async Task DeleteUser_ExistingUser_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            var existingUser = new User { Id = userId, Username = "testuser" };
            var deleteMessage = "testuser deleted successfully";
            
            _mockUserService.Setup(s => s.GetUser(userId)).ReturnsAsync(existingUser);
            _mockUserService.Setup(s => s.DeleteUser(userId)).ReturnsAsync(deleteMessage);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic responseValue = okResult.Value;
            Assert.Equal(deleteMessage, responseValue.GetType().GetProperty("message").GetValue(responseValue));
        }

        /// <summary>
        /// 測試 DeleteUser 方法 - 用戶不存在時返回 NotFound
        /// </summary>
        [Fact]
        public async Task DeleteUser_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            _mockUserService.Setup(s => s.GetUser(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            dynamic responseValue = notFoundResult.Value;
            Assert.Contains($"User with ID {userId} not found", 
                responseValue.GetType().GetProperty("message").GetValue(responseValue).ToString());
        }

        #endregion

        #region UpdateUser Tests

        /// <summary>
        /// 測試 UpdateUser 方法 - 成功更新用戶
        /// </summary>
        [Fact]
        public async Task UpdateUser_ValidUser_ReturnsOkResult()
        {
            // Arrange
            var userToUpdate = new User { Id = 1, Username = "updateduser", Email = "updated@test.com" };
            var updateMessage = "updateduser edited successfully.";
            _mockUserService.Setup(s => s.UpdateUser(It.IsAny<User>())).ReturnsAsync(updateMessage);

            // Act
            var result = await _controller.UpdateUser(userToUpdate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic responseValue = okResult.Value;
            Assert.Equal(updateMessage, responseValue.GetType().GetProperty("message").GetValue(responseValue));
        }

        /// <summary>
        /// 測試 UpdateUser 方法 - 用戶不存在時返回 NotFound
        /// </summary>
        [Fact]
        public async Task UpdateUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userToUpdate = new User { Id = 999, Username = "nonexistent", Email = "none@test.com" };
            var errorMessage = "User not found";
            _mockUserService.Setup(s => s.UpdateUser(It.IsAny<User>())).ReturnsAsync(errorMessage);

            // Act
            var result = await _controller.UpdateUser(userToUpdate);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            dynamic responseValue = notFoundResult.Value;
            Assert.Equal(errorMessage, responseValue.GetType().GetProperty("message").GetValue(responseValue));
        }

        /// <summary>
        /// 測試 UpdateUser 方法 - 用戶名重複時返回 BadRequest
        /// </summary>
        [Fact]
        public async Task UpdateUser_DuplicateUsername_ReturnsBadRequest()
        {
            // Arrange
            var userToUpdate = new User { Id = 1, Username = "duplicateuser", Email = "duplicate@test.com" };
            var errorMessage = "UserName already exists";
            _mockUserService.Setup(s => s.UpdateUser(It.IsAny<User>())).ReturnsAsync(errorMessage);

            // Act
            var result = await _controller.UpdateUser(userToUpdate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            dynamic responseValue = badRequestResult.Value;
            Assert.Equal(errorMessage, responseValue.GetType().GetProperty("message").GetValue(responseValue));
        }

        #endregion

        #region Service Verification Tests

        /// <summary>
        /// 驗證服務方法是否被正確調用
        /// </summary>
        [Fact]
        public async Task GetAll_CallsUserService_GetAllUsers()
        {
            // Arrange & Act
            await _controller.GetAll();

            // Assert
            _mockUserService.Verify(s => s.GetAllUsers(), Times.Once);
        }

        /// <summary>
        /// 驗證 Get 方法是否正確調用服務
        /// </summary>
        [Fact]
        public async Task Get_CallsUserService_GetUser()
        {
            // Arrange
            var userId = 1;

            // Act
            await _controller.Get(userId);

            // Assert
            _mockUserService.Verify(s => s.GetUser(userId), Times.Once);
        }

        #endregion
    }
}