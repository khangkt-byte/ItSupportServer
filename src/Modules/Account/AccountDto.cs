using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.src.Modules.Account
{
    public record CreateAccountsDto(
        Guid? EmpId,
        string Username,
        string Password
        );

    public record ChangePasswordDto(
        string CurrentPassword,
        string NewPassword
        );
}
