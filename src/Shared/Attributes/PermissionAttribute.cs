using Microsoft.AspNetCore.Authorization;

namespace ItSupportServer.src.Shared.Attributes
{
    // Attribute dùng để đặt lên Controller/Action
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission) : base(permission) { }
    }
}
