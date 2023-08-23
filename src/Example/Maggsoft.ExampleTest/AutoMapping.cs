using AutoMapper;
using Maggsoft.ExampleTest.Dto;
using Maggsoft.ExampleTest.Entity;

namespace Maggsoft.ExampleTest
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<User, UserResultDto>().ReverseMap();
            CreateMap<Log, LogResultDto>().ReverseMap();
        }
    }
}
