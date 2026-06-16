using FluentValidation;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.Validators;

public class FoodItemCreateDtoValidator : AbstractValidator<FoodItemCreateDto>
{
    public FoodItemCreateDtoValidator()
    {
        RuleFor(x => x.HouseholdId)
            .GreaterThan(0).WithMessage("家庭ID必须大于0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("食材名称不能为空")
            .Length(1, 100).WithMessage("食材名称长度必须在1-100个字符之间");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("分类不能为空")
            .Length(1, 50).WithMessage("分类长度必须在1-50个字符之间");

        RuleFor(x => x.StorageLocation)
            .IsInEnum().WithMessage("存储位置无效");

        RuleFor(x => x.PurchaseDate)
            .NotEmpty().WithMessage("购买日期不能为空");

        RuleFor(x => x.ExpiryDate)
            .NotEmpty().WithMessage("过期日期不能为空")
            .GreaterThan(x => x.PurchaseDate).WithMessage("过期日期必须晚于购买日期");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("数量必须大于0");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("单位不能为空")
            .Length(1, 20).WithMessage("单位长度必须在1-20个字符之间");
    }
}

public class FoodItemUpdateDtoValidator : AbstractValidator<FoodItemUpdateDto>
{
    public FoodItemUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .Length(1, 100).WithMessage("食材名称长度必须在1-100个字符之间")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Category)
            .Length(1, 50).WithMessage("分类长度必须在1-50个字符之间")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.StorageLocation)
            .IsInEnum().WithMessage("存储位置无效")
            .When(x => x.StorageLocation.HasValue);

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("数量必须大于0")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.Unit)
            .Length(1, 20).WithMessage("单位长度必须在1-20个字符之间")
            .When(x => !string.IsNullOrEmpty(x.Unit));
    }
}
