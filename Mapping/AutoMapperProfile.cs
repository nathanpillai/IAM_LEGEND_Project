using AutoMapper;
using IAMLegend.Dtos;
using IAMLegend.Entities;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IAMLegend.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserProfile, UserDto>();
            CreateMap<CreateUserRequest, UserProfile>();
        }

    }
}