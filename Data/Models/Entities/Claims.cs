namespace ItSupportServer.Data.Models.Entities
{
    public class Claims
    {
        public int ClaimId { get; set; }

        required public string Claim { get; set; }

        public string? Category { get; set; }

        public ICollection<RoleClaims> RoleClaims { get; set; } = new List<RoleClaims>();
        public ICollection<AccountClaims> AccountClaims { get; set; } = new List<AccountClaims>();
    }
}
