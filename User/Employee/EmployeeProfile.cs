using AutoMapper;
using NhaHangApi.EF_Core.Data;
using NhaHangApi.src.Shared.Helper;

namespace NhaHangApi.src.Modules.User.Employee
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<CreateEmployeeDto, Users>()
                .ForMember(e => e.Address, otp => otp.MapFrom(src => HandleJson.StringToObj(src.Address)))
                .ReverseMap();

            CreateMap<UpdateEmployeeDto, Users>()
                .ForMember(e => e.Address, otp => otp.MapFrom(src => HandleJson.StringToObj(src.Address)))
                .ReverseMap();

            CreateMap<updateProfileDto, Users>()
                .ForMember(e => e.Address, otp => otp.MapFrom(src => HandleJson.StringToObj(src.Address)))
                .ReverseMap();

        }
    }
}
