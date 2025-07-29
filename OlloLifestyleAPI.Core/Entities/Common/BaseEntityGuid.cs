using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities.Common;

public abstract class BaseEntityGuid
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
}