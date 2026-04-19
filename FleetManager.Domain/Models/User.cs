using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetManager.Domain.Models
{
    public partial class User : IdentityUser
    {

        [Required(ErrorMessage = "Повне ім'я є обов'язковим")]
        public string? FullName { get; set; }

        public int? RoleId { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<HardwareLog> HardwareLogs { get; set; } = new List<HardwareLog>();

        public virtual Role? Role { get; set; }
    }
}