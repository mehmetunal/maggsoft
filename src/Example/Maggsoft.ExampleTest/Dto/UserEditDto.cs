using Maggsoft.Core.Model;
using Maggsoft.ExampleTest.Entity;
using System;
using System.Collections.Generic;

namespace Maggsoft.ExampleTest.Dto
{
    public class UserEditDto : BaseDtoModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public virtual ICollection<UserLog> UserLogs { get; set; } = new List<UserLog>();
    }
}
