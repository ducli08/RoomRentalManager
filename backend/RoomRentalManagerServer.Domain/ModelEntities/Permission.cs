using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities
{
    [Table("permissions")]
    public class Permission
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        [Display(Name = "Tên quyền", Order = 1)]
        public string Name { get; set; }

        [Column("description")]
        [Display(Name = "Mô tả", Order = 2)]
        public string? Description { get; set; }

        [Column("createdAt")]
        [Display(Name = "Ngày tạo", Order = 3)]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        [Display(Name = "Ngày cập nhật cuối", Order = 4)]
        public DateTime? UpdatedAt { get; set; }

        [Column("creatorUser")]
        [Display(Name = "Người tạo", Order = 5)]
        public string CreatorUser { get; set; }

        [Column("lastUpdateUser")]
        [Display(Name = "Người cập nhật cuối", Order = 6)]
        public string LastUpdateUser { get; set; }
    }
}
