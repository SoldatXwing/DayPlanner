using DayPlanner.Abstractions.Exceptions;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using DayPlanner.Api.ApiControllers.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace DayPlanner.Tests.ApiControllers.V1
{
    [TestFixture]
    internal class AccountControllerTests
    {
        private Mock<ILogger<AccountController>>? _loggerMock;
        private Mock<IUserService>? _userServiceMock;
        private Mock<IJwtProvider>? _jwtProviderMock;
        private Mock<IAuthService>? _authServiceMock;
        private AccountController? _controller;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AccountController>>();
            _userServiceMock = new Mock<IUserService>();
            _jwtProviderMock = new Mock<IJwtProvider>();
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AccountController(_loggerMock.Object);
        }

        [Test]
        public async Task GetAccountInformationAsync_UserFound_ReturnsOk()
        {
            string userId = "test-user-id";
            var user = new User { Uid = userId, Email = "test@example.com" };

            _userServiceMock!.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var result = await _controller.GetAccountInformationAsync(_userServiceMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<OkObjectResult>());
                var okResult = result as OkObjectResult;
                Assert.That(okResult!.Value, Is.EqualTo(user));
            });
        }

        [Test]
        public async Task GetAccountInformationAsync_UserNotFound_ReturnsNotFound()
        {
            string userId = "nonexistent-user-id";
            _userServiceMock!
                .Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);


            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var result = await _controller.GetAccountInformationAsync(_userServiceMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
                var notFoundResult = result as NotFoundObjectResult;
                Assert.That(notFoundResult!.Value, Is.InstanceOf<ApiErrorModel>());
            });
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            var request = new UserRequest { Email = "test@example.com", Password = "password123" };
            string token = "mock-jwt-token";
            _jwtProviderMock!.Setup(p => p.GetForCredentialsAsync(request.Email, request.Password)).ReturnsAsync(token);

            var result = await _controller!.LoginAsync(request, _jwtProviderMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<OkObjectResult>());
                var okResult = result as OkObjectResult;
                Assert.That(okResult!.Value, Is.EqualTo(token));
            });
        }

        [Test]
        public async Task LoginAsync_InvalidCredentials_ReturnsBadRequest()
        {
            var request = new UserRequest { Email = "test@example.com", Password = "wrong-password" };
            _jwtProviderMock!.Setup(p => p.GetForCredentialsAsync(request.Email, request.Password))
                            .ThrowsAsync(new BadCredentialsException("Invalid credentials"));

            var result = await _controller!.LoginAsync(request, _jwtProviderMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var notFoundResult = result as BadRequestObjectResult;
                Assert.That(notFoundResult!.Value, Is.InstanceOf<ApiErrorModel>());
            });
        }

        [Test]
        public async Task ValidateTokenAsync_ValidToken_ReturnsOk()
        {
            string token = "valid-token";
            string userId = "user-id";
            _authServiceMock!.Setup(s => s.VerifyTokenAsync(token)).ReturnsAsync(userId);

            var result = await _controller!.ValidateTokenAsync($"Bearer {token}", _authServiceMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<OkObjectResult>());
                var okResult = result as OkObjectResult;
                Assert.That(okResult!.Value, Is.EqualTo(userId));
            });
        }

        [Test]
        public async Task ValidateTokenAsync_InvalidToken_ReturnsUnauthorized()
        {
            string token = "invalid-token";
            _authServiceMock!.Setup(s => s.VerifyTokenAsync(token)).ReturnsAsync((string)null!);

            var result = await _controller!.ValidateTokenAsync($"Bearer {token}", _authServiceMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
            });
        }

        [Test]
        public async Task RegisterUserAsync_ValidRequest_ReturnsCreatedUser()
        {
            var request = new RegisterUserRequest { Email = "test@example.com", Password = "password123" };
            var user = new User { Uid = "new-user-id", Email = request.Email };
            _userServiceMock!.Setup(s => s.CreateUserAsync(request)).ReturnsAsync(user);

            var result = await _controller!.RegisterUserAsync(request, _userServiceMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<OkObjectResult>());
                var okResult = result as OkObjectResult;
                Assert.That(okResult!.Value, Is.EqualTo(user));
            });
        }

        [Test]
        public async Task RegisterUserAsync_InvalidRequest_ReturnsBadRequest()
        {
            var request = new RegisterUserRequest { Email = "", Password = "" };

            var result = await _controller!.RegisterUserAsync(request, _userServiceMock!.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var notFoundResult = result as BadRequestObjectResult;
                Assert.That(notFoundResult!.Value, Is.InstanceOf<ApiErrorModel>());
            });

        }
    }
}
