using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;

namespace RoomRentalManagerServer.Application.Model.RoomRentalsModel.Dto
{
    public class CreateOrEditRoomRentalDto
    {
        public long? Id { get; set; }
        public string? RoomNumber { get; set; }
        public RoomType RoomType { get; set; }
        public string? Price { get; set; }
        public RoomStatus StatusRoom { get; set; }
        public string Note { get; set; }
        public string? Area { get; set; }
        public List<string>? ImagesDescription { get; set; }
    }
}
