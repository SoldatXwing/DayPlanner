using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using DayPlanner.Abstractions.Stores;
using DayPlanner.Api.ApiControllers.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace DayPlanner.Tests.ApiControllers.V1
{
    [TestFixture]
    internal class AppointmentControllerTests
    {
        private Mock<IAppointmentStore>? _appointmentStoreMock;
        private Mock<ILogger<AppointmentController>>? _loggerMock;
        private AppointmentController? _controller;

        [SetUp]
        public void SetUp()
        {
            _appointmentStoreMock = new Mock<IAppointmentStore>();
            _loggerMock = new Mock<ILogger<AppointmentController>>();
            _controller = new AppointmentController(_appointmentStoreMock.Object, _loggerMock.Object);
        }


        [Test]
        public async Task GetAllAppointmentsAsync_ValidUser_ReturnsOk()
        {
            var userId = "test-user-id";
            var appointments = new List<Appointment> { new Appointment { Title = "Test Appointment", UserId = userId } };
            _appointmentStoreMock!.Setup(s => s.GetAppointmentsCount(userId)).ReturnsAsync(1);
            _appointmentStoreMock.Setup(s => s.GetUsersAppointments(userId, 1, 10)).ReturnsAsync(appointments);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var result = await _controller.GetAllAppointmentsAsync(1, 10);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<OkObjectResult>());
                var okResult = result as OkObjectResult;
                var response = okResult!.Value as PaginatedResponse<Appointment>;
                Assert.That(response!.Items, Is.EquivalentTo(appointments));
            });
        }
        [Test]
        public async Task GetAllAppointmentsAsync_UnauthorizedUser_ReturnsUnauthorized()
        {
            var userId = "test-user-id";
            _appointmentStoreMock!
                .Setup(s => s.GetUsersAppointments(userId, 1, 10))
                .ThrowsAsync(new UnauthorizedAccessException());

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var result = await _controller.GetAllAppointmentsAsync(1, 10);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task CreateAppointmentAsync_ValidRequest_ReturnsCreated()
        {
            var userId = "test-user-id";
            var request = new AppointmentRequest { Title = "Test Appointment", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) };
            var appointment = new Appointment { Title = "Test Appointment", UserId = userId };

            _appointmentStoreMock!.Setup(s => s.CreateAppointment(userId, request)).ReturnsAsync(appointment);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var result = await _controller.CreateAppointmentAsync(request);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<CreatedResult>());
                var createdResult = result as CreatedResult;
                Assert.That(createdResult!.Value, Is.EqualTo(appointment));
            });
        }
        [Test]
        public async Task CreateAppointmentAsync_InvalidRequest_ReturnsBadRequest()
        {
            var userId = "test-user-id";
            var request = new AppointmentRequest
            {
                Title = "",
                Start = DateTime.MinValue,  // Invalid: Start is not set
                End = DateTime.MinValue    // Invalid: End is not set
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("user_id", userId) }));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var result = await _controller.CreateAppointmentAsync(request);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());

                var errorModel = badRequestResult.Value as ApiErrorModel;
                Assert.That(errorModel!.Message, Is.EqualTo("Invalid request attributes"));
                Assert.That(errorModel.Error, Is.EqualTo("At least one attribute is not valid."));
            });
        }

        [Test]
        public async Task DeleteAppointmentAsync_ValidRequest_ReturnsNoContent()
        {
            var userId = "test-user-id";
            var appointmentId = "1";

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            _appointmentStoreMock!.Setup(s => s.DeleteAppointment(userId, appointmentId)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteAppointmentAsync(appointmentId);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }
        [Test]
        public async Task DeleteAppointmentAsync_UnauthorizedUser_ReturnsForbid()
        {
            var userId = "test-user-id";
            var appointmentId = "appointment-id";

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            _appointmentStoreMock!
                .Setup(s => s.DeleteAppointment(userId, appointmentId))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.DeleteAppointmentAsync(appointmentId);

            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }
        [Test]
        public async Task DeleteAppointmentAsync_AppointmentNotFound_ReturnsNotFound()
        {
            var userId = "test-user-id";
            var appointmentId = "appointment-id";

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            _appointmentStoreMock!
                .Setup(s => s.DeleteAppointment(userId, appointmentId))
                .ThrowsAsync(new InvalidOperationException("Appointment not found"));

            var result = await _controller.DeleteAppointmentAsync(appointmentId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
                var notFoundResult = result as NotFoundObjectResult;
                Assert.That(notFoundResult!.Value, Is.InstanceOf<ApiErrorModel>());
                var errorModel = notFoundResult.Value as ApiErrorModel;
                Assert.That(errorModel!.Message, Is.EqualTo("Appointment not found"));
                Assert.That(errorModel.Error, Is.EqualTo("Appointment not found"));
            });
        }

        [Test]
        public async Task GetAppointmentsByDateAsync_InvalidDateRange_ReturnsBadRequest()
        {
            var start = DateTime.UtcNow.AddDays(1);
            var end = DateTime.UtcNow;

            var result = await _controller!.GetAppointmentsByDateAsync(start, end);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());
            });
        }

        [Test]
        public async Task UpdateAppointmentAsync_ValidRequest_ReturnsUpdatedAppointment()
        {
            var userId = "test-user-id";
            var appointmentId = "1";
            var request = new AppointmentRequest { Title = "Updated Title", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) };
            var updatedAppointment = new Appointment { Id = appointmentId, Title = "Updated Title", UserId = userId };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            _appointmentStoreMock!.Setup(s => s.UpdateAppointment(appointmentId, userId, request)).ReturnsAsync(updatedAppointment);

            var result = await _controller.UpdateAppointmentAsync(appointmentId, request);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<OkObjectResult>());
                var okResult = result as OkObjectResult;
                Assert.That(okResult!.Value, Is.EqualTo(updatedAppointment));
            });
        }
        [Test]
        public async Task UpdateAppointmentAsync_InvalidRequest_ReturnsBadRequest()
        {
            var userId = "test-user-id";
            var appointmentId = "appointment-id";
            var request = new AppointmentRequest
            {
                Title = "",
                Start = DateTime.MinValue, // Invalid: Start is not set
                End = DateTime.MinValue   // Invalid: End is not set
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var result = await _controller.UpdateAppointmentAsync(appointmentId, request);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                var badRequestResult = result as BadRequestObjectResult;
                Assert.That(badRequestResult!.Value, Is.InstanceOf<ApiErrorModel>());

                var errorModel = badRequestResult.Value as ApiErrorModel;
                Assert.That(errorModel!.Message, Is.EqualTo("Invalid request attributes"));
                Assert.That(errorModel.Error, Is.EqualTo("At least one attribute is not valid."));
            });
        }
        [Test]
        public async Task UpdateAppointmentAsync_UnauthorizedUser_ReturnsForbid()
        {
            var userId = "test-user-id";
            var appointmentId = "appointment-id";

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", userId)]));
            _controller!.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var updateRequest = new AppointmentRequest { Title = "Valid appointment", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) };

            _appointmentStoreMock!
                .Setup(s => s.UpdateAppointment(appointmentId, userId, updateRequest))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.UpdateAppointmentAsync(appointmentId, updateRequest);

            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }




    }
}






