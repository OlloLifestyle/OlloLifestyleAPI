using OlloLifestyleAPI.Core.Entities.Common;

namespace OlloLifestyleAPI.Core.Entities.FactoryFlowTracker;

public class User : AuditableEntityGuid
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public UserStatus Status { get; set; }
    
    // Computed Properties
    public string FullName => $"{FirstName} {LastName}";
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}