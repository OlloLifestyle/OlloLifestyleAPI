using OlloLifestyleAPI.Core.Entities.Common;

namespace OlloLifestyleAPI.Core.Entities.Master;

public class UserCompany : BaseEntity
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;
}