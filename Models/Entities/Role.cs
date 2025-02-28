using System.ComponentModel.DataAnnotations;

namespace DotnetAuthentication.Models.Entities;

public class Role
{
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}