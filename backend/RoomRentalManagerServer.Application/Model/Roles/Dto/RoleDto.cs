using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoomRentalManagerServer.Application.Model.PermissionModel.Dto;

namespace RoomRentalManagerServer.Application.Model.Roles.Dto
{
    public class RoleDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<PermissionDto> Permissions { get; set; }
    }
}
