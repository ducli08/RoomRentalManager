using System.ComponentModel.DataAnnotations;

namespace RoomRentalManagerServer.Application.Model.RoleGroupsModel.Dto
{
    public class RoleGroupDto
    {
        public long Id { get; set; }
        [Display(Name = "Tên nhóm quyền", Order = 1)]
        public string Name { get; set; }
        [Display(Name = "Trạng thái", Order = 2)]
        public bool Active { get; set; }
        [Display(Name = "Ngày tạo", Order = 3)]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Ngày cập nhật", Order = 4)]
        public DateTime UpdatedAt { get; set; }
        [Display(Name = "Người tạo", Order = 5)]
        public string CreatorUser { get; set; }
        [Display(Name = "Người cập nhật", Order = 6)]
        public string LastUpdateUser { get; set; }
        [Display(Name = "Mô tả", Order = 7)]
        public string Descriptions { get; set; }
    }
}
