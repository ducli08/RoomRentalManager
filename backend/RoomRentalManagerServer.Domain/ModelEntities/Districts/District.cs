using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.ModelEntities.Districts
{
    [Table("districts")]
    public class District
    {
        [Key][Column("id")] public int Id { get; set; }

        [Column("code")][StringLength(255)] public string Code { get; set; }

        [Column("name")][StringLength(255)] public string Name { get; set; }

        [Column("provinceCode")]
        [StringLength(255)]
        public string ProvinceCode { get; set; }

        [Column("type")] public DistrictType Type { get; set; }

        [Column("boundary")] public string? Boundary { get; set; }

        [Column("no_space_name")] public string? NoSpaceName { get; set; }

        [Column("center_location")] public string? CenterLocationStr { get; set; }

        [Column("processed")] public bool? Processed { get; set; }

        [Column("boundary_geom")] public string? BoundaryGeom { get; set; }

        [Column("province_code")] public string? ProvinceCodeStr { get; set; }

        [Column("provinceId")] public int? ProvinceId { get; set; }

        [NotMapped] public List<double>? CenterLocation { get; set; }
    }

    public enum DistrictType
    {
        [Display(Name = "Huyện", Order = 1)] District_0 = 1,
        [Display(Name = "Quận", Order = 1)] District_1 = 0,
        [Display(Name = "Thị xã", Order = 1)] Town = 2,

        [Display(Name = "Thành phố", Order = 1)]
        City = 3
    }
}
