using Defra.PTS.Application.Api.Services.Interface;
using Defra.PTS.Application.Functions.Functions.Application;
using Defra.PTS.Application.Models.CustomException;
using Defra.PTS.Application.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defra.PTS.Application.Functions.Tests.Functions.Application
{
    [TestFixture]
    public class ApplicationTests
    {
        private Mock<HttpRequest> requestMoq = new();
        private Mock<ILogger<PTS.Application.Functions.Functions.Application.Application>> loggerMock = new();
        private Mock<IApplicationService> applicationServiceMoq = new();
        private Mock<ITravelDocumentService> travelDocumentServiceMoq = new();
        PTS.Application.Functions.Functions.Application.Application? sut;

        [SetUp]
        public void SetUp()
        {
            requestMoq = new Mock<HttpRequest>();
            loggerMock = new Mock<ILogger<PTS.Application.Functions.Functions.Application.Application>>();
            applicationServiceMoq = new Mock<IApplicationService>();
            travelDocumentServiceMoq = new Mock<ITravelDocumentService>();

            sut = new PTS.Application.Functions.Functions.Application.Application(applicationServiceMoq.Object, travelDocumentServiceMoq.Object, loggerMock.Object);
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
        public async Task CreateApplication_WhenRequestDoesntExist_Then_ReturnsUserException()
        {
            var expectedResult = $"Error creating application";
            var result = await sut!.CreateApplication(null);

            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(result, Is.Not.Null);
            Assert.That(badRequestResult!.Value, Is.EqualTo(expectedResult));
        }

        [Test]
        public void CreateApplication_WhenRequestBodyExists_Then_ReturnsSuccessMessageWithValidGuid()
        {
            Task<Entities.Application> app = Task.FromResult(new Entities.Application());
            
            var json = "{\"id\":\"00000000-0000-0000-0000-000000000000\",\"petId\":\"e5edcdf5-bf5f-45c0-db18-08dbfbfaba95\",\"userId\":\"37d65eba-1d66-4181-19c5-08dbfb31348e\"," +
                "\"ownerId\":\"18c20f7c-2fa9-40bf-9468-08dbfbe90b34\",\"referenceNumber\":null,\"isDeclarationSigned\":true,\"isConsentAgreed\":true,\"isPrivacyPolicyAgreed\":true," +
                "\"dateOfApplication\":\"2023-12-13T16:44:10.0954219+00:00\",\"status\":\"AWAITING VERIFICATION\",\"createdBy\":null,\"createdOn\":\"2023-12-13T16:44:10.0954799+00:00\"," +
                "\"updatedBy\":null,\"updatedOn\":\"2023-12-13T16:44:10.0955304+00:00\",\"dynamicId\":null,\"dateAuthorised\":null,\"dateRejected\":null,\"dateRevoked\":null}";
            var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            requestMoq.Setup(a => a.Body).Returns(memoryStream);


            applicationServiceMoq.Setup(a => a.CreateApplication(It.IsAny<Entities.Application>())).Returns(app);

            var result = sut!.CreateApplication(requestMoq.Object);
            var okResult = result.Result as OkObjectResult;

            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult?.StatusCode, Is.EqualTo(200));

            applicationServiceMoq.Verify(a => a.CreateApplication(It.IsAny<Entities.Application>()), Times.Once);
        }
    }
}
