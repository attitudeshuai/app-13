using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Services;

public class HouseholdMemberService : IHouseholdMemberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public HouseholdMemberService(IUnitOfWork unitOfWork, IMapper mapper, IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<HouseholdMemberDto>> GetListAsync(QueryParametersDto parameters, int? householdId = null)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);

        PagedResult<HouseholdMember> result;
        if (householdId.HasValue)
        {
            result = await _unitOfWork.HouseholdMembers.GetByHouseholdIdAsync(householdId.Value, queryParams);
        }
        else
        {
            result = await _unitOfWork.HouseholdMembers.GetPagedAsync(queryParams);
        }

        return _mapper.Map<PagedResultDto<HouseholdMemberDto>>(result);
    }

    public async Task<PagedResultDto<HouseholdMemberDto>> GetMineAsync(int userId, QueryParametersDto parameters)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);
        var result = await _unitOfWork.HouseholdMembers.GetByUserIdAsync(userId, queryParams);
        return _mapper.Map<PagedResultDto<HouseholdMemberDto>>(result);
    }

    public async Task<HouseholdMemberDto> GetByIdAsync(int id)
    {
        var member = await _unitOfWork.HouseholdMembers.GetByIdAsync(id);
        if (member == null)
        {
            throw new BusinessException("成员不存在");
        }

        return _mapper.Map<HouseholdMemberDto>(member);
    }

    public async Task<HouseholdMemberDto> JoinHouseholdAsync(string inviteCode, int userId)
    {
        var household = await _unitOfWork.Households.GetByInviteCodeAsync(inviteCode);
        if (household == null)
        {
            throw new BusinessException("邀请码无效");
        }

        if (household.InviteCodeExpiresAt.HasValue && household.InviteCodeExpiresAt.Value < DateTime.UtcNow)
        {
            throw new BusinessException("邀请码已过期，请联系家庭所有者重新生成");
        }

        var existingMember = await _unitOfWork.HouseholdMembers.GetByHouseholdAndUserAsync(household.Id, userId);
        if (existingMember != null)
        {
            throw new BusinessException("您已经是该家庭的成员");
        }

        var member = new HouseholdMember
        {
            HouseholdId = household.Id,
            UserId = userId,
            Role = HouseholdRole.Member,
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

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        await _auditLogService.LogAsync("HouseholdMember", member.Id, "Join", userId, operatorName, household.Id, $"加入家庭「{household.Name}」");

        return _mapper.Map<HouseholdMemberDto>(member);
    }

    public async Task<HouseholdMemberDto> UpdateAsync(int id, HouseholdMemberUpdateDto dto, int userId)
    {
        var member = await _unitOfWork.HouseholdMembers.GetByIdAsync(id);
        if (member == null)
        {
            throw new BusinessException("成员不存在");
        }

        var isOwner = await _unitOfWork.HouseholdMembers.IsHouseholdOwnerAsync(member.HouseholdId, userId);
        var isAdmin = await _unitOfWork.HouseholdMembers.IsHouseholdAdminAsync(member.HouseholdId, userId);

        if (!isOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("只有家庭所有者或管理员可以修改成员角色");
        }

        if (member.Role == HouseholdRole.Owner)
        {
            throw new BusinessException("无法修改家庭所有者的角色");
        }

        if (dto.Role == HouseholdRole.Owner && !isOwner)
        {
            throw new UnauthorizedAccessException("只有家庭所有者可以指定所有者角色");
        }

        _mapper.Map(dto, member);
        await _unitOfWork.HouseholdMembers.UpdateAsync(member);
        await _unitOfWork.SaveChangesAsync();

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        var targetUser = await _unitOfWork.Users.GetByIdAsync(member.UserId);
        await _auditLogService.LogAsync("HouseholdMember", member.Id, "Update", userId, operatorName, member.HouseholdId, $"修改成员「{targetUser?.Username}」角色为 {member.Role}");

        return _mapper.Map<HouseholdMemberDto>(member);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var member = await _unitOfWork.HouseholdMembers.GetByIdAsync(id);
        if (member == null)
        {
            throw new BusinessException("成员不存在");
        }

        if (member.UserId == userId)
        {
            throw new BusinessException("不能删除自己");
        }

        var isOwner = await _unitOfWork.HouseholdMembers.IsHouseholdOwnerAsync(member.HouseholdId, userId);
        var isAdmin = await _unitOfWork.HouseholdMembers.IsHouseholdAdminAsync(member.HouseholdId, userId);

        if (!isOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("只有家庭所有者或管理员可以移除成员");
        }

        if (member.Role == HouseholdRole.Owner)
        {
            throw new BusinessException("无法移除家庭所有者");
        }

        if (member.Role == HouseholdRole.Admin && !isOwner)
        {
            throw new UnauthorizedAccessException("只有家庭所有者可以移除管理员");
        }

        await _unitOfWork.HouseholdMembers.DeleteAsync(id);

        var user = await _unitOfWork.Users.GetByIdAsync(member.UserId);
        if (user != null && user.DefaultHouseholdId == member.HouseholdId)
        {
            user.DefaultHouseholdId = null;
            await _unitOfWork.Users.UpdateAsync(user);
        }

        await _unitOfWork.SaveChangesAsync();

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        var removedUser = await _unitOfWork.Users.GetByIdAsync(member.UserId);
        await _auditLogService.LogAsync("HouseholdMember", id, "Delete", userId, operatorName, member.HouseholdId, $"移除成员「{removedUser?.Username}」");
    }

    public async Task LeaveHouseholdAsync(int householdId, int userId)
    {
        var member = await _unitOfWork.HouseholdMembers.GetByHouseholdAndUserAsync(householdId, userId);
        if (member == null)
        {
            throw new BusinessException("您不是该家庭的成员");
        }

        if (member.Role == HouseholdRole.Owner)
        {
            throw new BusinessException("家庭所有者不能退出家庭，请先转让所有权或删除家庭");
        }

        await _unitOfWork.HouseholdMembers.DeleteAsync(member.Id);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null && user.DefaultHouseholdId == householdId)
        {
            user.DefaultHouseholdId = null;
            await _unitOfWork.Users.UpdateAsync(user);
        }

        await _unitOfWork.SaveChangesAsync();

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        var household = await _unitOfWork.Households.GetByIdAsync(householdId);
        await _auditLogService.LogAsync("HouseholdMember", member.Id, "Leave", userId, operatorName, householdId, $"退出家庭「{household?.Name}」");
    }
}
