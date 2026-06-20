using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Services;

public class ShareService : IShareService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ShareService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ShareLinkDto> CreateShareLinkAsync(CreateShareLinkDto dto, int userId)
    {
        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(dto.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法创建分享链接");
        }

        if (dto.ValidDays <= 0)
        {
            throw new BusinessException("有效天数必须大于0");
        }

        var household = await _unitOfWork.Households.GetByIdAsync(dto.HouseholdId);
        if (household == null)
        {
            throw new BusinessException("家庭不存在");
        }

        var shareLink = new ShareLink
        {
            HouseholdId = dto.HouseholdId,
            Token = GenerateToken(),
            CreatedBy = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(dto.ValidDays),
            IsRevoked = false,
            ViewCount = 0
        };

        await _unitOfWork.ShareLinks.AddAsync(shareLink);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<ShareLinkDto>(shareLink);
        result.HouseholdName = household.Name;
        result.IsExpired = shareLink.ExpiresAt < DateTime.UtcNow;

        return result;
    }

    public async Task<PagedResultDto<ShareLinkDto>> GetShareLinksByHouseholdIdAsync(
        int householdId, QueryParametersDto parameters, int userId)
    {
        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(householdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法查看分享链接");
        }

        var queryParams = _mapper.Map<QueryParameters>(parameters);
        var result = await _unitOfWork.ShareLinks.GetByHouseholdIdAsync(householdId, queryParams);

        var household = await _unitOfWork.Households.GetByIdAsync(householdId);
        var householdName = household?.Name ?? string.Empty;

        var dtos = result.Items.Select(s =>
        {
            var dto = _mapper.Map<ShareLinkDto>(s);
            dto.HouseholdName = householdName;
            dto.IsExpired = s.ExpiresAt < DateTime.UtcNow;
            return dto;
        }).ToList();

        return new PagedResultDto<ShareLinkDto>
        {
            Items = dtos,
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<ShareLinkDto> RevokeShareLinkAsync(int shareLinkId, int userId)
    {
        var shareLink = await _unitOfWork.ShareLinks.GetByIdAsync(shareLinkId);
        if (shareLink == null)
        {
            throw new BusinessException("分享链接不存在");
        }

        var isOwnerOrAdmin = await _unitOfWork.HouseholdMembers.IsHouseholdOwnerOrAdminAsync(shareLink.HouseholdId, userId);
        if (!isOwnerOrAdmin && shareLink.CreatedBy != userId)
        {
            throw new UnauthorizedAccessException("只有家庭所有者、管理员或链接创建者可以撤销分享");
        }

        shareLink.IsRevoked = true;
        shareLink.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ShareLinks.UpdateAsync(shareLink);
        await _unitOfWork.SaveChangesAsync();

        var household = await _unitOfWork.Households.GetByIdAsync(shareLink.HouseholdId);
        var result = _mapper.Map<ShareLinkDto>(shareLink);
        result.HouseholdName = household?.Name ?? string.Empty;
        result.IsExpired = shareLink.ExpiresAt < DateTime.UtcNow;

        return result;
    }

    public async Task<SharedFoodItemsDto> GetSharedFoodItemsAsync(string token)
    {
        var shareLink = await _unitOfWork.ShareLinks.GetByTokenAsync(token);
        if (shareLink == null)
        {
            throw new BusinessException("分享链接不存在或已被撤销");
        }

        if (shareLink.ExpiresAt < DateTime.UtcNow)
        {
            throw new BusinessException("分享链接已过期");
        }

        shareLink.ViewCount++;
        shareLink.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.ShareLinks.UpdateAsync(shareLink);
        await _unitOfWork.SaveChangesAsync();

        var foodItems = await _unitOfWork.FoodItems.FindAsync(f => f.HouseholdId == shareLink.HouseholdId);
        var foodItemDtos = _mapper.Map<List<FoodItemDto>>(foodItems);

        return new SharedFoodItemsDto
        {
            HouseholdName = shareLink.Household?.Name ?? string.Empty,
            SharedAt = shareLink.CreatedAt,
            ExpiresAt = shareLink.ExpiresAt,
            FoodItemCount = foodItems.Count,
            FoodItems = foodItemDtos
        };
    }

    private string GenerateToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N").Substring(0, 16);
    }
}
