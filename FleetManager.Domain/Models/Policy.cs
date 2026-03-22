using System;
using System.Collections.Generic;

namespace FleetManager.Domain.Models;

public partial class Policy : BaseEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Robot> Robots { get; set; } = new List<Robot>();

    public bool IsDeleted { get; set; } = false;
}
