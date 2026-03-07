using System;
using System.Collections.Generic;

namespace FleetManager.Domain.Models;

public partial class HardwareLog : BaseEntity
{
    public int? RobotId { get; set; }

    public int? UserId { get; set; }

    public int? SeverityId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Robot? Robot { get; set; }

    public virtual LogSeverity? Severity { get; set; }

    public virtual User? User { get; set; }
}