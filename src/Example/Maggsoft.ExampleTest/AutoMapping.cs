using AutoMapper;
using Maggsoft.Core.Model.Pagination;
using Maggsoft.ExampleTest.Dto;
using Maggsoft.ExampleTest.Entity;

namespace Maggsoft.ExampleTest
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<User, UserAddDto>().ReverseMap();
            CreateMap<User, UserResultDto>().ReverseMap();
            CreateMap<IPagedList<User>, IPagedList<UserResultDto>>().ReverseMap();
            CreateMap<UserLog, UserLogResultDto>().ReverseMap();
        }
    }
}
