using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.ModelEntities.ImageDescriptions
{
    [Table("image_descriptions")]
    public class ImagesDescription
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("imageFileName")]
        public string ImageFileName { get; set; }

        [Column("imageUrl")]
        public string ImageUrl { get; set; }

        [Column("publicId")]
        public string PublicId { get; set; }

        [Column("createdDate")]
        public DateTime CreatedDate { get; set; }

        [Column("updatedDate")]
        public DateTime UpdatedDate { get; set; }

        [Column("creatorUser")]
        public string CreatorUser { get; set; }

        [Column("lastUpdateUser")]
        public string LastUpdateUser { get; set; }
    }
}
