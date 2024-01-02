using AutoMapper;
using AutoMapper.QueryableExtensions;
using Maggsoft.Core.Extensions;
using Maggsoft.Core.Infrastructure;
using Maggsoft.Core.Model.Pagination;
using Maggsoft.ExampleTest.Dto;
using Maggsoft.ExampleTest.Entity;
using Maggsoft.Mssql.Repository;
using Maggsoft.Services.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Maggsoft.ExampleTest.Services;

public class UserService : IUserService
{
    protected readonly IMapper Mapper;
    public readonly IMssqlRepository<User> Repository;
    public readonly IMssqlRepository<Log> LogRepository;
    public readonly DbContext DBContext;
    public UserService()
    {
        Mapper = MaggsoftContext.Current.Resolve<IMapper>()
                ?? throw new ArgumentNullException($"{nameof(IMapper)} is null");

        Repository = MaggsoftContext.Current.Resolve<IMssqlRepository<User>>()
                 ?? throw new ArgumentNullException($"{nameof(IMssqlRepository<User>)} is null");

        LogRepository = MaggsoftContext.Current.Resolve<IMssqlRepository<Log>>()
                 ?? throw new ArgumentNullException($"{nameof(IMssqlRepository<Log>)} is null");

        DBContext = MaggsoftContext.Current.Resolve<DbContext>()
                 ?? throw new ArgumentNullException($"{nameof(DbContext)} is null");
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

    public async Task<UserResultDto> AddAsync(UserAddDto addDto, bool publishEvent = true)
    {
        var user = addDto.ToEntity<User>();
        user.CreatorIP = "asdasdasd";
        user.CreatorUserId = Guid.NewGuid();
        user.CreatedDate = DateTime.UtcNow;
        var result = await Repository.AddAsync(user);

        var logEntity = new Log { CreatedDate = DateTime.UtcNow, CreatorIP = "asd", CreatorUserId = Guid.Empty, IsPublish = true, Text = "dsdfasd", UserId = user.Id };
        await LogRepository.AddAsync(logEntity);

        // await LogRepository.SaveChangesAsync();
        // await DBContext.SaveChangesAsync();

        return result.ToModel<UserResultDto>();
    }

    public Task<UserResultDto> UpdateAsync(UserEditDto editDto, bool publishEvent = true)
    {
        throw new NotImplementedException();
    }

    public Task<UserResultDto> DeleteAsync(Guid id, bool publishEvent = true)
    {
        throw new NotImplementedException();
    }
}
