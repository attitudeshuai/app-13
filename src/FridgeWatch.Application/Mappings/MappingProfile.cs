using AutoMapper;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Common;
using FridgeWatch.Application.DTOs;

namespace FridgeWatch.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<UserRegisterDto, User>();
        CreateMap<UserUpdateDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Household, HouseholdDto>()
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.HouseholdMembers.Count));
        CreateMap<HouseholdCreateDto, Household>();
        CreateMap<HouseholdUpdateDto, Household>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<HouseholdMember, HouseholdMemberDto>();
        CreateMap<HouseholdMemberCreateDto, HouseholdMember>();
        CreateMap<HouseholdMemberUpdateDto, HouseholdMember>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<FoodItem, FoodItemDto>()
            .ForMember(dest => dest.DaysToExpiry, opt => opt.MapFrom(src => (src.ExpiryDate - DateTime.UtcNow).Days));
        CreateMap<FoodItem, FoodItemDetailDto>()
            .ForMember(dest => dest.DaysToExpiry, opt => opt.MapFrom(src => (src.ExpiryDate - DateTime.UtcNow).Days))
            .ForMember(dest => dest.OriginalPhotoUrl, opt => opt.MapFrom(src => src.PhotoUrl));
        CreateMap<FoodItemCreateDto, FoodItem>();
        CreateMap<FoodItemUpdateDto, FoodItem>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ExpiryAlert, ExpiryAlertDto>();
        CreateMap<ExpiryAlertCreateDto, ExpiryAlert>();
        CreateMap<ExpiryAlertUpdateDto, ExpiryAlert>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ConsumptionRecord, ConsumptionRecordDto>();
        CreateMap<ConsumptionRecordCreateDto, ConsumptionRecord>();
        CreateMap<ConsumptionRecordUpdateDto, ConsumptionRecord>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<QueryParametersDto, QueryParameters>();
        CreateMap<FoodItemQueryParametersDto, FoodItemQueryParameters>();
        
        CreateMap(typeof(PagedResult<>), typeof(PagedResultDto<>));

        CreateMap<ShoppingList, ShoppingListDto>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count))
            .ForMember(dest => dest.PurchasedCount, opt => opt.MapFrom(src => src.Items.Count(i => i.IsPurchased)));
        CreateMap<ShoppingListCreateDto, ShoppingList>();
        CreateMap<ShoppingListUpdateDto, ShoppingList>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ShoppingListItem, ShoppingListItemDto>();
        CreateMap<ShoppingListItemCreateDto, ShoppingListItem>();
        CreateMap<ShoppingListItemUpdateDto, ShoppingListItem>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ShareLink, ShareLinkDto>();

        CreateMap<Recipe, RecipeDto>();
        CreateMap<RecipeCreateDto, Recipe>();
        CreateMap<RecipeUpdateDto, Recipe>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<RecipeIngredient, RecipeIngredientDto>();
        CreateMap<RecipeIngredientCreateDto, RecipeIngredient>();
        CreateMap<RecipeIngredientUpdateDto, RecipeIngredient>();

        CreateMap<AuditLog, AuditLogDto>();
        CreateMap<AuditLogQueryParametersDto, QueryParameters>();
    }
}
