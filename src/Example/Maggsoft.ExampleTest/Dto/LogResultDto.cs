using Maggsoft.Core.Model;
using System;

namespace Maggsoft.ExampleTest.Dto
{
    public class LogResultDto : BaseDtoModel
    {
        public string Text { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedDate { get;  set; }
    }
}
