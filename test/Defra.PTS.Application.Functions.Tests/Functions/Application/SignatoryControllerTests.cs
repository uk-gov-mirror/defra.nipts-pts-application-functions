using Defra.PTS.Application.Api.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Defra.PTS.Application.Models.Dto;
using Defra.PTS.Application.Functions.Application;
using System.Text.Json;

namespace Defra.PTS.Application.Functions.Tests.Controllers
{
    [TestFixture]
    public class SignatoryControllerTests
    {
        private Mock<ISignatoryService> _signatoryServiceMock = new();
        private Mock<ILogger<SignatoryController>> _loggerMock = new();
        private SignatoryController? sut;
        private const string InvalidRequestBodyMessage = "Invalid request body, is NULL or Empty";
        private const string InvalidJsonMessage = "Cannot deserialize request body or Id is missing";
        private const string InvalidJsonMessageByName = "Cannot deserialize request body or Name is missing";

        [SetUp]
        public void SetUp()
        {
            _signatoryServiceMock = new Mock<ISignatoryService>();
            _loggerMock = new Mock<ILogger<SignatoryController>>();
            sut = new SignatoryController(_signatoryServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetLatestSignatory_WhenSignatoryExists_ReturnsOkResult()
        {
            // Arrange
            var signatoryDto = new SignatoryDto
            {
                ID = Guid.NewGuid(),
                Name = "John Doe",
                Title = "Manager",
                ValidFrom = DateTime.Now.AddYears(-1),
                ValidTo = DateTime.Now.AddYears(1),
                SignatureImage = Convert.FromBase64String("aW1hZ2VfZGF0YQ==")
            };
            _signatoryServiceMock.Setup(service => service.GetLatestSignatory()).ReturnsAsync(signatoryDto);

            // Act
            var result = await sut!.GetLatestSignatory(new DefaultHttpContext().Request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(signatoryDto, okResult.Value);
        }

        [Test]
        public async Task GetCurrentSignatory_WhenSignatoryExists_ReturnsOkResult()
        {
            // Arrange
            var signatoryDto = new SignatoryDto
            {
                ID = Guid.NewGuid(),
                Name = "John Doe",
                Title = "Manager",
                ValidFrom = DateTime.Now.AddYears(-1),
                ValidTo = DateTime.Now.AddYears(1),
                SignatureImage = Convert.FromBase64String("aW1hZ2VfZGF0YQ==")
            };
            _signatoryServiceMock.Setup(service => service.GetCurrentSignatory()).ReturnsAsync(signatoryDto);

            // Act
            var result = await sut!.GetCurrentSignatory(new DefaultHttpContext().Request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(signatoryDto, okResult.Value);
        }

        [Test]
        public async Task GetLatestSignatory_WhenSignatoryDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            _signatoryServiceMock.Setup(service => service.GetLatestSignatory()).ReturnsAsync((SignatoryDto?)null);

            // Act
            var result = await sut!.GetLatestSignatory(new DefaultHttpContext().Request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
        }

        [Test]
        public async Task GetCurrentSignatory_WhenSignatoryDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            _signatoryServiceMock.Setup(service => service.GetCurrentSignatory()).ReturnsAsync((SignatoryDto?)null);

            // Act
            var result = await sut!.GetCurrentSignatory(new DefaultHttpContext().Request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
        }

        [Test]
        public async Task GetLatestSignatory_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _signatoryServiceMock
                .Setup(service => service.GetLatestSignatory())
                .ThrowsAsync(new Exception("Service exception"));

            var mockHttpRequest = new Mock<HttpRequest>();

            // Act
            var result = await sut!.GetLatestSignatory(mockHttpRequest.Object);

            // Assert
            Assert.IsInstanceOf<StatusCodeResult>(result); // Verify the result is of the correct type
            var statusCodeResult = result as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, statusCodeResult!.StatusCode);

        }

        [Test]
        public async Task GetCurrentSignatory_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _signatoryServiceMock
                .Setup(service => service.GetCurrentSignatory())
                .ThrowsAsync(new Exception("Service exception"));

            var mockHttpRequest = new Mock<HttpRequest>();

            // Act
            var result = await sut!.GetCurrentSignatory(mockHttpRequest.Object);

            // Assert
            Assert.IsInstanceOf<StatusCodeResult>(result); // Verify the result is of the correct type
            var statusCodeResult = result as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, statusCodeResult!.StatusCode);

        }

        [Test]
        public async Task GetSignatoryById_WhenSignatoryExists_ReturnsOkResult()
        {
            // Arrange
            var signatoryId = Guid.NewGuid();
            var signatoryDto = new SignatoryDto
            {
                ID = signatoryId,
                Name = "Jane Doe",
                Title = "Director",
                ValidFrom = DateTime.Now.AddYears(-2),
                ValidTo = DateTime.Now.AddYears(2),
                SignatureImage = Convert.FromBase64String("c2lnbmF0dXJlX2RhdGE=")
            };
            _signatoryServiceMock.Setup(service => service.GetSignatoryById(signatoryId)).ReturnsAsync(signatoryDto);

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestIdDto { Id = signatoryId });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryById(httpRequest);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(signatoryDto, okResult.Value);
        }

