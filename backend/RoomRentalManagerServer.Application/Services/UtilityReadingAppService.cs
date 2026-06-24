using AutoMapper;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.UtilityReadingsModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces.ContractInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.RoomRentalInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.UserInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.UtilityReadingInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Application.Services
{
    public class UtilityReadingAppService : IUtilityReadingAppService
    {
        private readonly ILogger<UtilityReadingAppService> _logger;
        private readonly IMapper _mapper;
        private readonly IUtilityReadingRepository _utilityReadingRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IRoomRentalRepository _roomRentalRepository;
        private readonly IUserRepository _userRepository;
        private readonly IInvoiceAppService _invoiceAppService;
        private readonly ICurrentUserAppService _currentUser;
        private readonly RoomRentalManagerServerDbContext _context;

        public UtilityReadingAppService(
            ILogger<UtilityReadingAppService> logger,
            IMapper mapper,
            IUtilityReadingRepository utilityReadingRepository,
            IContractRepository contractRepository,
            IRoomRentalRepository roomRentalRepository,
            IUserRepository userRepository,
            IInvoiceAppService invoiceAppService,
            ICurrentUserAppService currentUser,
            RoomRentalManagerServerDbContext context)
        {
            _logger = logger;
            _mapper = mapper;
            _utilityReadingRepository = utilityReadingRepository;
            _contractRepository = contractRepository;
            _roomRentalRepository = roomRentalRepository;
            _userRepository = userRepository;
            _invoiceAppService = invoiceAppService;
            _currentUser = currentUser;
            _context = context;
        }

        public async Task<UtilityReadingPrepareDto> GetPrepareAsync(long contractId, int month, int year, long? utilityReadingId = null)
        {
            var result = new UtilityReadingPrepareDto
            {
                ContractId = contractId,
                Month = month,
                Year = year
            };

            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                result.CanSave = false;
                result.Message = "Hợp đồng không tồn tại.";
                return result;
            }

            if (contract.StatusContract != StatusContract.Active)
            {
                result.CanSave = false;
                result.Message = "Chỉ được nhập chỉ số cho hợp đồng đang kích hoạt.";
                return result;
            }

            await EnrichContractNamesAsync(result, contract);

            result.ContractStartDate = contract.StartDate;
            result.ContractEndDate = contract.EndDate;
            result.ElectricUnitPrice = contract.ElectricUnitPrice;

            if (!IsValidMonthYear(month, year, out var monthYearError))
            {
                result.CanSave = false;
                result.Message = monthYearError;
                return result;
            }

            if (!IsMonthWithinContract(contract, month, year))
            {
                result.CanSave = false;
                result.Message = "Tháng/năm không nằm trong thời gian hợp đồng.";
                return result;
            }

            var duplicate = await _utilityReadingRepository.GetByContractMonthYearAsync(contractId, month, year);
            if (duplicate != null && duplicate.Id != utilityReadingId)
            {
                result.CanSave = false;
                result.Message = "Đã tồn tại chỉ số cho hợp đồng trong tháng/năm này.";
                return result;
            }

            var (prevMonth, prevYear) = GetPreviousMonthYear(month, year);
            if (IsMonthWithinContract(contract, prevMonth, prevYear))
            {
                var prevReading = await _utilityReadingRepository.GetByContractMonthYearAsync(contractId, prevMonth, prevYear);
                if (prevReading == null)
                {
                    result.CanSave = false;
                    result.Message = $"Chưa nhập chỉ số tháng {prevMonth}/{prevYear}. Vui lòng nhập tháng trước.";
                    return result;
                }

                result.OldElectricIndex = prevReading.NewElectricIndex;
            }
            else
            {
                result.OldElectricIndex = 0;
            }

            result.CanSave = true;
            return result;
        }

        public async Task<UtilityReadingDto?> GetByIdAsync(long id)
        {
            var entity = await _utilityReadingRepository.GetByIdAsync(id);
            if (entity == null) return null;

            var dto = _mapper.Map<UtilityReadingDto>(entity);
            await EnrichDtosAsync(new List<UtilityReadingDto> { dto });
            dto.IsLockedByPayment = await _invoiceAppService.IsUtilityReadingInvoiceLockedByPaymentAsync(id);
            return dto;
        }

        public async Task<PagedResultDto<UtilityReadingDto>> GetAllAsync(PagedRequestDto<UtilityReadingFilterDto> request)
        {
            var query = BuildFilteredQuery(request.Filter);

            if (!string.IsNullOrEmpty(request.SortOrder) && !string.IsNullOrEmpty(request.SortBy))
            {
                query = request.SortOrder == "desc"
                    ? query.OrderByDescending(x => EF.Property<object>(x, request.SortBy))
                    : query.OrderBy(x => EF.Property<object>(x, request.SortBy));
            }
            else
            {
                query = query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);
            }

            var total = await query.CountAsync();
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var dtos = _mapper.Map<List<UtilityReadingDto>>(items);
            await EnrichDtosAsync(dtos);

            foreach (var dto in dtos)
            {
                dto.IsLockedByPayment = await _invoiceAppService.IsUtilityReadingInvoiceLockedByPaymentAsync(dto.Id);
            }

            return new PagedResultDto<UtilityReadingDto>(dtos, total);
        }

        public async Task<UtilityReadingDto> CreateOrEditAsync(CreateOrEditUtilityReadingDto input)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var isUpdate = input.Id.HasValue && input.Id.Value > 0;
            var contract = await _contractRepository.GetByIdAsync(input.ContractId)
                ?? throw new InvalidOperationException("Hợp đồng không tồn tại.");

            if (contract.StatusContract != StatusContract.Active)
                throw new InvalidOperationException("Chỉ được nhập chỉ số cho hợp đồng đang kích hoạt.");

            if (!IsValidMonthYear(input.Month, input.Year, out var monthYearError))
                throw new InvalidOperationException(monthYearError);

            if (!IsMonthWithinContract(contract, input.Month, input.Year))
                throw new InvalidOperationException("Tháng/năm không nằm trong thời gian hợp đồng.");

            var prepare = await GetPrepareAsync(input.ContractId, input.Month, input.Year, input.Id);
            if (!prepare.CanSave)
                throw new InvalidOperationException(prepare.Message ?? "Không thể lưu chỉ số.");

            if (input.NewElectricIndex < prepare.OldElectricIndex)
                throw new InvalidOperationException("Chỉ số điện mới phải lớn hơn hoặc bằng chỉ số cũ.");

            UtilityReading? existingT = null;
            UtilityReading? nextReading = null;

            if (isUpdate)
            {
                existingT = await _utilityReadingRepository.GetByIdAsync(input.Id!.Value, asNoTracking: false)
                    ?? throw new KeyNotFoundException("Chỉ số không tồn tại.");

                await _invoiceAppService.EnsureUtilityReadingInvoiceEditableAsync(existingT.Id);

                var (nextMonth, nextYear) = GetNextMonthYear(existingT.Month, existingT.Year);
                nextReading = await _utilityReadingRepository.GetByContractMonthYearAsync(existingT.ContractId, nextMonth, nextYear, asNoTracking: false);
                if (nextReading != null)
                {
                    await _invoiceAppService.EnsureUtilityReadingInvoiceEditableAsync(nextReading.Id);
                }
            }

            var userName = _currentUser.UserName ?? "system";
            var now = DateTime.UtcNow;
            var readingsToInvoice = new List<UtilityReading>();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                UtilityReading current;
                if (isUpdate)
                {
                    current = existingT!;
                    current.NewElectricIndex = input.NewElectricIndex;
                    current.ElectricUsage = input.NewElectricIndex - current.OldElectricIndex;
                    current.ElectricUnitPrice = contract.ElectricUnitPrice;
                    current.Status = UtilityReadingStatus.Confirmed;
                    current.UpdatedAt = now;
                    current.UpdaterUser = userName;
                    await _utilityReadingRepository.UpdateAsync(current);
                }
                else
                {
                    current = new UtilityReading
                    {
                        ContractId = input.ContractId,
                        Month = input.Month,
                        Year = input.Year,
                        OldElectricIndex = prepare.OldElectricIndex,
                        NewElectricIndex = input.NewElectricIndex,
                        ElectricUsage = input.NewElectricIndex - prepare.OldElectricIndex,
                        ElectricUnitPrice = contract.ElectricUnitPrice,
                        Status = UtilityReadingStatus.Confirmed,
                        CreatedAt = now,
                        UpdatedAt = now,
                        CreatorUser = userName,
                        UpdaterUser = userName
                    };
                    await _utilityReadingRepository.AddAsync(current);
                }

                readingsToInvoice.Add(current);

                if (nextReading != null)
                {
                    nextReading.OldElectricIndex = current.NewElectricIndex;
                    nextReading.ElectricUsage = nextReading.NewElectricIndex - nextReading.OldElectricIndex;
                    nextReading.UpdatedAt = now;
                    nextReading.UpdaterUser = userName;
                    await _utilityReadingRepository.UpdateAsync(nextReading);
                    readingsToInvoice.Add(nextReading);
                }

                foreach (var reading in readingsToInvoice)
                {
                    await _invoiceAppService.GenerateOrRegenerateFromUtilityReadingAsync(reading, contract);
                    reading.Status = UtilityReadingStatus.InvoiceGenerated;
                    reading.UpdatedAt = DateTime.UtcNow;
                    reading.UpdaterUser = userName;
                    await _utilityReadingRepository.UpdateAsync(reading);
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create or edit utility reading");
                throw;
            }

            return (await GetByIdAsync(readingsToInvoice[0].Id))!;
        }

        public async Task<byte[]> ExportExcelAsync(UtilityReadingFilterDto filter)
        {
            var query = BuildFilteredQuery(filter);
            var items = await query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToListAsync();
            var dtos = _mapper.Map<List<UtilityReadingDto>>(items);
            await EnrichDtosAsync(dtos);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("ChiSoDienNuoc");

            var headers = new[]
            {
                "Phòng", "Người thuê", "Hợp đồng", "Tháng", "Năm",
                "Điện cũ", "Điện mới", "Tiêu thụ điện", "Trạng thái"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var dto in dtos)
            {
                ws.Cell(row, 1).Value = dto.RoomName ?? "";
                ws.Cell(row, 2).Value = dto.TenantName ?? "";
                ws.Cell(row, 3).Value = dto.ContractId;
                ws.Cell(row, 4).Value = dto.Month;
                ws.Cell(row, 5).Value = dto.Year;
                ws.Cell(row, 6).Value = dto.OldElectricIndex;
                ws.Cell(row, 7).Value = dto.NewElectricIndex;
                ws.Cell(row, 8).Value = dto.ElectricUsage;
                ws.Cell(row, 9).Value = dto.Status.ToString();
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private IQueryable<UtilityReading> BuildFilteredQuery(UtilityReadingFilterDto? filter)
        {
            var query = from ur in _utilityReadingRepository.Query().AsNoTracking()
                        join c in _contractRepository.Query().AsNoTracking() on ur.ContractId equals c.Id
                        select new { Reading = ur, Contract = c };

            if (filter?.Month is > 0)
                query = query.Where(x => x.Reading.Month == filter.Month);
            if (filter?.Year is > 0)
                query = query.Where(x => x.Reading.Year == filter.Year);
            if (filter?.ContractId is > 0)
                query = query.Where(x => x.Reading.ContractId == filter.ContractId);
            if (filter?.Status is not null)
                query = query.Where(x => x.Reading.Status == filter.Status);
            if (filter?.RoomRentalId is > 0)
                query = query.Where(x => x.Contract.RoomRentalId == filter.RoomRentalId);
            if (filter?.TenantId is > 0)
                query = query.Where(x => x.Contract.TenantId == filter.TenantId || x.Contract.TenantIds.Contains(filter.TenantId.Value));
            if (!string.IsNullOrWhiteSpace(filter?.CreatorUser))
                query = query.Where(x => x.Reading.CreatorUser.Contains(filter.CreatorUser));
            if (!string.IsNullOrWhiteSpace(filter?.UpdaterUser))
                query = query.Where(x => x.Reading.UpdaterUser.Contains(filter.UpdaterUser));
            if (filter?.CreatedAt is not null)
            {
                var createdDate = filter.CreatedAt.Value.Date;
                query = query.Where(x => x.Reading.CreatedAt.Date == createdDate);
            }
            if (filter?.UpdatedAt is not null)
            {
                var updatedDate = filter.UpdatedAt.Value.Date;
                query = query.Where(x => x.Reading.UpdatedAt.Date == updatedDate);
            }

            return query.Select(x => x.Reading);
        }

        private async Task EnrichDtosAsync(List<UtilityReadingDto> dtos)
        {
            if (dtos.Count == 0) return;

            var contractIds = dtos.Select(x => x.ContractId).Distinct().ToList();
            var contracts = await _contractRepository.Query().AsNoTracking()
                .Where(c => contractIds.Contains(c.Id)).ToListAsync();

            var roomIds = contracts.Select(c => c.RoomRentalId).Distinct().ToList();
            var tenantIds = contracts
                .SelectMany(c => GetTenantIds(c))
                .Distinct()
                .ToList();

            var rooms = await _roomRentalRepository.GetAllRoomRentalAsync();
            var roomList = await rooms.Where(r => roomIds.Contains(r.Id)).ToListAsync();
            var roomMap = roomList.ToDictionary(r => r.Id, r => r.RoomNumber.ToString());

            var users = await _userRepository.GetAllQueryAsync();
            var userList = await users.Where(u => tenantIds.Contains(u.Id)).ToListAsync();
            var userMap = userList.ToDictionary(u => u.Id, u => u.Name);

            var contractMap = contracts.ToDictionary(c => c.Id);

            foreach (var dto in dtos)
            {
                if (!contractMap.TryGetValue(dto.ContractId, out var contract)) continue;
                dto.RoomName = roomMap.TryGetValue(contract.RoomRentalId, out var rn) ? rn : null;
                dto.TenantName = string.Join(", ", GetTenantIds(contract)
                    .Where(userMap.ContainsKey)
                    .Select(id => userMap[id]));
            }
        }

        private async Task EnrichContractNamesAsync(UtilityReadingPrepareDto dto, Contract contract)
        {
            var room = await _roomRentalRepository.GetRoomRetalById(contract.RoomRentalId);
            var users = await _userRepository.GetAllQueryAsync();
            var tenantNames = await users
                .Where(u => GetTenantIds(contract).Contains(u.Id))
                .Select(u => u.Name)
                .ToListAsync();
            dto.RoomName = room?.RoomNumber.ToString();
            dto.TenantName = string.Join(", ", tenantNames);
        }

        private static bool IsValidMonthYear(int month, int year, out string? error)
        {
            if (month < 1 || month > 12)
            {
                error = "Tháng phải từ 1 đến 12.";
                return false;
            }

            if (year < 0 || year > 9999)
            {
                error = "Năm không hợp lệ.";
                return false;
            }

            error = null;
            return true;
        }

        private static bool IsMonthWithinContract(Contract contract, int month, int year)
        {
            var periodStart = new DateTime(year, month, 1);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);
            return periodStart.Date >= contract.StartDate.Date && periodEnd.Date <= contract.EndDate.Date;
        }

        private static (int Month, int Year) GetPreviousMonthYear(int month, int year)
        {
            if (month == 1) return (12, year - 1);
            return (month - 1, year);
        }

        private static (int Month, int Year) GetNextMonthYear(int month, int year)
        {
            if (month == 12) return (1, year + 1);
            return (month + 1, year);
        }

        private static long[] GetTenantIds(Contract contract)
        {
            if (contract.TenantIds is { Length: > 0 })
            {
                return contract.TenantIds.Where(x => x > 0).Distinct().ToArray();
            }

            return contract.TenantId > 0 ? new[] { contract.TenantId } : Array.Empty<long>();
        }
    }
}
