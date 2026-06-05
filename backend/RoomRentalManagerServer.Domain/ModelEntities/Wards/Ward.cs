using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.ModelEntities.Wards
{
    [Table("wards")]
    public class Ward
    {
        [Key][Column("id")] public int Id { get; set; }

        [Column("code")][StringLength(255)] public string? Code { get; set; }

        [Column("districtCode")]
        [StringLength(255)]
        public string? DistrictCode { get; set; }

        [Column("name")][StringLength(255)] public string? Name { get; set; }

        [Column("type")] public WardType Type { get; set; }

        [Column("boundary")] public string? Boundary { get; set; }

        [Column("no_space_name")] public string? NoSpaceName { get; set; }

        [Column("province_code")] public string? ProvinceCode { get; set; }

        [Column("center_location")] public string? CenterLocationStr { get; set; }

        [Column("processed")] public bool? Processed { get; set; }

        [Column("boundary_geom")] public string? BoundaryGeom { get; set; }

        [Column("district_code")] public string? DistrictCodeStr { get; set; }

        [Column("districtId")] public int? DistrictId { get; set; }

        [NotMapped] public List<double>? CenterLocation { get; set; }
    }

    public enum WardType
    {
        [Display(Name = "Phường", Order = 1)] Ward = 0,

        [Display(Name = "Thị trấn", Order = 1)]
        Town = 1,
        [Display(Name = "Xã", Order = 1)] Commune = 2
    }
}
