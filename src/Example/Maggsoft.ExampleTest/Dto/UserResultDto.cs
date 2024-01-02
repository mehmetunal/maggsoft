using Maggsoft.Core.Model;
using System;
using System.Collections.Generic;

namespace Maggsoft.ExampleTest.Dto
{
    public class UserResultDto : BaseDtoModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; }

        public virtual ICollection<LogResultDto> Logs { get; set; } = new List<LogResultDto>();
    }
}
