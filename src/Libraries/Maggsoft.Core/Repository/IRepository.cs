using System;
using System.Linq;
using Maggsoft.Core.Entities;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Maggsoft.Core.Repository
{
    public interface IRepository<T> where T : IEntity
    {
        
    }
}
