using Maggsoft.Core.Base;

namespace Maggsoft.Core.Extensions
{
    public static class ResponseExtensions
    {
        public static void AddMessage(this Response response, string message)
        {
            response.Messages.Add(message);
        }
        public static void AddValidationMessages(this Response response, string message)
        {
            response.ValidationMessages.Add(message);
        }
    }
}
