using Microsoft.EntityFrameworkCore;
using RoomRentalManagerServer.Domain.ModelEntities;
using RoomRentalManagerServer.Domain.ModelEntities.BankAccounts;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.Districts;
using RoomRentalManagerServer.Domain.ModelEntities.Equipments;
using RoomRentalManagerServer.Domain.ModelEntities.ImageDescriptions;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;
using RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount;
using RoomRentalManagerServer.Domain.ModelEntities.PaymentSubmissions;
using RoomRentalManagerServer.Domain.ModelEntities.Provinces;
using RoomRentalManagerServer.Domain.ModelEntities.RoleGroupPermission;
using RoomRentalManagerServer.Domain.ModelEntities.RoleGroups;
using RoomRentalManagerServer.Domain.ModelEntities.Roles;
using RoomRentalManagerServer.Domain.ModelEntities.RoomEquipments;
using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;
using RoomRentalManagerServer.Domain.ModelEntities.User;
using RoomRentalManagerServer.Domain.ModelEntities.Wards;
namespace RoomRentalManagerServer.Infrastructure.Data;

public class RoomRentalManagerServerDbContext : DbContext
{
    public RoomRentalManagerServerDbContext(DbContextOptions<RoomRentalManagerServerDbContext> options) : base(options) { }

    public DbSet<Users> Users { get; set; }
    public DbSet<RoomRental> RoomRentals { get; set; }  
    public DbSet<ImagesDescription> ImagesDescriptions { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentSubmission> PaymentSubmissions { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<RoomEquipment> RoomEquipments { get; set; }
    public DbSet<Equipment> Equipments { get; set; }
    public DbSet<Province> Province { get; set; }
    public DbSet<District> District { get; set; }
    public DbSet<Ward> Ward { get; set; }
    public DbSet<RoleGroup> RoleGroup { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<RoleGroupPermission> RoleGroupPermission { get; set; }
    public DbSet<Permission> Permission { get; set; }
    public DbSet<RolePermission> RolePermission { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
