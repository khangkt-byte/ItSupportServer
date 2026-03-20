using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class Employees : BaseEntity<Guid>
    {
        public Guid EmpId { get; set; }
        public override Guid Id => EmpId;

        public string? EmpCode { get; set; }

        required public string FullName { get; set; }

        //[Column("birthday")]
        //public DateTime? Birthday { get; set; }

        //[Column("gender")]
        //public string? Gender { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        required public int DptId { get; set; }

        public Departments Department { get; set; } = null!;

        required public int AreaId { get; set; }

        public Areas Area { get; set; } = null!;

        public string? Position { get; set; }

        //[Column("url_image")]
        //public string? UrlImage { get; set; }

        //[Column("hire_date")]
        //public DateTime? HireDate { get; set; }

        //[Column("status")]
        //public bool? Status { get; set; }

        public Accounts? Account { get; set; }
        public ICollection<IssueLogOperators> IssueLogOperators { get; set; } = [];
        public ICollection<IssueLogRequesters> IssueLogRequesters { get; set; } = [];
    }
}
