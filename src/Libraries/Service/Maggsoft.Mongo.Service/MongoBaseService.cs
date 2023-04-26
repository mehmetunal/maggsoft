using AutoMapper;
using Maggsoft.Core.Infrastructure;
using Maggsoft.Core.Model;
using Maggsoft.Data.Mongo;
using Maggsoft.Mongo.Model.Pagination;
using Maggsoft.Mongo.Repository;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Threading.Tasks;

namespace Maggsoft.Services
{
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
        protected readonly IMongoRepository<TTable> Repository;

        #endregion

        #region Ctor

        public MongoBaseService()
        {
            Mapper = EngineContext.Current.Resolve<IMapper>()
                     ?? throw new ArgumentNullException($"{nameof(IMapper)} is null");
            Repository = EngineContext.Current.Resolve<IMongoRepository<TTable>>()
                         ?? throw new ArgumentNullException($"{nameof(IMongoRepository<TTable>)} is null");
        }

        #endregion

        #region Method

        public virtual async Task<PagedList<TResultDto>> GetAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {

            var builder = Builders<TTable>.Filter;
            var filter = builder.Where(c => true);

            if (!showHidden)
                filter = filter & builder.Where(p => p.IsPublish);

            filter = filter & builder.Where(p => !p.IsDeleted);

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

        public virtual async Task<TResultDto> AddAsync(TAddDto companyAddDto)
        {
            if (companyAddDto == null)
                throw new ArgumentNullException($"{nameof(companyAddDto)}");

            var domainEntity = Mapper.Map<TTable>(companyAddDto);

            domainEntity.CreatedDate = DateTime.UtcNow;
            domainEntity.CreatorIP = RemoteIp;

            var result = await Repository.AddAsync(domainEntity);

            // event notification
            // await _mediator.EntityInserted(vendor);

            return Mapper.Map<TResultDto>(domainEntity);
        }

        public virtual async Task<TResultDto> UpdateAsync(TEditDto companyEditDto)
        {
            if (companyEditDto == null)
                throw new ArgumentNullException($"{nameof(companyEditDto)}");

            var domainEntity = Mapper.Map<TTable>(companyEditDto);

            domainEntity.ModifiedDate = DateTime.UtcNow;
            domainEntity.ModifierIP = RemoteIp;

            // event notification
            // await _mediator.EntityUpdated(vendor);

            var result = await Repository.UpdateAsync(domainEntity);

            return Mapper.Map<TResultDto>(result);
        }

        public virtual async Task<TResultDto> DeleteAsync(object id)
        {
            // var filter = Builders<Company>.Filter.Eq("Id", id);
            var result = await Repository.DeleteAsync(id);
            // event notification
            // await _mediator.EntityDeleted(vendorNote);
            return Mapper.Map<TResultDto>(result);
        }

        #endregion

        #region Prop

        protected virtual string RemoteIp => EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext.Connection.RemoteIpAddress.ToString();

        #endregion
    }
}