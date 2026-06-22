using Microsoft.Extensions.Logging.Abstractions;
using RoomRentalManagerServer.Application.Common.CommonAppService;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.ContractsModel.Dto;
using RoomRentalManagerServer.Application.Model.Login.Dto;
using RoomRentalManagerServer.Application.Model.RoleGroupsModel.Dto;
using RoomRentalManagerServer.Application.Model.RoomRentalsModel.Dto;
using RoomRentalManagerServer.Application.Model.UsersModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.Districts;
using RoomRentalManagerServer.Domain.ModelEntities.Provinces;
using RoomRentalManagerServer.Domain.ModelEntities.RoleGroups;
using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;
using RoomRentalManagerServer.Domain.ModelEntities.User;
using RoomRentalManagerServer.Domain.ModelEntities.Wards;

namespace RoomRentalManagerServer.Tests;

public class CommonAppServiceTests
{
    [Fact]
    public async Task GetCustomSelectListItem_AcceptsPluralActiveContractsAlias()
    {
        var appService = CreateService(new FakeContractAppService
        {
            ActiveContractsResult =
            [
                new SelectListItemDto { Value = "1", Text = "HD-001" }
            ]
        });

        var result = await appService.GetCustomSelectListItem("activeContracts", string.Empty);

        Assert.Single(result);
        Assert.Equal("1", result[0].Value);
    }

    [Fact]
    public async Task GetCustomSelectListItem_AcceptsSingularRoleGroupAlias()
    {
        var appService = CreateService(roleGroupAppService: new FakeRoleGroupAppService
        {
            RoleGroupsResult =
            [
                new RoleGroup { Id = 7, Name = "Admin" }
            ]
        });

        var result = await appService.GetCustomSelectListItem("roleGroup", string.Empty);

        Assert.Single(result);
        Assert.Equal("7", result[0].Value);
        Assert.Equal("Admin", result[0].Text);
    }

    [Fact]
    public async Task GetCustomSelectListItem_InvalidType_ThrowsArgumentException()
    {
        var appService = CreateService();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => appService.GetCustomSelectListItem("not-supported", string.Empty));
        Assert.Contains("Invalid typeSelect provided", ex.Message);
    }

    private static CommonAppService CreateService(
        IContractAppService? contractAppService = null,
        IRoleGroupAppService? roleGroupAppService = null)
    {
        return new CommonAppService(
            NullLogger<CommonAppService>.Instance,
            new FakeWardAppService(),
            new FakeProvinceAppService(),
            new FakeDistrictAppService(),
            new FakeUserAppService(),
            new FakeRoomRentalAppService(),
            roleGroupAppService ?? new FakeRoleGroupAppService(),
            contractAppService ?? new FakeContractAppService());
    }
}

internal sealed class FakeWardAppService : IWardAppService
{
    public Task<List<Ward>> GetAllWardsAsync(string? districtCode) => Task.FromResult(new List<Ward>());
}

internal sealed class FakeProvinceAppService : IProvinceAppService
{
    public Task<List<Province>> GetAllProvincesAsync() => Task.FromResult(new List<Province>());
}

internal sealed class FakeDistrictAppService : IDistrictAppService
{
    public Task<List<District>> GetAllDistrictsAsync(string? provinceCode) => Task.FromResult(new List<District>());
}

internal sealed class FakeUserAppService : IUserAppService
{
    public Task<(List<string> Paths, List<string> PublicIds, List<string> Errors)> UploadAvatarAsync(List<Microsoft.AspNetCore.Http.IFormFile> avatar, string webRoot) => throw new NotImplementedException();
    public Task<RoomRentalManagerServer.Application.Common.CommonDto.PagedResultDto<UserDto>> GetAllUsersAsync(RoomRentalManagerServer.Application.Common.CommonDto.PagedRequestDto<UserFilterDto> pagedRequestDto) => throw new NotImplementedException();
    public Task<UserDto> GetUserByIdAsync(long id) => throw new NotImplementedException();
    public Task<bool> CreateOrEditUserAsync(CreateOrEditUserDto input) => throw new NotImplementedException();
    public Task DeleteUserAsync(long id) => throw new NotImplementedException();
    public Task<UserDto> Authentication(string username, string password) => throw new NotImplementedException();
    public Task<List<Users>> GetAllUserForSelectListItem() => Task.FromResult(new List<Users>());
    public Task<UserDto> FindOrCreateGoogleUserAsync(GoogleTokenPayload googlePayload, string webRoot) => throw new NotImplementedException();
}

internal sealed class FakeRoomRentalAppService : IRoomRentalAppService
{
    public Task<RoomRentalManagerServer.Application.Common.CommonDto.PagedResultDto<RoomRentalDto>> GetAllRoomRentalAsync(RoomRentalManagerServer.Application.Common.CommonDto.PagedRequestDto<RoomRentalFilterDto> pagedRequestDto) => throw new NotImplementedException();
    public Task<RoomRentalDto> GetRoomRentalByIdAsync(long id) => throw new NotImplementedException();
    public Task<bool> CreateOrEditRoomRentalAsync(CreateOrEditRoomRentalDto input) => throw new NotImplementedException();
    public Task DeleteRoomRentalAsync(long id, string webRoot) => throw new NotImplementedException();
    public Task<List<RoomRental>> GetAllRoomRentalForSelectListItem() => Task.FromResult(new List<RoomRental>());
    public Task<(List<string> Paths, List<string> Errors)> UploadImageDescriptionAsync(List<Microsoft.AspNetCore.Http.IFormFile> uploadImages, string webRoot) => throw new NotImplementedException();
}

internal sealed class FakeRoleGroupAppService : IRoleGroupAppService
{
    public List<RoleGroup> RoleGroupsResult { get; set; } = [];

    public Task<RoomRentalManagerServer.Application.Common.CommonDto.PagedResultDto<RoleGroupDto>> GetAllRoleGroupsAsync(RoomRentalManagerServer.Application.Common.CommonDto.PagedRequestDto<RoleGroupFilterDto> pagedRequestRoleGroupDto) => throw new NotImplementedException();
    public Task<RoleGroupDto?> GetRoleGroupByIdAsync(long id) => throw new NotImplementedException();
    public Task<bool> CreateOrEditRoleGroupAsync(CreateOrEditRoleGroupDto createOrEditRoleGroupDto) => throw new NotImplementedException();
    public Task UpdateRoleGroupAsync(RoleGroup roleGroup) => throw new NotImplementedException();
    public Task<RoleGroupDto> AddRoleGroupAsync(RoleGroup roleGroup) => throw new NotImplementedException();
    public Task DeleteRoleGroupAsync(long id) => throw new NotImplementedException();
    public Task<List<RoleGroup>> GetAllRoleGroupAsync() => Task.FromResult(RoleGroupsResult);
}

internal sealed class FakeContractAppService : IContractAppService
{
    public List<SelectListItemDto> ActiveContractsResult { get; set; } = [];

    public Task<RoomRentalManagerServer.Application.Common.CommonDto.PagedResultDto<ContractDto>> GetAllContractAsync(RoomRentalManagerServer.Application.Common.CommonDto.PagedRequestDto<ContractFilterDto> pagedRequestDto) => throw new NotImplementedException();
    public Task<ContractDto?> GetContractByIdAsync(long id) => throw new NotImplementedException();
    public Task<bool> CreateOrEditContractAsync(CreateOrEditContractDto input) => throw new NotImplementedException();
    public Task DeleteContractAsync(long id) => throw new NotImplementedException();
    public Task<List<SelectListItemDto>> GetActiveContractsForSelectListItemAsync() => Task.FromResult(ActiveContractsResult);
}
