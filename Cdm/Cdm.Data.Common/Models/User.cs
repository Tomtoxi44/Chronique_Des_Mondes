namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Users")]
public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
