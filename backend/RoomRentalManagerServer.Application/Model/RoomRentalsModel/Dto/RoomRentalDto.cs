using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;
using System.ComponentModel.DataAnnotations;

namespace RoomRentalManagerServer.Application.Model.RoomRentalsModel.Dto
{
    public class RoomRentalDto
    {
        public long Id { get; set; }
        [Display(Name = "Số phòng", Order = 1)]
        public int RoomNumber { get; set; }

        [Display(Name = "Loại phòng", Order = 2)]
        public RoomType RoomType { get; set; }

        [Display(Name = "Giá", Order = 3)]
        public double Price { get; set; }

        [Display(Name = "Trạng thái phòng", Order = 4)]
        public RoomStatus StatusRoom { get; set; }

        [Display(Name = "Ghi chú", Order = 5)]
        public string Note { get; set; }

        [Display(Name = "Diện tích", Order = 6)]
        public double Area { get; set; }

        [Display(Name = "Hình ảnh", Order = 7)]
        public List<string>? ImagesDescription { get; set; }

        [Display(Name = "Ngày tạo", Order = 8)]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Ngày cập nhật", Order = 9)]
        public DateTime UpdatedDate { get; set; }

        [Display(Name = "Người tạo", Order = 10)]
        public string CreatorUser { get; set; }

        [Display(Name = "Người cập nhật cuối", Order = 11)]
        public string LastUpdateUser { get; set; }
    }
}
