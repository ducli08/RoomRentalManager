using RoomRentalManagerServer.Domain.ModelEntities.Equipments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.ModelEntities.RoomEquipments
{
    [Table("roomEquipment")]
    public class RoomEquipment
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("roomId")]
        public long RoomId { get; set; }

        [Column("equipmentId")]
        public long EquipmentId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("status")]
        public Status Status { get; set; }

        [Column("note")]
        public string Note { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
