using System;
using System.Collections.Generic;

namespace FleetManager.Domain.Models;

public partial class User : BaseEntity
{
    public string Username { get; set; } = null!;

    public string? FullName { get; set; }

    public string PasswordHash { get; set; } = null!;

    public int? RoleId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<HardwareLog> HardwareLogs { get; set; } = new List<HardwareLog>();

    public virtual Role? Role { get; set; }
}
