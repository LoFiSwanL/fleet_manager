using System;
using System.Collections.Generic;

namespace FleetManager.Domain.Models;

public partial class Robot : BaseEntity
{
    public string Name { get; set; } = null!;

    public string SerialNumber { get; set; } = null!;

    public int? StatusId { get; set; }

    public string? IpAddress { get; set; }

    public int? PolicyId { get; set; }

    public int? FirmwareId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Firmware? Firmware { get; set; }

    public virtual ICollection<HardwareLog> HardwareLogs { get; set; } = new List<HardwareLog>();

    public virtual Policy? Policy { get; set; }

    public virtual RobotStatus? Status { get; set; }
}
