using Defra.PTS.Application.Api.Services.Interface;
using Defra.PTS.Functions.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defra.PTS.Application.Functions.Tests.Functions
{
    public class HealthCheckTest
    {
        private Mock<HttpRequest> requestMoq = new();
        private Mock<ILogger<HealthCheck>> loggerMock = new();
        private Mock<IApplicationService> applicationServiceMoq = new();
        HealthCheck? sut;

        [SetUp]
        public void SetUp()
        {
            requestMoq = new Mock<HttpRequest>();
            loggerMock = new Mock<ILogger<HealthCheck>>();
            applicationServiceMoq = new Mock<IApplicationService>();

            sut = new HealthCheck(applicationServiceMoq.Object, loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            requestMoq.Reset();
            loggerMock.Reset();
            applicationServiceMoq.Reset();

            sut = null;
        }

        [Test]
        public void HealthCheck_WhenTrue_Then_ReturnsServiceAvailable()
        {
            applicationServiceMoq.Setup(a => a.PerformHealthCheckLogic()).Returns(Task.FromResult(true));
            var result = sut!.Run(requestMoq.Object);
            var okResult = result.Result as OkResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(okResult?.StatusCode, Is.EqualTo(200));
            applicationServiceMoq.Verify(a => a.PerformHealthCheckLogic(), Times.Once);
        }

        [Test]
        public void HealthCheck_WhenFalse_Then_ReturnsServiceUnavailable()
        {
            applicationServiceMoq.Setup(a => a.PerformHealthCheckLogic()).Returns(Task.FromResult(false));
            var result = sut!.Run(requestMoq.Object);
            var okResult = result.Result as StatusCodeResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(okResult?.StatusCode, Is.EqualTo(503));
            applicationServiceMoq.Verify(a => a.PerformHealthCheckLogic(), Times.Once);
        }


     

    }
}
