using System;
using System.Collections.Generic;

namespace Maggsoft.Core.Base
{
    public partial class Response<T> : Response where T : class
    {
        private T _result;

        public Response()
        {
            Result = Activator.CreateInstance<T>();
        }

        public T Result
        {
            get => _result;
            set => _result = value;
        }
    }

    public class Response : IResponse
    {
        public Response()
        {
            Messages = new List<string>();
            ValidationMessages = new List<string>();
        }

        public List<string> Messages { get; set; }
        public string SystemError { get; set; }
        public List<string> ValidationMessages { get; set; }
        public int StatusCode { get; set; }
        public DateTime TimeStamp { get; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public bool IsError { get; set; }
        public string ApiVersion { get; set; }
    }
}
