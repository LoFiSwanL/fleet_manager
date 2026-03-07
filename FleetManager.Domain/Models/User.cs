using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetManager.Domain.Models;

public partial class User : BaseEntity
{
    [Required(ErrorMessage = "Логін є обов'язковим")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Повне ім'я є обов'язковим")]
    public string? FullName { get; set; }

    [Required]
    public string PasswordHash { get; set; } = null!;

    public int? RoleId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<HardwareLog> HardwareLogs { get; set; } = new List<HardwareLog>();

    public virtual Role? Role { get; set; }
}
