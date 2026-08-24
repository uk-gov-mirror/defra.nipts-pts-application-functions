using Defra.PTS.Application.Repositories.Interfaces;
using Defra.PTS.Application.Api.Services.Interface;
using modelEntity = Defra.PTS.Application.Entities;
using Defra.PTS.Application.Models.CustomException;
using Defra.PTS.Application.Entities;

namespace Defra.PTS.Application.Api.Services.Implementation
{
    public class TravelDocumentService(
        ITravelDocumentRepository travelDocumentRepository
            , IReferenceGeneratorService referenceGeneratorService) : ITravelDocumentService
    {        
        private readonly ITravelDocumentRepository _travelDocumentRepository = travelDocumentRepository;
        private readonly IReferenceGeneratorService _referenceGeneratorService = referenceGeneratorService;

        public async Task<TravelDocument> CreateTravelDocument(modelEntity.Application application)
        {
            var travelDocumentReference = await _referenceGeneratorService.GetTravelDocumentReference();

            if (string.IsNullOrEmpty(travelDocumentReference))
            {
                throw new ApplicationFunctionException("Cannot create travel document");
            }

            var travelDocument = await GetTravelDocumentAsync(application);
            travelDocument.DocumentReferenceNumber = travelDocumentReference;

            await _travelDocumentRepository.Add(travelDocument);
            await _travelDocumentRepository.SaveChanges();

            return travelDocument;
        }

        public static Task<TravelDocument> GetTravelDocumentAsync(modelEntity.Application application)
        {
            var travelDocument = new TravelDocument
            {
                ApplicationId = application.Id,
                PetId = application.PetId,
                OwnerId = application.OwnerId,
                CreatedOn = application.CreatedOn,
                CreatedBy = application.CreatedBy,
                IsLifeTIme = true,
                DocumentSignedBy = "John Smith (APHA)"
            };
            return Task.FromResult(travelDocument);
        }
    }
}
