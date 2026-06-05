using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.ModelEntities.Equipments
{
    [Table("equipment")]
    public class Equipment
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("status")]
        public Status Status { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("lastUpdateUser")]
        public DateTime LastUpdateUser { get; set; }

        [Column("creatorUser")]
        public string CreatorUser { get; set; }

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
    public enum Status
    {
        [Display(Name = "Tốt")]
        Good = 1,
        [Display(Name = "Hỏng")]
        Broken = 2,
        [Display(Name = "Mất")]
        Lost = 3,
        [Display(Name = "Khác")]
        Other = 4
    }
}
