using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Model.ImageDescriptionsModel.Dto
{
    public class CreateOrEditImageDescriptionDto
    {
        public string ImageFileName { get; set; }
        public string ImageUrl { get; set; }
        public string PublicId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatorUser { get; set; }
        public string LastUpdateUser { get; set; }
    }
}
