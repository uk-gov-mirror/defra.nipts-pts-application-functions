using Defra.PTS.Application.Api.Services.Interface;
using Defra.PTS.Application.Models.CustomException;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using applicationModel = Defra.PTS.Application.Models;
using applicationEntity = Defra.PTS.Application.Entities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Defra.PTS.Application.Functions.Functions.Application
{
    /// <summary>
    /// Get user applications
    /// </summary>
    /// <remarks>
    /// Get application details
    /// </remarks>
    public class ApplicationDetails
    {
        private readonly IApplicationService _applicationService;
        private readonly ISignatoryService _signatoryService;
        private readonly ILogger<ApplicationDetails> _logger;

        private const string _getApplicationDetailsTag = "GetApplicationDetails";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDetails"/> class.
        /// </summary>
        /// <param name="applicationService">The application service</param>
        /// <param name="signatoryService">The signatory service</param>
        /// <param name="logger">The logger</param>
        public ApplicationDetails(IApplicationService applicationService, ISignatoryService signatoryService, ILogger<ApplicationDetails> logger)
        {
            _applicationService = applicationService;
            _signatoryService = signatoryService;
            _logger = logger;
        }

        /// <summary>
        /// GetApplicationDetails
        /// </summary>
        /// <param name="req">The HTTP request</param>    
        /// <returns>The application details</returns>
        /// <exception cref="ApplicationFunctionException">Thrown when the application ID input is invalid</exception>
        [Function("GetApplicationDetails")]
        [OpenApiOperation(operationId: "GetApplicationDetails", tags: _getApplicationDetailsTag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(applicationModel.ApplicationDetail), Description = "Get Application Details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Description = "The OK response")]
        public async Task<IActionResult> GetApplicationDetails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Applications/GetApplicationDetails")] HttpRequest req)
        {
            try
            {
                var inputData = req?.Body;
                if (inputData != null)
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    applicationEntity.ApplicationDetail applicationDetail = JsonConvert.DeserializeObject<applicationEntity.ApplicationDetail>(requestBody, new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Error,
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    // Get Application Details
                    var applicationResponse = await _applicationService.GetApplicationDetails(applicationDetail.ApplicationId);
                    if (applicationResponse == null)
                    {
                        return new NotFoundObjectResult($"No application found for id {applicationDetail.ApplicationId}");
                    }

                    // Get Current Signatory
                    var signatoryResponse = await _signatoryService.GetCurrentSignatory();
                    if (signatoryResponse == null)
                    {
                        return new NotFoundObjectResult("No signatory information available");
                    }

                    // Combine Application Details and Signatory Details into one object
                    var combinedResponse = new
                    {
                        // Include all properties from applicationResponse
                        applicationResponse.ApplicationId,
                        applicationResponse.Status,
                        applicationResponse.ReferenceNumber,
                        applicationResponse.DateOfApplication,
                        applicationResponse.UserId,
                        applicationResponse.UserFullName,
                        applicationResponse.UserEmail,
                        applicationResponse.OwnerId,
                        applicationResponse.OwnerName,
                        applicationResponse.OwnerEmail,
                        applicationResponse.OwnerPhoneNumber,
                        applicationResponse.OwnerAddressId,
                        applicationResponse.OwnerAddressLineOne,
                        applicationResponse.OwnerAddressLineTwo,
                        applicationResponse.OwnerTownOrCity,
                        applicationResponse.OwnerCounty,
                        applicationResponse.OwnerPostcode,
                        applicationResponse.PetId,
                        applicationResponse.PetName,
                        applicationResponse.PetSpeciesId,
                        applicationResponse.PetBreedId,
                        applicationResponse.PetBreedName,
                        applicationResponse.PetBreedOther,
                        applicationResponse.PetGenderId,
                        applicationResponse.PetBirthDate,
                        applicationResponse.PetColourId,
                        applicationResponse.PetColourName,
                        applicationResponse.PetColourOther,
                        applicationResponse.PetHasUniqueFeature,
                        applicationResponse.PetUniqueFeature,
                        applicationResponse.MicrochipNumber,
                        applicationResponse.MicrochippedDate,
                        applicationResponse.DocumentReferenceNumber,
                        applicationResponse.DocumentIssueDate,
                        DocumentSignedBy = signatoryResponse.Name,
                        DocumentSignedByTitle = signatoryResponse.Title,
                        DocumentSignedBySignature = signatoryResponse.SignatureImage
                    };

                    return new OkObjectResult(combinedResponse);
                }

                throw new ApplicationFunctionException("Invalid Application Id input, is NULL or Empty");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Stack: {StackTrace} \n Exception Message: {Message}", ex.StackTrace, ex.Message);
                return new BadRequestObjectResult("Failed to retrieve application details");
            }
        }
    }
}
