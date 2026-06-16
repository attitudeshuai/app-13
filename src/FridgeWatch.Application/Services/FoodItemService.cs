using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Services;

public class FoodItemService : IFoodItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FoodItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<FoodItemDto>> GetListAsync(QueryParametersDto parameters, int? householdId = null)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);

        PagedResult<FoodItem> result;
        if (householdId.HasValue)
        {
            result = await _unitOfWork.FoodItems.GetByHouseholdIdAsync(householdId.Value, queryParams);
        }
        else
        {
            result = await _unitOfWork.FoodItems.GetPagedAsync(queryParams);
        }

        return _mapper.Map<PagedResultDto<FoodItemDto>>(result);
    }

    public async Task<FoodItemDto> GetByIdAsync(int id)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
        if (foodItem == null)
        {
            throw new BusinessException("食材不存在");
        }

        return _mapper.Map<FoodItemDto>(foodItem);
    }

    public async Task<FoodItemDto> CreateAsync(FoodItemCreateDto dto, int userId)
    {
        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(dto.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法添加食材");
        }

        var foodItem = _mapper.Map<FoodItem>(dto);
        foodItem.Status = CalculateStatus(foodItem.ExpiryDate);

        await _unitOfWork.FoodItems.AddAsync(foodItem);
        await _unitOfWork.SaveChangesAsync();

        await CheckAndSyncAlertsAsync(foodItem);

        return _mapper.Map<FoodItemDto>(foodItem);
    }

    public async Task<FoodItemDto> UpdateAsync(int id, FoodItemUpdateDto dto, int userId)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
        if (foodItem == null)
        {
            throw new BusinessException("食材不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(foodItem.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法修改食材");
        }

        _mapper.Map(dto, foodItem);

        if (dto.ExpiryDate.HasValue)
        {
            foodItem.Status = CalculateStatus(dto.ExpiryDate.Value);
        }

        await _unitOfWork.FoodItems.UpdateAsync(foodItem);
        await _unitOfWork.SaveChangesAsync();

        await CheckAndSyncAlertsAsync(foodItem);

        return _mapper.Map<FoodItemDto>(foodItem);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
        if (foodItem == null)
        {
            throw new BusinessException("食材不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(foodItem.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法删除食材");
        }

        await _unitOfWork.FoodItems.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<FoodItemDto> UpdateStatusAsync(int id, FoodStatus status, int userId)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
        if (foodItem == null)
        {
            throw new BusinessException("食材不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(foodItem.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法修改食材状态");
        }

        foodItem.Status = status;
        foodItem.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.FoodItems.UpdateAsync(foodItem);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<FoodItemDto>(foodItem);
    }

    private FoodStatus CalculateStatus(DateTime expiryDate)
    {
        var today = DateTime.UtcNow.Date;
        var daysToExpiry = (expiryDate.Date - today).Days;

        if (daysToExpiry < 0)
        {
            return FoodStatus.Expired;
        }
        else if (daysToExpiry <= 3)
        {
            return FoodStatus.NearExpiry;
        }
        else
        {
            return FoodStatus.Fresh;
        }
    }

    private async Task CheckAndSyncAlertsAsync(FoodItem foodItem)
    {
        var daysToExpiry = (foodItem.ExpiryDate.Date - DateTime.UtcNow.Date).Days;
        var needsAlert = daysToExpiry <= 3 && daysToExpiry >= 0;

        var existingAlerts = await _unitOfWork.ExpiryAlerts
            .FindAsync(a => a.FoodItemId == foodItem.Id && a.AlertType == AlertType.NearExpiry);

        var householdMembers = await _unitOfWork.HouseholdMembers
            .FindAsync(m => m.HouseholdId == foodItem.HouseholdId);

        if (needsAlert)
        {
            foreach (var member in householdMembers)
            {
                var existingAlert = existingAlerts.FirstOrDefault(a => a.UserId == member.UserId);
                if (existingAlert == null)
                {
                    var alert = new ExpiryAlert
                    {
                        FoodItemId = foodItem.Id,
                        UserId = member.UserId,
                        AlertType = AlertType.NearExpiry,
                        AlertDate = DateTime.UtcNow,
                        IsRead = false
                    };
                    await _unitOfWork.ExpiryAlerts.AddAsync(alert);
                }
            }
        }
        else
        {
            foreach (var alert in existingAlerts)
            {
                await _unitOfWork.ExpiryAlerts.DeleteAsync(alert.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
