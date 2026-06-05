using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Model.UsersModel.Dto
{
    public class UserDto
    {
        public long Id { get; set; }
        [Display(Name = "Tên người dùng", Order = 1)]
        public string Name { get; set; }

        
        [Display(Name = "Email", Order = 2)]
        public string Email { get; set; }
        
        [Display(Name = "Tỉnh/TP", Order = 4)]
        public string ProvinceCode { get; set; }

       
        [Display(Name = "Quận/Huyện", Order = 5)]
        public string DistrictCode { get; set; }

        
        [Display(Name = "Xã/Phường", Order = 6)]
        public string WardCode { get; set; }

        
        [Display(Name = "Địa chỉ cụ thể", Order = 7)]
        public string Address { get; set; }

       
        [Display(Name = "CCCD/CMT", Order = 8)]
        public string IDCard { get; set; }

        
        [Display(Name = "Công việc", Order = 9)]
        public string Job { get; set; }

        
        [Display(Name = "Ngày tháng năm sinh", Order = 10)]
        public DateTime DateOfBirth { get; set; }

        
        [Display(Name = "Giới tính", Order = 11)]
        public string Gender { get; set; }

        
        [Display(Name = "Phương tiện", Order = 12)]
        public string BikeId { get; set; }

        [Display(Name = "Số điện thoại", Order = 13)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Quyền", Order = 14)]
        public string RoleGroupId { get; set; }

        public string Password { get; set; }

        public string Avatar { get; set; }

        public string? AvatarPublicId { get; set; }
    }
}
