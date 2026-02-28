using System;
using System.Collections.Generic;

namespace FleetManager.Domain.Models;

public partial class LogSeverity : BaseEntity
{
    public string Name { get; set; } = null!;

    public virtual ICollection<HardwareLog> HardwareLogs { get; set; } = new List<HardwareLog>();
}
