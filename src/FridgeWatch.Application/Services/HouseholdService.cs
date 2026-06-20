using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Services;

public class HouseholdService : IHouseholdService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public HouseholdService(IUnitOfWork unitOfWork, IMapper mapper, IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<HouseholdDto>> GetListAsync(QueryParametersDto parameters, int? userId = null)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);

        PagedResult<Household> result;
        if (userId.HasValue)
        {
            result = await _unitOfWork.Households.GetByUserIdAsync(userId.Value, queryParams);
        }
        else
        {
            result = await _unitOfWork.Households.GetPagedAsync(queryParams);
        }

        return _mapper.Map<PagedResultDto<HouseholdDto>>(result);
    }

    public async Task<HouseholdDto> GetByIdAsync(int id)
    {
        var household = await _unitOfWork.Households.GetByIdAsync(id);
        if (household == null)
        {
            throw new BusinessException("家庭不存在");
        }

        return _mapper.Map<HouseholdDto>(household);
    }

    public async Task<HouseholdDto> CreateAsync(HouseholdCreateDto dto, int userId)
    {
        var household = _mapper.Map<Household>(dto);
        household.CreatedBy = userId;
        household.InviteCode = GenerateInviteCode();
        household.InviteCodeExpiresAt = DateTime.UtcNow.AddDays(7);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Households.AddAsync(household);
            await _unitOfWork.SaveChangesAsync();

            var member = new HouseholdMember
            {
                HouseholdId = household.Id,
                UserId = userId,
                Role = HouseholdRole.Owner,
                JoinedAt = DateTime.UtcNow
            };
            await _unitOfWork.HouseholdMembers.AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user != null && !user.DefaultHouseholdId.HasValue)
            {
                user.DefaultHouseholdId = household.Id;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        await _auditLogService.LogAsync("Household", household.Id, "Create", userId, operatorName, household.Id, $"创建家庭「{household.Name}」");

        return _mapper.Map<HouseholdDto>(household);
    }

    public async Task<HouseholdDto> UpdateAsync(int id, HouseholdUpdateDto dto, int userId)
    {
        var household = await _unitOfWork.Households.GetByIdAsync(id);
        if (household == null)
        {
            throw new BusinessException("家庭不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdOwnerOrAdminAsync(id, userId))
        {
            throw new UnauthorizedAccessException("只有家庭所有者或管理员可以修改家庭信息");
        }

        _mapper.Map(dto, household);
        await _unitOfWork.Households.UpdateAsync(household);
        await _unitOfWork.SaveChangesAsync();

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        await _auditLogService.LogAsync("Household", household.Id, "Update", userId, operatorName, household.Id, $"修改家庭「{household.Name}」信息");

        return _mapper.Map<HouseholdDto>(household);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var household = await _unitOfWork.Households.GetByIdAsync(id);
        if (household == null)
        {
            throw new BusinessException("家庭不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdOwnerAsync(id, userId))
        {
            throw new UnauthorizedAccessException("只有家庭所有者可以删除家庭");
        }

        var members = await _unitOfWork.HouseholdMembers.FindAsync(m => m.HouseholdId == id);
        foreach (var member in members)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(member.UserId);
            if (user != null && user.DefaultHouseholdId == id)
            {
                user.DefaultHouseholdId = null;
                await _unitOfWork.Users.UpdateAsync(user);
            }
        }

        await _unitOfWork.Households.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        await _auditLogService.LogAsync("Household", id, "Delete", userId, operatorName, id, $"删除家庭「{household.Name}」");
    }

    public async Task<HouseholdDto> ResetInviteCodeAsync(int householdId, ResetInviteCodeDto dto, int userId)
    {
        var household = await _unitOfWork.Households.GetByIdAsync(householdId);
        if (household == null)
        {
            throw new BusinessException("家庭不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdOwnerOrAdminAsync(householdId, userId))
        {
            throw new UnauthorizedAccessException("只有家庭所有者或管理员可以重置邀请码");
        }

        if (dto.ValidDays.HasValue && dto.ValidDays.Value <= 0)
        {
            throw new BusinessException("有效天数必须大于0");
        }

        household.InviteCode = GenerateInviteCode();
        household.InviteCodeExpiresAt = dto.ValidDays.HasValue
            ? DateTime.UtcNow.AddDays(dto.ValidDays.Value)
            : null;

        await _unitOfWork.Households.UpdateAsync(household);
        await _unitOfWork.SaveChangesAsync();

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        await _auditLogService.LogAsync("Household", household.Id, "ResetInviteCode", userId, operatorName, household.Id, $"重置家庭「{household.Name}」邀请码");

        return _mapper.Map<HouseholdDto>(household);
    }

    private string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
