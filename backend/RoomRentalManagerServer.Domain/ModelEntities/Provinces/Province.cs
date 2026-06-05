using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.ModelEntities.Provinces
{
    [Table("provinces")]
    public class Province
    {
        [Key][Column("id")] public int Id { get; set; }

        [Column("code")][StringLength(255)] public string Code { get; set; }

        [Column("name")][StringLength(255)] public string Name { get; set; }

        [Column("type")] public ProvinceType Type { get; set; }

        [Column("boundary")] public string Boundary { get; set; }

        [Column("center_location")] public string CenterLocationStr { get; set; }

        [Column("processed")] public bool? Processed { get; set; }

        [NotMapped] public List<double>? CenterLocation { get; set; }

        [Column("boundary_geom")] public string BoundaryGeom { get; set; }
    }

    public enum ProvinceType
    {
        [Display(Name = "Thành phố (Trực thuộc trung ương)", Order = 1)]
        City = 1,
        [Display(Name = "Tỉnh", Order = 1)] Province = 0
    }
}
