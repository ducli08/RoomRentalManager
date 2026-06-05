using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.UserInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.User;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        public UserRepository(RoomRentalManagerServerDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task AddAsync(Users user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add user: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var existUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
                ArgumentNullException.ThrowIfNull(existUser);
                _context.Users.Remove(existUser);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete user: {ex.Message}");
                throw;
            }
        }

        public async Task<IQueryable<Users>> GetAllQueryAsync()
        {
            try
            {
                return _context.Users.AsQueryable().AsNoTracking();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all users: {ex.Message}");
                throw;
            }
        }

        public async Task<Users?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get user by id: {ex.Message}");
                throw;
            }

        }

        public async Task UpdateAsync(Users user)
        {
            try
            {
                var existUser = _context.Users.FirstOrDefault(x => x.Id == user.Id);
                ArgumentNullException.ThrowIfNull(existUser);
                existUser.RoleGroupId = user.RoleGroupId;
                existUser.Name = user.Name;
                existUser.Email = user.Email;
                existUser.Password = user.Password;
                existUser.ProvinceCode = user.ProvinceCode;
                existUser.DistrictCode = user.DistrictCode;
                existUser.WardCode = user.WardCode;
                existUser.Address = user.Address;
                existUser.IDCard = user.IDCard;
                existUser.Job = user.Job;
                existUser.DateOfBirth = user.DateOfBirth;
                existUser.Gender = user.Gender;
                existUser.BikeId = user.BikeId;
                existUser.PhoneNumber = user.PhoneNumber;
                existUser.UpdatedDate = DateTime.UtcNow;
                existUser.LastUpdateUser = user.LastUpdateUser;
                if (!string.IsNullOrEmpty(user.Provider))
                {
                    existUser.Provider = user.Provider;
                }
                if (!string.IsNullOrEmpty(user.ProviderId))
                {
                    existUser.ProviderId = user.ProviderId;
                }
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    existUser.Avatar = user.Avatar;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update user: {ex.Message}");
                throw;
            }

        }

        public async Task<Users> GetUserByEmail(string email)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    return user;
                }
                else
                {
                    return null;
                }    
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to check if user exists: {ex.Message}");
                throw;
            }
        }

        public async Task<Users?> GetUserByProviderAsync(string provider, string providerId)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderId == providerId);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get user by provider: {ex.Message}");
                throw;
            }
        }

        public async Task<Users?> GetUserByEmailOrProviderAsync(string email, string provider, string providerId)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => 
                        (x.Email == email) || 
                        (x.Provider == provider && x.ProviderId == providerId));
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get user by email or provider: {ex.Message}");
                throw;
            }
        }
    }
}
