using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoomRentalManagerServer.Application.Model.Roles.Dto;

namespace RoomRentalManagerServer.Application.Model.RoleGroupsModel.Dto
{
    public class CreateOrEditRoleGroupDto
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Descriptions { get; set; }
        public List<RoleDto> RoleDtos { get; set; }
    }
}
