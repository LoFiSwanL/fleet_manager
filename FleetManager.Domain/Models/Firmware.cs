using System;
using System.Collections.Generic;

namespace FleetManager.Domain.Models;

public partial class Firmware : BaseEntity
{
    public string Version { get; set; } = null!;

    public DateTime? ReleaseDate { get; set; }

    public virtual ICollection<Robot> Robots { get; set; } = new List<Robot>();
}
