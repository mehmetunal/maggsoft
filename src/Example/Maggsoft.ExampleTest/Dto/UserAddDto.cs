using Maggsoft.Core.Model;
using Maggsoft.ExampleTest.Entity;
using System.Collections.Generic;

namespace Maggsoft.ExampleTest.Dto
{
    public class UserAddDto : BaseDtoModel
    {
        public string Text { get; set; }
        public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
    }
}
