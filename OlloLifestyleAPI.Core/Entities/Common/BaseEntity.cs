using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities.Common;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
}