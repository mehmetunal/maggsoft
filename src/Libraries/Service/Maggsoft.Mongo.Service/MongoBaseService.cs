using AutoMapper;
using Maggsoft.Core.Infrastructure;
using Maggsoft.Core.Model;
using Maggsoft.Core.Repository;
using Maggsoft.Data.Mongo;
using Maggsoft.Mongo.Model.Pagination;
using Maggsoft.Mongo.Repository;
using Maggsoft.Services.Events;
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
        protected readonly IMongoWriteRepository<TTable> WriteRepository;
        protected readonly IMongoReadRepository<TTable> ReadRepository;
        private readonly IEventPublisher EventPublisher;

        #endregion

        #region Ctor

        public MongoBaseService()
        {
            Mapper = EngineContext.Current.Resolve<IMapper>()
                     ?? throw new ArgumentNullException($"{nameof(IMapper)} is null");
            WriteRepository = EngineContext.Current.Resolve<IMongoWriteRepository<TTable>>()
                         ?? throw new ArgumentNullException($"{nameof(IMongoWriteRepository<TTable>)} is null");
            ReadRepository = EngineContext.Current.Resolve<IMongoReadRepository<TTable>>()
                         ?? throw new ArgumentNullException($"{nameof(IMongoReadRepository<TTable>)} is null");
            EventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
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

            var query = ReadRepository.Collection;

            var result = await PagedList<TTable>.Create(query, filter, builderSort, pageIndex, pageSize);

            return Mapper.Map<PagedList<TResultDto>>(result);
        }

        public virtual async Task<TResultDto> GetByIdAsync(object id)
        {
            var result = await ReadRepository.FindByIdAsync(id);
            return Mapper.Map<TResultDto>(result);
        }

        public virtual async Task<int> CountAsync()
        {
            return await ReadRepository.Table.CountAsync();
        }

        public virtual async Task<TResultDto> AddAsync(TAddDto companyAddDto)
        {
            if (companyAddDto == null)
                throw new ArgumentNullException($"{nameof(companyAddDto)}");

            var domainEntity = Mapper.Map<TTable>(companyAddDto);

            domainEntity.CreatedDate = DateTime.UtcNow;
            domainEntity.CreatorIP = RemoteIp;

            var result = await WriteRepository.AddAsync(domainEntity);

            // event notification
            // await _mediator.EntityInserted(vendor);
            EventPublisher.EntityInserted(domainEntity);

            return Mapper.Map<TResultDto>(domainEntity);
        }

        public virtual async Task<TResultDto> UpdateAsync(TEditDto companyEditDto)
        {
            if (companyEditDto == null)
                throw new ArgumentNullException($"{nameof(companyEditDto)}");

            var domainEntity = Mapper.Map<TTable>(companyEditDto);

            domainEntity.ModifiedDate = DateTime.UtcNow;
            domainEntity.ModifierIP = RemoteIp;

            var result = await WriteRepository.UpdateAsync(domainEntity);

            // event notification
            // await _mediator.EntityUpdated(vendor);
            EventPublisher.EntityUpdated(domainEntity);

            return Mapper.Map<TResultDto>(result);
        }

        public virtual async Task<TResultDto> DeleteAsync(object id)
        {
            // var filter = Builders<Company>.Filter.Eq("Id", id);
            var result = await WriteRepository.DeleteAsync(id);
            
            // event notification
            // await _mediator.EntityDeleted(vendorNote);
            EventPublisher.EntityDeleted(result);

            return Mapper.Map<TResultDto>(result);
        }

        #endregion

        #region Prop

        protected virtual string RemoteIp => EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext.Connection.RemoteIpAddress.ToString();

        #endregion
    }
}