        [Test]
        public async Task GetSignatoryById_WhenSignatoryDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var signatoryId = Guid.NewGuid();
            _signatoryServiceMock.Setup(service => service.GetSignatoryById(signatoryId)).ReturnsAsync((SignatoryDto?)null);

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestIdDto { Id = signatoryId });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryById(httpRequest);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
        }

        [Test]
        public async Task GetSignatoryById_WhenServiceThrowsException_InvalidDataException_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var signatoryId = Guid.NewGuid();
            _signatoryServiceMock.Setup(service => service.GetSignatoryById(signatoryId)).ThrowsAsync(new InvalidDataException("Service exception"));

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestIdDto { Id = signatoryId });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryById(httpRequest);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var basdResult = (BadRequestObjectResult)result;
            Assert.AreEqual(InvalidRequestBodyMessage, basdResult.Value);
        }

        [Test]
        public async Task GetSignatoryById_WhenServiceThrowsException_JsonExceptionWhenReturns_EmptyGuid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var signatoryId = Guid.Empty;
            _signatoryServiceMock.Setup(service => service.GetSignatoryById(signatoryId)).ThrowsAsync(new JsonException(InvalidJsonMessage));

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestIdDto { Id = signatoryId });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryById(httpRequest);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var basdResult = (BadRequestObjectResult)result;
            Assert.AreEqual(InvalidJsonMessage, basdResult.Value);
        }


        [Test]
        public async Task GetSignatoryById_WhenServiceThrowsException_JsonException_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var signatoryId = Guid.NewGuid();
            _signatoryServiceMock.Setup(service => service.GetSignatoryById(signatoryId)).ThrowsAsync(new JsonException(InvalidJsonMessage));

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestIdDto { Id = signatoryId });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryById(httpRequest);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var basdResult = (BadRequestObjectResult)result;
            Assert.AreEqual(InvalidJsonMessage, basdResult.Value);
        }

        [Test]
        public async Task GetSignatoryById_WhenServiceThrowsException_Exception_ReturnsStatusCodeResult()
        {
            // Arrange
            var signatoryId = Guid.NewGuid();
            _signatoryServiceMock.Setup(service => service.GetSignatoryById(signatoryId)).ThrowsAsync(new Exception(InvalidJsonMessage));

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestIdDto { Id = signatoryId });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryById(httpRequest);

            // Assert
            Assert.IsInstanceOf<StatusCodeResult>(result); // Verify the result is of the correct type
            var statusCodeResult = result as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, statusCodeResult!.StatusCode);
        }


        [Test]
        public async Task GetSignatoryByName_WhenSignatoryExists_ReturnsOkResult()
        {
            // Arrange
            var name = "Jane Doe";
            var signatoryDto = new SignatoryDto
            {
                ID = Guid.NewGuid(),
                Name = name,
                Title = "Director",
                ValidFrom = DateTime.Now.AddYears(-2),
                ValidTo = DateTime.Now.AddYears(2),
                SignatureImage = Convert.FromBase64String("c2lnbmF0dXJlX2RhdGE=")
            };
            _signatoryServiceMock.Setup(service => service.GetSignatoryByName(name)).ReturnsAsync(signatoryDto);

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestNameDto { Name = name });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryByName(httpRequest);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(signatoryDto, okResult.Value);
        }

        [Test]
        public async Task GetSignatoryByName_WhenSignatoryDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var name = "Nonexistent Signatory";
            _signatoryServiceMock.Setup(service => service.GetSignatoryByName(name)).ReturnsAsync((SignatoryDto?)null);

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestNameDto { Name = name });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryByName(httpRequest);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
        }

        [Test]
        public async Task GetSignatoryByName_WhenSignatoryDoesNotExist_InvalidDataException_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var name = "Nonexistent Signatory";
            _signatoryServiceMock.Setup(service => service.GetSignatoryByName(name)).ThrowsAsync(new InvalidDataException("Service exception")); ;

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestNameDto { Name = name });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryByName(httpRequest);

            // Assert
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var basdResult = (BadRequestObjectResult)result;
            Assert.AreEqual(InvalidRequestBodyMessage, basdResult.Value);
        }

        [Test]
        public async Task GetSignatoryByName_WhenSignatoryDoesNotExist_JsonExceptionWhenReturns_EmptyGuid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var name = "";
            _signatoryServiceMock.Setup(service => service.GetSignatoryByName(name)).ThrowsAsync(new JsonException(InvalidJsonMessageByName)); ;

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestNameDto { Name = name });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryByName(httpRequest);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var basdResult = (BadRequestObjectResult)result;
            Assert.AreEqual(InvalidJsonMessageByName, basdResult.Value);
        }

        [Test]
        public async Task GetSignatoryByName_WhenSignatoryDoesNotExist_JsonException_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var name = "Nonexistent Signatory";
            _signatoryServiceMock.Setup(service => service.GetSignatoryByName(name)).ThrowsAsync(new JsonException(InvalidJsonMessageByName)); ;

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestNameDto { Name = name });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryByName(httpRequest);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var basdResult = (BadRequestObjectResult)result;
            Assert.AreEqual(InvalidJsonMessageByName, basdResult.Value);
        }

        [Test]
        public async Task GetSignatoryByName_WhenSignatoryDoesNotExist_Exception_ReturnsStatusCodeResult()
        {
            // Arrange
            var name = "Nonexistent Signatory";
            _signatoryServiceMock.Setup(service => service.GetSignatoryByName(name)).ThrowsAsync(new Exception(InvalidJsonMessageByName)); ;

            var httpRequest = new DefaultHttpContext().Request;
            var requestBody = JsonSerializer.Serialize(new SignatoryRequestNameDto { Name = name });
            httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await sut!.GetSignatoryByName(httpRequest);

            // Assert
            Assert.IsInstanceOf<StatusCodeResult>(result); // Verify the result is of the correct type
            var statusCodeResult = result as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, statusCodeResult!.StatusCode);
        }
    }
}
