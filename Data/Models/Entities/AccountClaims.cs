namespace ItSupportServer.Data.Models.Entities
{
    public class AccountClaims
    {
        required public Guid AccountId { get; set; }
        public Accounts Account { get; set; }

        public int ClaimId { get; set; }
        public Claims Claim { get; set; }
    }
}
