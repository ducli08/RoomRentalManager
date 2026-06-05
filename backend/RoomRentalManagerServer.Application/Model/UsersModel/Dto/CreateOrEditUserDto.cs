using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Model.UsersModel.Dto
{
    public class CreateOrEditUserDto
    {
        public long? Id { get; set; }
        public string RoleGroupId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ProvinceCode { get; set; }
        public string DistrictCode { get; set; }
        public string WardCode { get; set; }
        public string Address { get; set; }
        public string IDCard { get; set; }
        public string Job { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string BikeId { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
        public string? AvatarPublicId { get; set; }
    }
}
