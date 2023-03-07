using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace UserRolePermissionService.DTOs
{
    public class RoleDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public virtual ICollection<UserDTO> Users { get; set; }
    }
}
