using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.ContractsModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces.ContractInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.InvoiceInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.RoomRentalInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.UserInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;

namespace RoomRentalManagerServer.Application.Services
{
    public class ContractAppService : IContractAppService
    {
        private readonly ILogger<ContractAppService> _logger;
        private readonly IMapper _mapper;
        private readonly IContractRepository _contractRepository;
        private readonly IRoomRentalRepository _roomRentalRepository;
        private readonly IUserRepository _userRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICurrentUserAppService _currentUserAppService;

        public ContractAppService(
            ILogger<ContractAppService> logger,
            IMapper mapper,
            IContractRepository contractRepository,
            IRoomRentalRepository roomRentalRepository,
            IUserRepository userRepository,
            IInvoiceRepository invoiceRepository,
            ICurrentUserAppService currentUserAppService)
        {
            _logger = logger;
            _mapper = mapper;
            _contractRepository = contractRepository;
            _roomRentalRepository = roomRentalRepository;
            _userRepository = userRepository;
            _invoiceRepository = invoiceRepository;
            _currentUserAppService = currentUserAppService;
        }

        public async Task<PagedResultDto<ContractDto>> GetAllContractAsync(PagedRequestDto<ContractFilterDto> pagedRequestDto)
        {
            try
            {
                var query = _contractRepository.Query().AsNoTracking();
                var filter = pagedRequestDto.Filter;

                if (filter?.RoomRentalId > 0)
                {
                    query = query.Where(x => x.RoomRentalId == filter.RoomRentalId);
                }

                if (filter?.TenantId > 0)
                {
                    query = query.Where(x =>
                        x.TenantId == filter.TenantId
                        || x.TenantIds.Contains(filter.TenantId));
                }

                if (filter?.StatusContract != 0)
                {
                    query = query.Where(x => x.StatusContract == filter.StatusContract);
                }

                if (filter?.StartDateFrom.HasValue == true)
                {
                    query = query.Where(x => x.StartDate >= filter.StartDateFrom.Value);
                }

                if (filter?.StartDateTo.HasValue == true)
                {
                    query = query.Where(x => x.StartDate <= filter.StartDateTo.Value);
                }

                if (filter?.EndDateFrom.HasValue == true)
                {
                    query = query.Where(x => x.EndDate >= filter.EndDateFrom.Value);
                }

                if (filter?.EndDateTo.HasValue == true)
                {
                    query = query.Where(x => x.EndDate <= filter.EndDateTo.Value);
                }

                if (!string.IsNullOrEmpty(filter?.CreatorUser))
                {
                    query = query.Where(x => x.CreatorUser != null && x.CreatorUser.Contains(filter.CreatorUser));
                }

                if (!string.IsNullOrEmpty(pagedRequestDto.SortOrder) && !string.IsNullOrEmpty(pagedRequestDto.SortBy))
                {
                    query = pagedRequestDto.SortOrder == "desc"
                        ? query.OrderByDescending(x => EF.Property<object>(x, pagedRequestDto.SortBy))
                        : query.OrderBy(x => EF.Property<object>(x, pagedRequestDto.SortBy));
                }
                else
                {
                    query = query.OrderByDescending(x => x.UpdatedAt);
                }

                var total = await query.CountAsync();
                var contracts = await query
                    .Skip((pagedRequestDto.Page - 1) * pagedRequestDto.PageSize)
                    .Take(pagedRequestDto.PageSize)
                    .ToListAsync();

                var contractDtos = _mapper.Map<List<ContractDto>>(contracts);
                await EnrichContractDtosAsync(contractDtos);

                return new PagedResultDto<ContractDto>(contractDtos, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all contracts");
                throw;
            }
        }

        public async Task<ContractDto?> GetContractByIdAsync(long id)
        {
            try
            {
                var contract = await _contractRepository.GetByIdAsync(id);
                if (contract == null)
                {
                    return null;
                }

                var dto = _mapper.Map<ContractDto>(contract);
                await EnrichContractDtosAsync(new List<ContractDto> { dto });
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get contract by id {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreateOrEditContractAsync(CreateOrEditContractDto input)
        {
            if (input == null)
            {
                return false;
            }

            var isUpdate = input.Id.HasValue && input.Id.Value > 0;

            try
            {
                var tenantIds = NormalizeTenantIds(input.TenantIds, input.TenantId);
                await ValidateContractInputAsync(input, tenantIds, isUpdate ? input.Id : null);

                var contract = _mapper.Map<Contract>(input);
                contract.TenantIds = tenantIds;
                contract.TenantId = tenantIds[0];

                if (!decimal.TryParse(input.DepositAmout, out var deposit))
                {
                    throw new InvalidOperationException("Invalid deposit amount.");
                }

                if (!decimal.TryParse(input.MonthlyRent, out var monthlyRent))
                {
                    throw new InvalidOperationException("Invalid monthly rent.");
                }

                contract.DepositAmout = deposit;
                contract.MonthlyRent = monthlyRent;
                contract.ElectricUnitPrice = ParseDecimalOrDefault(input.ElectricUnitPrice, 4000);
                contract.WaterUnitPrice = ParseDecimalOrDefault(input.WaterUnitPrice, 30000);
                contract.GarbageFeePerMonthPerPerson = ParseDecimalOrDefault(input.GarbageFeePerMonthPerPerson, 12500);

                if (isUpdate)
                {
                    contract.Id = input.Id!.Value;
                    await UpdateContractAsync(contract);
                }
                else
                {
                    await AddContractAsync(contract);
                }

                return true;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Contract validation failed");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to {Action} contract", isUpdate ? "update" : "create");
                return false;
            }
        }

        public async Task DeleteContractAsync(long id)
        {
            try
            {
                var contract = await _contractRepository.GetByIdAsync(id);
                if (contract == null)
                {
                    throw new KeyNotFoundException($"Contract with id {id} not found.");
                }

                var hasInvoices = await _invoiceRepository.Query().AnyAsync(x => x.ContractId == id);
                if (hasInvoices)
                {
                    throw new InvalidOperationException("Cannot delete a contract that already has invoices.");
                }

                await _contractRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete contract {Id}", id);
                throw;
            }
        }

        public async Task<List<SelectListItemDto>> GetActiveContractsForSelectListItemAsync()
        {
            var contracts = await _contractRepository.Query().AsNoTracking()
                .Where(c => c.StatusContract == StatusContract.Active)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            var contractDtos = _mapper.Map<List<ContractDto>>(contracts);
            await EnrichContractDtosAsync(contractDtos);

            return contractDtos.Select(c => new SelectListItemDto
            {
                Value = c.Id.ToString(),
                Text = $"#{c.Id} - {c.RoomName ?? ""} - {c.TenantName ?? ""}"
            }).ToList();
        }

        private async Task ValidateContractInputAsync(CreateOrEditContractDto input, long[] tenantIds, long? excludeContractId)
        {
            if (input.EndDate < input.StartDate)
            {
                throw new InvalidOperationException("End date must be on or after start date.");
            }

            var roomRental = await _roomRentalRepository.GetRoomRetalById(input.RoomRentalId);
            if (roomRental == null)
            {
                throw new InvalidOperationException("Room rental does not exist.");
            }

            if (tenantIds.Length == 0)
            {
                throw new InvalidOperationException("At least one tenant must be selected.");
            }

            var userQuery = await _userRepository.GetAllQueryAsync();
            var existingTenantIds = await userQuery
                .Where(u => tenantIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            if (existingTenantIds.Count != tenantIds.Length)
            {
                throw new InvalidOperationException("One or more selected tenants do not exist.");
            }

            if (input.StatusContract == StatusContract.Active)
            {
                var hasActiveContract = await _contractRepository.Query()
                    .AnyAsync(c => c.RoomRentalId == input.RoomRentalId
                        && c.StatusContract == StatusContract.Active
                        && (!excludeContractId.HasValue || c.Id != excludeContractId.Value));

                if (hasActiveContract)
                {
                    throw new InvalidOperationException("The room already has an active contract.");
                }
            }
        }

        private async Task AddContractAsync(Contract contract)
        {
            if (!_currentUserAppService.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userName = _currentUserAppService.UserName ?? throw new InvalidOperationException("User is null.");
            contract.CreatorUser = userName;
            contract.UpdaterUser = userName;
            contract.CreatedAt = contract.UpdatedAt = DateTime.UtcNow;
            await _contractRepository.AddAsync(contract);
        }

        private async Task UpdateContractAsync(Contract contract)
        {
            contract.UpdaterUser = _currentUserAppService.UserName ?? throw new InvalidOperationException("User is null.");
            contract.UpdatedAt = DateTime.UtcNow;

            var existing = await _contractRepository.GetByIdAsync(contract.Id, asNoTracking: false);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Contract with id {contract.Id} not found.");
            }

            contract.CreatorUser = existing.CreatorUser;
            contract.CreatedAt = existing.CreatedAt;
            await _contractRepository.UpdateAsync(contract);
        }

        private async Task EnrichContractDtosAsync(List<ContractDto> contractDtos)
        {
            if (contractDtos == null || contractDtos.Count == 0)
            {
                return;
            }

            var roomIds = contractDtos.Select(x => x.RoomRentalId).Distinct().ToList();
            var tenantIds = contractDtos
                .SelectMany(x => NormalizeTenantIds(x.TenantIds, x.TenantId))
                .Distinct()
                .ToList();

            var roomQuery = await _roomRentalRepository.GetAllRoomRentalAsync();
            var rooms = await roomQuery.Where(r => roomIds.Contains(r.Id)).ToListAsync();
            var roomMap = rooms.ToDictionary(r => r.Id, r => r.RoomNumber.ToString());

            var userQuery = await _userRepository.GetAllQueryAsync();
            var users = await userQuery.Where(u => tenantIds.Contains(u.Id)).ToListAsync();
            var userMap = users.ToDictionary(u => u.Id, u => u.Name);

            foreach (var dto in contractDtos)
            {
                dto.RoomName = roomMap.TryGetValue(dto.RoomRentalId, out var roomName) ? roomName : null;

                var normalizedTenantIds = NormalizeTenantIds(dto.TenantIds, dto.TenantId);
                dto.TenantIds = normalizedTenantIds;
                dto.TenantNames = normalizedTenantIds
                    .Where(userMap.ContainsKey)
                    .Select(id => userMap[id])
                    .ToArray();
                dto.TenantId = normalizedTenantIds.FirstOrDefault();
                dto.TenantName = dto.TenantNames.Length > 0 ? string.Join(", ", dto.TenantNames) : null;
            }
        }

        private static decimal ParseDecimalOrDefault(string? value, decimal defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return decimal.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        private static long[] NormalizeTenantIds(IEnumerable<long>? tenantIds, long fallbackTenantId = 0)
        {
            var normalized = tenantIds?
                .Where(x => x > 0)
                .Distinct()
                .ToArray() ?? Array.Empty<long>();

            if (normalized.Length == 0 && fallbackTenantId > 0)
            {
                return new[] { fallbackTenantId };
            }

            return normalized;
        }
    }
}
