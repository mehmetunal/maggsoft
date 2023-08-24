using AutoMapper;
using AutoMapper.QueryableExtensions;
using Maggsoft.Core.Extensions;
using Maggsoft.Core.Infrastructure;
using Maggsoft.Core.Mapper;
using Maggsoft.Core.Model.Pagination;
using Maggsoft.ExampleTest.Dto;
using Maggsoft.ExampleTest.Entity;
using Maggsoft.Npgsql.Repository;
using Maggsoft.Services.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Maggsoft.ExampleTest.Services
{
    public class UserService : IUserService
    {
        protected readonly IMapper Mapper;
        public readonly INpgsqlRepository<User> Repository;
        public UserService()
        {
            Mapper = EngineContext.Current.Resolve<IMapper>()
                    ?? throw new ArgumentNullException($"{nameof(IMapper)} is null");

            Repository = EngineContext.Current.Resolve<INpgsqlRepository<User>>()
                     ?? throw new ArgumentNullException($"{nameof(INpgsqlRepository<Maggsoft.ExampleTest.Entity.User>)} is null");
        }
        public async Task<IPagedList<UserResultDto>> GetAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, Expression<Func<User, object>> @order = null,
            Func<IIncludable<User>, IIncludable> @includes = null)
        {
            var query = Repository.Table;

            if (!showHidden)
                query = query.Where(p => p.IsPublish);

            query = query.Where(p => !p.IsDeleted);

            if (@includes != null)
                query = query.IncludeMultiple(@includes);

            if (@order != null)
                query = query.OrderBy(@order);
            else
                query = query.OrderBy(v => v.DisplayOrder);

            var asd = (await query.ToPagedListAsync(pageIndex, pageSize)).Map<User, UserResultDto>();

            var bb = (await query.ToPagedListAsync(pageIndex, pageSize)).ToMap<UserResultDto>();

            return asd;
        }
    }
}
