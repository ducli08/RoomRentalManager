using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities.User
{
    [Table("user")]
    public class Users
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("roleGroupId")]
        public int RoleGroupId { get; set; }

        [Column("name")]
        [Display(Name = "Tên người dùng", Order = 1)]
        public string Name { get; set; }

        [Column("email")]
        [Display(Name = "Email", Order = 2)]
        public string Email { get; set; }

        [Column("password")]
        [Display(Name = "Mật khẩu", Order = 3)]
        public string Password { get; set; }

        [Column("provinceId")]
        [Display(Name = "Tỉnh/TP", Order = 4)]
        public string? ProvinceCode { get; set; }

        [Column("districtId")]
        [Display(Name = "Quận/Huyện", Order = 5)]
        public string? DistrictCode { get; set; }

        [Column("wardId")]
        [Display(Name = "Xã/Phường", Order = 6)]
        public string? WardCode { get; set; }

        [Column("address")]
        [Display(Name = "Địa chỉ cụ thể", Order = 7)]
        public string? Address { get; set; }

        [Column("idCard")]
        [Display(Name = "CCCD/CMT", Order = 8)]
        public string? IDCard { get; set; }

        [Column("job")]
        [Display(Name = "Công việc", Order = 9)]
        public string? Job { get; set; }

        [Column("dateofbirth")]
        [Display(Name = "Ngày tháng năm sinh", Order = 10)]
        public DateTime? DateOfBirth { get; set; }

        [Column("gender")]
        [Display(Name = "Giới tính", Order = 11)]
        public string? Gender { get; set; }

        [Column("bikeId")]
        [Display(Name = "Phương tiện", Order = 12)]
        public string? BikeId { get; set; }

        [Column("phoneNumber")]
        [Display(Name = "Số điện thoại", Order = 13)]
        public string? PhoneNumber { get; set; }

        [Column("createdDate")]
        [Display(Name = "Ngày tạo", Order = 14)]
        public DateTime CreatedDate { get; set; }

        [Column("updatedDate")]
        [Display(Name = "Ngày cập nhật", Order = 15)]
        public DateTime UpdatedDate { get; set; }

        [Column("creatorUser")]
        [Display(Name = "Người tạo", Order = 16)]
        public string? CreatorUser { get; set; }

        [Column("lastUpdateUser")]
        [Display(Name = "Người cập nhật", Order = 17)]
        public string? LastUpdateUser { get; set; }

        [Column("avatar")]
        [Display(Name = "Ảnh đại diện", Order = 18)]
        public string? Avatar { get; set; }

        [Column("avatarPublicId")]
        public string? AvatarPublicId { get; set; }

        [Column("provider")]
        public string? Provider { get; set; }

        [Column("providerId")]
        public string? ProviderId { get; set; }
    }
}
