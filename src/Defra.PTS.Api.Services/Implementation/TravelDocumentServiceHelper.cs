using Defra.PTS.Application.Api.Services.Interface;
using Defra.PTS.Application.Models.Constants;

namespace Defra.PTS.Application.Api.Services.Implementation
{
    public class TravelDocumentServiceHelper : ITravelDocumentServiceHelper
    {
        public string GenerateUniqueAlphaNumericCode(int length)
        {
            string result;
            do
            {
                result = GetUniqueCode(length);
            } while (ConditionSatisfied(result));
            return "GB826" + result;
        }

        public static string GetUniqueCode(int length)
        {
            const string chars = ApplicationConstant.AlphaNumericBase;            
            Guid guid = Guid.NewGuid();
            byte[] bytes = guid.ToByteArray();

            char[] codeArray = new char[length];

            for (int i = 0; i < length; i++)            {

                codeArray[i] = chars[bytes[i] % chars.Length];
            }
            return new string(codeArray);
        }

        
        static bool ConditionSatisfied(string result)
        {
            return result.StartsWith("AD", StringComparison.Ordinal) || result.Any(c => c >= 'G' && c <= 'Z');
        }
    }
    

}
