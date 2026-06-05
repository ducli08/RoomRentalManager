using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.ModelEntities.RoomRentals
{
    [Table("roomrental")]
    public class RoomRental
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("roomNumber")]
        [Display(Name = "Số phòng", Order = 1)]
        public int RoomNumber { get; set; }

        [Column("roomType")]
        [Display(Name = "Loại phòng", Order = 2)]
        public RoomType RoomType { get; set; }

        [Column("price")]
        [Display(Name = "Giá", Order = 3)]
        public double Price { get; set; }

        [Column("statusRoom")]
        [Display(Name = "Trạng thái phòng", Order = 4)]
        public RoomStatus StatusRoom { get; set; }

        [Column("note")]
        [Display(Name = "Ghi chú", Order = 5)]
        public string Note { get; set; }

        [Column("area")]
        [Display(Name = "Diện tích", Order = 6)]
        public double Area { get; set; }

        [Column("images")]
        [Display(Name = "Hình ảnh", Order = 7)]
        public List<string>? ImagesDescription { get; set; }

        [Column("createdDate")]
        [Display(Name = "Ngày tạo", Order = 8)]
        public DateTime CreatedDate { get; set; }

        [Column("updatedDate")]
        [Display(Name = "Ngày cập nhật", Order = 9)]
        public DateTime UpdatedDate { get; set; }

        [Column("creatorUser")]
        [Display(Name = "Người tạo", Order = 10)]
        public string CreatorUser { get; set; }

        [Column("lsatUpdateUser")]
        [Display(Name = "Người cập nhật cuối", Order = 11)]
        public string LastUpdateUser { get; set; }

    }
    public enum RoomType
    {
        [Display(Name = "Phòng đơn")]
        SingleRoom = 1,
        [Display(Name = "Phòng đôi")]
        DoubleRoom = 2,
        [Display(Name = "Phòng ba")]
        TripleRoom = 3,
        [Display(Name = "Phòng bốn")]
        QuadRoom = 4
    }
    public enum RoomStatus
    {
        [Display(Name = "Trống")]
        Empty = 1,
        [Display(Name = "Đã thuê")]
        Rented = 2,
        [Display(Name = "Đang sửa chữa")]
        UnderRepair = 3,
        [Display(Name = "Đã đặt cọc")]
        Deposited = 4
    }
}
