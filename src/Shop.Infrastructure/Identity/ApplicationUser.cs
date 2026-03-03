using Microsoft.AspNetCore.Identity;

namespace Shop.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<long>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
