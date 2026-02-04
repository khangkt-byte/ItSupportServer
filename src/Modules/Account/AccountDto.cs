using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.src.Modules.Account
{
    public record CreateAccountsDto
    {
        Guid? EmpId { get; init; }
        string Username { get; init; }
        string Password { get; init; }
    }

    public record ChangePasswordDto
    {
        string CurrentPassword { get; init; }
        string NewPassword { get; init; }
    }
}
