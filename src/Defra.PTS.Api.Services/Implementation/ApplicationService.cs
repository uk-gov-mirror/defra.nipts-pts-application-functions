using Defra.PTS.Application.Repositories.Interfaces;
using Defra.PTS.Application.Api.Services.Interface;
using Microsoft.Extensions.Logging;
using applicationModel = Defra.PTS.Application.Models;
using applicationEntity = Defra.PTS.Application.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Defra.PTS.Application.Repositories.Implementation;
using Defra.PTS.Application.Models.Enums;
using Defra.PTS.Application.Models.CustomException;
using Newtonsoft.Json;
using System.Security.Principal;
using Defra.PTS.Application.Models.Helper;
using Defra.PTS.Application.Entities;
using Defra.PTS.Application.Repositories;
using Defra.PTS.Application.Models.Dto;

namespace Defra.PTS.Application.Api.Services.Implementation
{
    public class ApplicationService(
          ILogger<ApplicationService> log
            , IApplicationRepository applicationRepository
            , IReferenceGeneratorService referenceGeneratorService) : IApplicationService
    {
        private readonly ILogger<ApplicationService> _log = log;
        private readonly IApplicationRepository _applicationRepository = applicationRepository;
        private readonly IReferenceGeneratorService _referenceGeneratorService = referenceGeneratorService;

        public async Task<applicationEntity.Application> CreateApplication(applicationEntity.Application application)
        {

            var uniqueReferenceNumber = await _referenceGeneratorService.GetUniqueApplicationReference();

            if (string.IsNullOrEmpty(uniqueReferenceNumber))
            {
                throw new ApplicationFunctionException("Cannot create Unique Application Reference");
            }

            application.ReferenceNumber = uniqueReferenceNumber;

            await _applicationRepository.Add(application);
            await _applicationRepository.SaveChanges();

            return application;
        }

        public applicationEntity.Application GetApplication(Guid id)
        {
            _log.LogInformation("Running inside method {MethodName}", nameof(GetApplication));
            return _applicationRepository.GetApplication(id);
        }

        public async Task<IEnumerable<ApplicationSummaryDto>> GetApplicationsForUser(Guid userId)
        {
            var result = await _applicationRepository.GetApplicationsForUser(userId);
            return result.Select(x => new ApplicationSummaryDto
            {
                ApplicationId = x.ApplicationId,
                OwnerName = x.OwnerName ?? string.Empty,
                PetName = x.PetName ?? string.Empty,
                PetSpeciesId = x.PetSpeciesId.GetValueOrDefault(),
                ReferenceNumber = x.ReferenceNumber ?? string.Empty,
                Status = x.Status ?? string.Empty,
                DocumentIssueDate = x.DocumentIssueDate,
                DateOfApplication = x.DateOfApplication,
            });
        }

        public async Task<VwApplication?> GetApplicationDetails(Guid applicationId)
        {
            return await _applicationRepository.GetApplicationDetails(applicationId);
        }

        public async Task<bool> PerformHealthCheckLogic()
        {
              return await _applicationRepository.PerformHealthCheckLogic();
        }
    }
}
