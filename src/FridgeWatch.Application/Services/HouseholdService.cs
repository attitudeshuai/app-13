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

    public HouseholdService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return _mapper.Map<HouseholdDto>(household);
    }

    public async Task<HouseholdDto> UpdateAsync(int id, HouseholdUpdateDto dto, int userId)
    {
        var household = await _unitOfWork.Households.GetByIdAsync(id);
        if (household == null)
        {
            throw new BusinessException("家庭不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdOwnerAsync(id, userId))
        {
            throw new UnauthorizedAccessException("只有家庭所有者可以修改家庭信息");
        }

        _mapper.Map(dto, household);
        await _unitOfWork.Households.UpdateAsync(household);
        await _unitOfWork.SaveChangesAsync();

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

        await _unitOfWork.Households.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    private string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
