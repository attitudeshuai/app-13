using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Services;

public class ExpiryAlertService : IExpiryAlertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpiryAlertService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ExpiryAlertDto>> GetListAsync(QueryParametersDto parameters, int? foodItemId = null)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);

        PagedResult<ExpiryAlert> result;
        if (foodItemId.HasValue)
        {
            result = await _unitOfWork.ExpiryAlerts.GetByFoodItemIdAsync(foodItemId.Value, queryParams);
        }
        else
        {
            result = await _unitOfWork.ExpiryAlerts.GetPagedAsync(queryParams);
        }

        return _mapper.Map<PagedResultDto<ExpiryAlertDto>>(result);
    }

    public async Task<PagedResultDto<ExpiryAlertDto>> GetMineAsync(int userId, QueryParametersDto parameters, bool? isRead = null)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);
        var result = await _unitOfWork.ExpiryAlerts.GetByUserIdAsync(userId, queryParams, isRead);
        return _mapper.Map<PagedResultDto<ExpiryAlertDto>>(result);
    }

    public async Task<ExpiryAlertDto> GetByIdAsync(int id)
    {
        var alert = await _unitOfWork.ExpiryAlerts.GetByIdAsync(id);
        if (alert == null)
        {
            throw new BusinessException("提醒不存在");
        }

        return _mapper.Map<ExpiryAlertDto>(alert);
    }

    public async Task<ExpiryAlertDto> CreateAsync(ExpiryAlertCreateDto dto, int userId)
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

        var alert = _mapper.Map<ExpiryAlert>(dto);
        alert.UserId = userId;

        await _unitOfWork.ExpiryAlerts.AddAsync(alert);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExpiryAlertDto>(alert);
    }

    public async Task<ExpiryAlertDto> UpdateAsync(int id, ExpiryAlertUpdateDto dto, int userId)
    {
        var alert = await _unitOfWork.ExpiryAlerts.GetByIdAsync(id);
        if (alert == null)
        {
            throw new BusinessException("提醒不存在");
        }

        if (alert.UserId != userId)
        {
            throw new UnauthorizedAccessException("只能修改自己的提醒");
        }

        _mapper.Map(dto, alert);
        await _unitOfWork.ExpiryAlerts.UpdateAsync(alert);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExpiryAlertDto>(alert);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var alert = await _unitOfWork.ExpiryAlerts.GetByIdAsync(id);
        if (alert == null)
        {
            throw new BusinessException("提醒不存在");
        }

        if (alert.UserId != userId)
        {
            throw new UnauthorizedAccessException("只能删除自己的提醒");
        }

        await _unitOfWork.ExpiryAlerts.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _unitOfWork.ExpiryAlerts.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int id, int userId)
    {
        var alert = await _unitOfWork.ExpiryAlerts.GetByIdAsync(id);
        if (alert == null)
        {
            throw new BusinessException("提醒不存在");
        }

        if (alert.UserId != userId)
        {
            throw new UnauthorizedAccessException("只能标记自己的提醒为已读");
        }

        await _unitOfWork.ExpiryAlerts.MarkAsReadAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _unitOfWork.ExpiryAlerts.MarkAllAsReadAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task BatchDeleteAsync(IEnumerable<int> ids, int userId)
    {
        var idList = ids.ToList();
        if (!idList.Any())
        {
            throw new BusinessException("请选择要删除的提醒");
        }

        var alerts = await _unitOfWork.ExpiryAlerts.FindAsync(
            ea => idList.Contains(ea.Id) && ea.UserId == userId);
        var ownedIds = alerts.Select(a => a.Id).ToList();

        if (!ownedIds.Any())
        {
            throw new BusinessException("没有可删除的提醒");
        }

        await _unitOfWork.ExpiryAlerts.DeleteByIdsAsync(ownedIds);
        await _unitOfWork.SaveChangesAsync();
    }
}
