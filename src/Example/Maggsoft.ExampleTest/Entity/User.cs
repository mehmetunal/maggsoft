using Maggsoft.Data.Mssql;
using System.Collections.Generic;

namespace Maggsoft.ExampleTest.Entity;

public class User : BaseEntity
{
    public string Text { get; set; }

    public virtual ICollection<UserLog> UserLogs { get; set; } = new List<UserLog>();
}
