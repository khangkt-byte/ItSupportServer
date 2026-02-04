using AutoMapper;

namespace NhaHangApi.src.Modules.User.Customer
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<RegisterCustomerDto, ResultRegisterCustomerDto>().ReverseMap();


        }
    }
}
