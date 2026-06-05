namespace RoomRentalManagerServer.Application.Model.RoleGroupsModel.Dto
{
    public class RoleGroupFilterDto
    {
        public string Name { get; set; }
        public bool? Active { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatorUser { get; set; }
        public string LastUpdateUser { get; set; }
        public string Descriptions { get; set; }
    }
}
