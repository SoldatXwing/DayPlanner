using DayPlanner.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Diagnostics;

namespace DayPlanner.Tests.Middleware
{
    [TestFixture]
    public class TraceIdMiddlewareTests
    {
        [Test]
        public async Task InvokeAsync_ShouldAddTraceIdToHttpContextAndResponseHeaders()
        {
            var context = new DefaultHttpContext();
            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            var middleware = new TraceIdMiddleware(nextMock.Object);

            // Start an Activity to ensure Activity.Current is set
            using var activity = new Activity("TestActivity");
            activity.Start();

            // Act
            await middleware.InvokeAsync(context);

            Assert.Multiple(() =>
            {
                // Verify TraceId is added to HttpContext.Items
                Assert.That(context.Items.ContainsKey("TraceId"), Is.True);
                Assert.That(context.Items["TraceId"], Is.EqualTo(activity.TraceId.ToString()));

                // Verify TraceId is added to response headers
                Assert.That(context.Response.Headers.ContainsKey("Trace-Id"), Is.True);
                Assert.That(context.Response.Headers["Trace-Id"], Is.EqualTo(activity.TraceId.ToString()));
            });

            // Verify the next middleware in the pipeline is called
            nextMock.Verify(next => next(context), Times.Once);
        }

        [Test]
        public void InvokeAsync_ShouldThrowArgumentNullException_WhenNextIsNull()
        {
            RequestDelegate? next = null;

            Assert.Throws<ArgumentNullException>(() => new TraceIdMiddleware(next!));
        }
    }


}
