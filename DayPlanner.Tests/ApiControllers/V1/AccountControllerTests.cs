using DayPlanner.Abstractions.Exceptions;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using DayPlanner.Abstractions.Stores;
using DayPlanner.Api.ApiControllers.V1;
using Google.Apis.Auth.OAuth2.Responses;
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
        private Mock<IUserStore>? _userStoreMock;
        private Mock<IJwtProvider>? _jwtProviderMock;
        private Mock<IAuthService>? _authServiceMock;
        private AccountController? _controller;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AccountController>>();
            _userStoreMock = new Mock<IUserStore>();
            _jwtProviderMock = new Mock<IJwtProvider>();
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AccountController(_loggerMock.Object);
        }

        [Test]
        public async Task GetAccountInformationAsync_UserFound_ReturnsOk()
        {
            string userId = "test-user-id";
            var user = new User { Uid = userId, Email = "test@example.com" };

            _userStoreMock!.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(user);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var result = await _controller.GetAccountInformationAsync(_userStoreMock.Object);

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
            _userStoreMock!
                .Setup(s => s.GetByIdAsync(userId))!
                .ReturnsAsync((User?)null);


            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var result = await _controller.GetAccountInformationAsync(_userStoreMock.Object);

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
            string refreshToken = "mock-refresh-token";
            _jwtProviderMock!
                .Setup(p => p.GetForCredentialsAsync(request.Email, request.Password))
                .ReturnsAsync(new Abstractions.Models.Backend.TokenResponse() { Token = token, RefreshToken = refreshToken});

            var result = await _controller!.LoginAsync(request, _jwtProviderMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<OkObjectResult>());

                var okResult = result as OkObjectResult;
                Assert.That(okResult, Is.Not.Null);

                //dynamic value = okResult!.Value!;
                //Assert.That(value, Is.Not.Null);
                //Assert.That(value.token.ToString(), Is.EqualTo(token));
                //Assert.That(value.refreshToken.ToString(), Is.EqualTo(refreshToken));
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
            _userStoreMock!.Setup(s => s.CreateAsync(request)).ReturnsAsync(user);

            var result = await _controller!.RegisterUserAsync(request, _userStoreMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<OkObjectResult>());
                var okResult = result as OkObjectResult;
                Assert.That(okResult!.Value, Is.EqualTo(user));
            });
        }

        [Test]
        public async Task RegisterUserAsync_MissingEmailAndPassword_ReturnsBadRequest()
        {
            var request = new RegisterUserRequest { Email = "", Password = "" };

            var result = await _controller!.RegisterUserAsync(request, _userStoreMock!.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());
                var apiError = badRequestResult!.Value as ApiErrorModel;
                Assert.That(apiError!.Error, Is.EqualTo("Invalid data"));
                Assert.That(apiError.Message, Is.EqualTo("Email and password are required."));
            });
        }
        [Test]
        public async Task RegisterUserAsync_ShortPassword_ReturnsBadRequest()
        {
            var request = new RegisterUserRequest { Email = "test@example.com", Password = "123" };

            var result = await _controller!.RegisterUserAsync(request, _userStoreMock!.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());
                var apiError = badRequestResult!.Value as ApiErrorModel;
                Assert.That(apiError!.Error, Is.EqualTo("Password must be at least 6 characters long."));
            });
        }
        [Test]
        public async Task RegisterUserAsync_InvalidEmailFormat_ReturnsBadRequest()
        {
            var request = new RegisterUserRequest { Email = "invalid-email", Password = "test123456" };

            var result = await _controller!.RegisterUserAsync(request, _userStoreMock!.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());
                var apiError = badRequestResult!.Value as ApiErrorModel;
                Assert.That(apiError!.Error, Is.EqualTo("Invalid email provided."));
            });
        }
        [Test]
        public async Task RegisterUserAsync_InvalidPhoneNumberFormat_ReturnsBadRequest()
        {
            var request = new RegisterUserRequest { Email = "test@example.com", Password = "test123456", PhoneNumber = "12345" };

            var result = await _controller!.RegisterUserAsync(request, _userStoreMock!.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());
                var apiError = badRequestResult!.Value as ApiErrorModel;
                Assert.That(apiError!.Error, Is.EqualTo("Invalid phone number provided."));
            });
        }

        [Test]
        public async Task RegisterUserAsync_EmailAlreadyInUse_ReturnsBadRequest()
        {
            var request = new RegisterUserRequest { Email = "existing@email.com", Password = "test123456" };

            _userStoreMock!.Setup(s => s.CreateAsync(request))
                .ThrowsAsync(new InvalidOperationException("Email already in use"));

            var result = await _controller!.RegisterUserAsync(request, _userStoreMock!.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());
                var apiError = badRequestResult!.Value as ApiErrorModel;
                Assert.That(apiError!.Error, Is.EqualTo("Email is already in use"));
            });
        }

        [Test]
        public async Task RegisterUserAsync_PhoneNumberAlreadyInUse_ReturnsBadRequest()
        {
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                Password = "test123456",
                PhoneNumber = "+4918334984823"
            };

            _userStoreMock!.Setup(s => s.CreateAsync(request))
                .ThrowsAsync(new InvalidOperationException("Phone number already in use"));

            var result = await _controller!.RegisterUserAsync(request, _userStoreMock!.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());
                var apiError = badRequestResult!.Value as ApiErrorModel;
                Assert.That(apiError!.Error, Is.EqualTo("Phone number is already in use"));
            });
        }
        [Test]
        public async Task RefreshTokenAsync_MissingRefreshToken_ReturnsBadRequest()
        {
            var result = await _controller!.RefreshTokenAsync("", _jwtProviderMock!.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());
                var apiError = badRequestResult!.Value as ApiErrorModel;
                Assert.That(apiError!.Message, Is.EqualTo("Refresh token is required."));
            });
        }

        [Test]
        public async Task RefreshTokenAsync_InvalidRefreshToken_ReturnsBadRequest()
        {
            string invalidToken = "invalid-refresh-token";
            _jwtProviderMock!.Setup(p => p.RefreshIdTokenAsync(invalidToken))
                .ThrowsAsync(new BadCredentialsException("Invalid refresh token"));

            var result = await _controller!.RefreshTokenAsync(invalidToken, _jwtProviderMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());
                var apiError = badRequestResult!.Value as ApiErrorModel;
                Assert.That(apiError!.Message, Is.EqualTo("Invalid refresh token."));
            });
        }

        [Test]
        public async Task RefreshTokenAsync_ValidRefreshToken_ReturnsNewToken()
        {
            string refreshToken = "valid-refresh-token";
            string newToken = "new-jwt-token";
            _jwtProviderMock!.Setup(p => p.RefreshIdTokenAsync(refreshToken))
                .ReturnsAsync(newToken);

            var result = await _controller!.RefreshTokenAsync(refreshToken, _jwtProviderMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<OkObjectResult>());
                var okResult = result as OkObjectResult;
                Assert.That(okResult!.Value, Is.EqualTo(newToken));
            });
        }
    }
}