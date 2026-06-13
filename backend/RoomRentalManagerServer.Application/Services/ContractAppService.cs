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

                if (pagedRequestDto.Filter?.RoomRentalId > 0)
                {
                    query = query.Where(x => x.RoomRentalId == pagedRequestDto.Filter.RoomRentalId);
                }

                if (pagedRequestDto.Filter?.TenantId > 0)
                {
                    query = query.Where(x => x.TenantId == pagedRequestDto.Filter.TenantId);
                }

                if (pagedRequestDto.Filter?.StatusContract != 0)
                {
                    query = query.Where(x => x.StatusContract == pagedRequestDto.Filter.StatusContract);
                }

                if (pagedRequestDto.Filter?.StartDateFrom.HasValue == true)
                {
                    query = query.Where(x => x.StartDate >= pagedRequestDto.Filter.StartDateFrom.Value);
                }

                if (pagedRequestDto.Filter?.StartDateTo.HasValue == true)
                {
                    query = query.Where(x => x.StartDate <= pagedRequestDto.Filter.StartDateTo.Value);
                }

                if (pagedRequestDto.Filter?.EndDateFrom.HasValue == true)
                {
                    query = query.Where(x => x.EndDate >= pagedRequestDto.Filter.EndDateFrom.Value);
                }

                if (pagedRequestDto.Filter?.EndDateTo.HasValue == true)
                {
                    query = query.Where(x => x.EndDate <= pagedRequestDto.Filter.EndDateTo.Value);
                }

                if (!string.IsNullOrEmpty(pagedRequestDto.Filter?.CreatorUser))
                {
                    query = query.Where(x => x.CreatorUser != null && x.CreatorUser.Contains(pagedRequestDto.Filter.CreatorUser));
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
                await ValidateContractInputAsync(input, isUpdate ? input.Id : null);

                var contract = _mapper.Map<Contract>(input);

                if (!decimal.TryParse(input.DepositAmout, out var deposit))
                {
                    throw new InvalidOperationException("Tiền cọc không hợp lệ.");
                }

                if (!decimal.TryParse(input.MonthlyRent, out var monthlyRent))
                {
                    throw new InvalidOperationException("Tiền thuê hàng tháng không hợp lệ.");
                }

                contract.DepositAmout = deposit;
                contract.MonthlyRent = monthlyRent;

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
                    throw new InvalidOperationException("Không thể xóa hợp đồng đã có hóa đơn liên quan.");
                }

                await _contractRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete contract {Id}", id);
                throw;
            }
        }

        private async Task ValidateContractInputAsync(CreateOrEditContractDto input, long? excludeContractId)
        {
            if (input.EndDate < input.StartDate)
            {
                throw new InvalidOperationException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            }

            var roomRental = await _roomRentalRepository.GetRoomRetalById(input.RoomRentalId);
            if (roomRental == null)
            {
                throw new InvalidOperationException("Phòng trọ không tồn tại.");
            }

            var tenant = await _userRepository.GetByIdAsync(input.TenantId);
            if (tenant == null)
            {
                throw new InvalidOperationException("Người thuê không tồn tại.");
            }

            if (input.StatusContract == StatusContract.Active)
            {
                var hasActiveContract = await _contractRepository.Query()
                    .AnyAsync(c => c.RoomRentalId == input.RoomRentalId
                        && c.StatusContract == StatusContract.Active
                        && (!excludeContractId.HasValue || c.Id != excludeContractId.Value));

                if (hasActiveContract)
                {
                    throw new InvalidOperationException("Phòng trọ đã có hợp đồng đang kích hoạt.");
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
            var tenantIds = contractDtos.Select(x => x.TenantId).Distinct().ToList();

            var roomQuery = await _roomRentalRepository.GetAllRoomRentalAsync();
            var rooms = await roomQuery.Where(r => roomIds.Contains(r.Id)).ToListAsync();
            var roomMap = rooms.ToDictionary(r => r.Id, r => r.RoomNumber.ToString());

            var userQuery = await _userRepository.GetAllQueryAsync();
            var users = await userQuery.Where(u => tenantIds.Contains(u.Id)).ToListAsync();
            var userMap = users.ToDictionary(u => u.Id, u => u.Name);

            foreach (var dto in contractDtos)
            {
                dto.RoomName = roomMap.TryGetValue(dto.RoomRentalId, out var roomName) ? roomName : null;
                dto.TenantName = userMap.TryGetValue(dto.TenantId, out var tenantName) ? tenantName : null;
            }
        }
    }
}
