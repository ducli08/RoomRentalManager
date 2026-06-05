using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;

namespace RoomRentalManagerServer.Application.Model.RoomRentalsModel.Dto
{
    public class RoomRentalFilterDto
    {
        public string RoomNumber { get; set; }
        public RoomType RoomType { get; set; }
        public string PriceStart { get; set; }
        public string PriceEnd { get; set; }
        public RoomStatus StatusRoom { get; set; }
        public string Note { get; set; }
        public string AreaStart { get; set; }
        public string AreaEnd { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CreatorUser { get; set; }
        public string LastUpdateUser { get; set; }
    }
}
