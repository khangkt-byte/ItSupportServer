using ITSupportServer.src.Shared.Base;

namespace ITSupportServer.src.Modules.User.Customer
{
    public interface ICustomerService
    {
        public Task<BaseResult<ResultRegisterCustomerDto>> RegisterCustomerdto(RegisterCustomerDto dto);
        public Task<BaseResult<PaginatedResult<List<ListCustomerDto>>>> GetCustomers(string? query, int page, int pageSize, SortOBJ? sort);
    }
}
