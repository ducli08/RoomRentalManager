using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities.Roles
{
    [Table("role")]
    public class Role
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        [Display(Name = "Tên vai trò", Order = 1)]
        public string Name { get; set; }

        [Column("active")]
        [Display(Name = "Trạng thái", Order = 2)]
        public bool Active { get; set; }

        [Column("createdAt")]
        [Display(Name = "Ngày tạo", Order = 3)]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        [Display(Name = "Ngày cập nhật", Order = 4)]
        public DateTime UpdatedAt { get; set; }

        [Column("creatorUser")]
        [Display(Name = "Người tạo", Order = 5)]
        public string CreatorUser { get; set; }

        [Column("lastUpdateUser")]
        [Display(Name = "Người cập nhật", Order = 6)]
        public string LastUpdateUser { get; set; }
    }
}
