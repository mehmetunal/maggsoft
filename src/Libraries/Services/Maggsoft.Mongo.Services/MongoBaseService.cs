using AutoMapper;
using Maggsoft.Core.Infrastructure;
using Maggsoft.Core.Model;
using Maggsoft.Data.Mongo;
using Maggsoft.Mongo.Model.Pagination;
using Maggsoft.Mongo.Repository;
using Maggsoft.Services.Events;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Threading.Tasks;

namespace Maggsoft.Services;

public partial class MongoBaseService<TTable, TResultDto, TEditDto, TAddDto> where TTable : BaseEntity,
    new()
    where TResultDto : BaseDtoModel, 
    new()
    where TEditDto : BaseDtoModel,
    new()
    where TAddDto : BaseDtoModel
{
    #region Properties

    protected readonly IMapper Mapper;
    private readonly IEventPublisher EventPublisher;
    protected readonly IMongoRepository<TTable> Repository;

    #endregion

    #region Ctor

    public MongoBaseService()
    {
        Mapper = MaggsoftContext.Current.Resolve<IMapper>()
                 ?? throw new ArgumentNullException($"{nameof(IMapper)} is null");

        EventPublisher = MaggsoftContext.Current.Resolve<IEventPublisher>()
                 ?? throw new ArgumentNullException($"{nameof(IEventPublisher)} is null");

        Repository = MaggsoftContext.Current.Resolve<IMongoRepository<TTable>>()
                     ?? throw new ArgumentNullException($"{nameof(IMongoRepository<TTable>)} is null");
    }

    #endregion

    #region Method

    public virtual async Task<PagedList<TResultDto>> GetAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {

        var builder = Builders<TTable>.Filter;
        var filter = builder.Where(c => true);

        if (!showHidden)
            filter &= builder.Where(p => p.IsPublish);

        filter &= builder.Where(p => !p.IsDeleted);

        var builderSort = Builders<TTable>.Sort.Descending(x => x.DisplayOrder);

        var query = Repository.Collection;

        var result = await PagedList<TTable>.Create(query, filter, builderSort, pageIndex, pageSize);

        return Mapper.Map<PagedList<TResultDto>>(result);
    }

    public virtual async Task<TResultDto> GetByIdAsync(object id)
    {
        var result = await Repository.FindByIdAsync(id);
        return Mapper.Map<TResultDto>(result);
    }

    public virtual async Task<int> CountAsync()
    {
        return await Repository.Table.CountAsync();
    }

    public virtual async Task<TResultDto> AddAsync(TAddDto companyAddDto, bool publishEvent = true)
    {
        if (companyAddDto == null)
            throw new ArgumentNullException($"{nameof(companyAddDto)}");

        var domainEntity = Mapper.Map<TTable>(companyAddDto);

        domainEntity.CreatedDate = DateTime.UtcNow;
        domainEntity.CreatorIP = RemoteIp;

        var result = await Repository.AddAsync(domainEntity);

        // event notification
        // await _mediator.EntityInserted(vendor);
        if (publishEvent)
            await EventPublisher.EntityInsertedAsync(result);


        return Mapper.Map<TResultDto>(domainEntity);
    }

    public virtual async Task<TResultDto> UpdateAsync(TEditDto companyEditDto, bool publishEvent = true)
    {
        if (companyEditDto == null)
            throw new ArgumentNullException($"{nameof(companyEditDto)}");

        var domainEntity = Mapper.Map<TTable>(companyEditDto);

        domainEntity.ModifiedDate = DateTime.UtcNow;
        domainEntity.ModifierIP = RemoteIp;

        // event notification
        // await _mediator.EntityUpdated(vendor);
        if (publishEvent)
            await EventPublisher.EntityUpdatedAsync(domainEntity);

        var result = await Repository.UpdateAsync(domainEntity);

        return Mapper.Map<TResultDto>(result);
    }

    public virtual async Task<TResultDto> DeleteAsync(object id, bool publishEvent = true)
    {
        // var filter = Builders<Company>.Filter.Eq("Id", id);
        var result = await Repository.DeleteAsync(id);

        // event notification
        // await _mediator.EntityDeleted(vendorNote);
        if (publishEvent)
            await EventPublisher.EntityDeletedAsync(result);

        return Mapper.Map<TResultDto>(result);
    }

    #endregion

    #region Prop

    protected virtual string RemoteIp => MaggsoftContext.Current.Resolve<IHttpContextAccessor>().HttpContext.Connection.RemoteIpAddress.ToString();

    #endregion
}