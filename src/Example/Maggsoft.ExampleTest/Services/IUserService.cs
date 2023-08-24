using Maggsoft.Core.IoC;
using Maggsoft.Core.Model.Pagination;
using Maggsoft.ExampleTest.Dto;
using Maggsoft.ExampleTest.Entity;
using Maggsoft.Services.Extensions;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Maggsoft.ExampleTest.Services
{
    public interface IUserService : IService
    {
        Task<IPagedList<UserResultDto>> GetAsync(int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false,
            Expression<Func<User, object>> @order = null,
            Func<IIncludable<User>, IIncludable> @includes = null);
    }
}
