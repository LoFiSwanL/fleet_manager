using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetManager.Domain.Models;

public partial class Robot : BaseEntity
{
    [Required(ErrorMessage = "Ім'я є обов'язковим")]
    public string Name { get; set; } = null!;

    public string SerialNumber { get; set; } = null!;

    public int? StatusId { get; set; }

    public string? IpAddress { get; set; }

    public int? PolicyId { get; set; }

    public int? FirmwareId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Firmware? Firmware { get; set; }

    public virtual ICollection<HardwareLog> HardwareLogs { get; set; } = new List<HardwareLog>();

    public virtual Policy? Policy { get; set; }

    public virtual RobotStatus? Status { get; set; }
}
