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

    public HouseholdMemberService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        return _mapper.Map<HouseholdMemberDto>(member);
    }

    public async Task<HouseholdMemberDto> UpdateAsync(int id, HouseholdMemberUpdateDto dto, int userId)
    {
        var member = await _unitOfWork.HouseholdMembers.GetByIdAsync(id);
        if (member == null)
        {
            throw new BusinessException("成员不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdOwnerAsync(member.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("只有家庭所有者可以修改成员角色");
        }

        _mapper.Map(dto, member);
        await _unitOfWork.HouseholdMembers.UpdateAsync(member);
        await _unitOfWork.SaveChangesAsync();

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

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdOwnerAsync(member.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("只有家庭所有者可以移除成员");
        }

        await _unitOfWork.HouseholdMembers.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}
