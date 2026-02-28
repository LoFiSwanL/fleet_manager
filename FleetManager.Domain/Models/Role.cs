using System;
using System.Collections.Generic;

namespace FleetManager.Domain.Models;

public partial class Role : BaseEntity
{
    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
