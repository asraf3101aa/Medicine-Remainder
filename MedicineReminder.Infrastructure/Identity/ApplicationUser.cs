using Microsoft.AspNetCore.Identity;

namespace MedicineReminder.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FcmToken { get; set; }
}
