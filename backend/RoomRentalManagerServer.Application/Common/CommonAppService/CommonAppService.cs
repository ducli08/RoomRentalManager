using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace RoomRentalManagerServer.Application.Common.CommonAppService
{
    public class CommonAppService : ICommonAppService
    {
        private readonly ILogger<CommonAppService> _logger;
        private readonly IWardAppService _wardAppService;
        private readonly IProvinceAppService _provinceAppService;
        private readonly IDistrictAppService _districtAppService;
        private readonly IUserAppService _userAppService;
        private readonly IRoomRentalAppService _roomRentalAppService;
        private readonly IRoleGroupAppService _roleGroupAppService;
        public CommonAppService(ILogger<CommonAppService> logger, IWardAppService wardAppService, IProvinceAppService provinceAppService,
            IDistrictAppService districtAppService, IUserAppService userAppService, IRoomRentalAppService roomRentalAppService, IRoleGroupAppService roleGroupAppService)
        {
            _logger = logger;
            _wardAppService = wardAppService;
            _provinceAppService = provinceAppService;
            _districtAppService = districtAppService;
            _userAppService = userAppService;
            _roomRentalAppService = roomRentalAppService;
            _roleGroupAppService = roleGroupAppService;
        }

        public async Task<List<SelectListItemDto>> GetCustomSelectListItem(string typeSelect, string cascadeValue)
        {
            var selectListItemDtos = new List<SelectListItemDto>();
            switch (typeSelect)
            {
                case "provinces":
                    var provinces = await _provinceAppService.GetAllProvincesAsync();
                    provinces.ForEach(p => selectListItemDtos.Add(new SelectListItemDto
                    {
                        Value = p.Code,
                        Text = p.Name
                    }));
                    break;
                case "districts":
                    var districts = await _districtAppService.GetAllDistrictsAsync(cascadeValue);
                    districts.ForEach(d => selectListItemDtos.Add(new SelectListItemDto
                    {
                        Value = d.Code,
                        Text = d.Name,
                        CascaderId = d.ProvinceCode
                    }));
                    break;
                case "wards":
                    var wards = await _wardAppService.GetAllWardsAsync(cascadeValue);
                    wards.ForEach(w => selectListItemDtos.Add(new SelectListItemDto
                    {
                        Value = (w.Code != null ? w.Code : ""),
                        Text = (w.Name != null ? w.Name : ""),
                        CascaderId = w.DistrictCode
                    }));
                    break;
                case "user":
                    var users = await _userAppService.GetAllUserForSelectListItem();
                    users.ForEach(u => selectListItemDtos.Add(new SelectListItemDto
                    {
                        Value = u.Name.ToString(),
                        Text = u.Name
                    }));
                    break;
                case "roomRental":
                    var roomRentals = await _roomRentalAppService.GetAllRoomRentalForSelectListItem();
                    roomRentals.ForEach(r => selectListItemDtos.Add(new SelectListItemDto
                    {
                        Value = r.Id.ToString(),
                        Text = r.RoomNumber.ToString()
                    }));
                    break;
                case "roleGroups":
                    var roleGroups = await _roleGroupAppService.GetAllRoleGroupAsync();
                    roleGroups.ForEach(r => selectListItemDtos.Add(new SelectListItemDto
                    {
                        Value = r.Id.ToString(),
                        Text = r.Name.ToString()
                    }));
                    break;
                default:
                    throw new ArgumentException("Invalid typeSelect provided");
            }
            return selectListItemDtos;
        }

        public List<SelectListItemDto> GetEnumSelectListItem(string enumTypeName)
        {
            if (string.IsNullOrWhiteSpace(enumTypeName))
                throw new ArgumentException("Enum type name is required", nameof(enumTypeName));

            // 1. Tìm enum type
            var enumType = FindEnumType(enumTypeName);
            if (enumType == null || !enumType.IsEnum)
                throw new ArgumentException($"Type '{enumTypeName}' is not a valid enum type.");

            // 2. Gọi generic method
            var method = typeof(CommonAppService)
                .GetMethod(nameof(GetCustomSelectListFromEnum), BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
                throw new InvalidOperationException("Could not find method GetCustomSelectListFromEnum.");

            var genericMethod = method.MakeGenericMethod(enumType);

            var result = genericMethod.Invoke(this, null);
            if (result is List<SelectListItemDto> list)
                return list;
            else
                return new List<SelectListItemDto>();
        }

        private Type FindEnumType(string typeName)
        {
            var domainAssembly = typeof(RoomRentalManagerServer.Domain.ModelEntities.User.Users).Assembly;
            var enumType = domainAssembly.GetTypes()
                .FirstOrDefault(t => t.IsEnum && t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
            return enumType;
        }
        private List<SelectListItemDto> GetCustomSelectListFromEnum<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e => new SelectListItemDto
                {
                    Value = Convert.ToInt32(e).ToString(),
                    Text = GetDisplayName(e)
                })
                .ToList();
        }

        private string GetDisplayName<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            var memberInfo = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault();
            if (memberInfo != null)
            {
                var displayAttr = memberInfo.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null)
                {
                    return displayAttr.Name ?? enumValue.ToString();
                }
            }
            return enumValue.ToString();
        }

    }
}
