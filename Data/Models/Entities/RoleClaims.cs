namespace ItSupportServer.Data.Models.Entities
{
    public class RoleClaims
    {
        public int RoleId { get; set; }
        public Roles Role { get; set; }

        public int ClaimId { get; set; }
        public Claims Claim { get; set; }
    }
}
