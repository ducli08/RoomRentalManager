using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities
{
    [Table("rolePermission")]
    public class RolePermission
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("roleId")]
        public long RoleId { get; set; }

        [Column("permissionId")]
        public long PermissionId { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("createdBy")]
        public string? CreatedBy { get; set; }

        [Column("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [Column("updatedBy")]
        public string? UpdatedBy { get; set; }
    }
}
