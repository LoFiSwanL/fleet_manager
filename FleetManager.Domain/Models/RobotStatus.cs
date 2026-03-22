using System;
using System.Collections.Generic;

namespace FleetManager.Domain.Models;

public partial class RobotStatus : BaseEntity
{
    public string Name { get; set; } = null!;

    public virtual ICollection<Robot> Robots { get; set; } = new List<Robot>();
    public bool IsDeleted { get; set; } = false;
}
