using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Services;

public class ConsumptionRecordService : IConsumptionRecordService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ConsumptionRecordService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ConsumptionRecordDto>> GetListAsync(
        QueryParametersDto parameters,
        int? foodItemId = null,
        int? householdId = null)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);

        PagedResult<ConsumptionRecord> result;
        if (foodItemId.HasValue)
        {
            result = await _unitOfWork.ConsumptionRecords.GetByFoodItemIdAsync(foodItemId.Value, queryParams);
        }
        else if (householdId.HasValue)
        {
            result = await _unitOfWork.ConsumptionRecords.GetByHouseholdIdAsync(householdId.Value, queryParams);
        }
        else
        {
            result = await _unitOfWork.ConsumptionRecords.GetPagedAsync(queryParams);
        }

        return _mapper.Map<PagedResultDto<ConsumptionRecordDto>>(result);
    }

    public async Task<PagedResultDto<ConsumptionRecordDto>> GetMineAsync(int userId, QueryParametersDto parameters)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);
        var result = await _unitOfWork.ConsumptionRecords.GetByUserIdAsync(userId, queryParams);
        return _mapper.Map<PagedResultDto<ConsumptionRecordDto>>(result);
    }

    public async Task<ConsumptionRecordDto> GetByIdAsync(int id)
    {
        var record = await _unitOfWork.ConsumptionRecords.GetByIdAsync(id);
        if (record == null)
        {
            throw new BusinessException("消耗记录不存在");
        }

        return _mapper.Map<ConsumptionRecordDto>(record);
    }

    public async Task<ConsumptionRecordDto> CreateAsync(ConsumptionRecordCreateDto dto, int userId)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(dto.FoodItemId);
        if (foodItem == null)
        {
            throw new BusinessException("食材不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(foodItem.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员");
        }

        if (dto.ConsumedQuantity > foodItem.Quantity)
        {
            throw new BusinessException("消耗数量不能大于剩余数量");
        }

        var record = _mapper.Map<ConsumptionRecord>(dto);
        record.UserId = userId;
        record.ConsumedAt = dto.ConsumedAt ?? DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.ConsumptionRecords.AddAsync(record);

            foodItem.Quantity -= dto.ConsumedQuantity;
            if (foodItem.Quantity <= 0)
            {
                foodItem.Status = FoodStatus.Consumed;
                foodItem.Quantity = 0;
            }
            foodItem.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.FoodItems.UpdateAsync(foodItem);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return _mapper.Map<ConsumptionRecordDto>(record);
    }

    public async Task<ConsumptionRecordDto> UpdateAsync(int id, ConsumptionRecordUpdateDto dto, int userId)
    {
        var record = await _unitOfWork.ConsumptionRecords.GetByIdAsync(id);
        if (record == null)
        {
            throw new BusinessException("消耗记录不存在");
        }

        if (record.UserId != userId)
        {
            throw new UnauthorizedAccessException("只能修改自己的消耗记录");
        }

        _mapper.Map(dto, record);
        await _unitOfWork.ConsumptionRecords.UpdateAsync(record);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ConsumptionRecordDto>(record);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var record = await _unitOfWork.ConsumptionRecords.GetByIdAsync(id);
        if (record == null)
        {
            throw new BusinessException("消耗记录不存在");
        }

        if (record.UserId != userId)
        {
            throw new UnauthorizedAccessException("只能删除自己的消耗记录");
        }

        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(record.FoodItemId);
        if (foodItem == null)
        {
            throw new BusinessException("对应食材不存在");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.ConsumptionRecords.DeleteAsync(id);

            foodItem.Quantity += record.ConsumedQuantity;
            if (foodItem.Quantity > 0)
            {
                foodItem.Status = CalculateStatus(foodItem.ExpiryDate);
            }
            foodItem.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.FoodItems.UpdateAsync(foodItem);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
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
}
