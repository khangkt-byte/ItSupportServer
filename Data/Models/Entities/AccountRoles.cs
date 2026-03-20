namespace ItSupportServer.Data.Models.Entities
{
    public class AccountRoles
    {
        required public Guid AccountId { get; set; }
        public Accounts Account { get; set; } = null!;

        public int RoleId { get; set; }
        public Roles Role { get; set; } = null!;
    }
}
