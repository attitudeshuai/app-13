using FluentValidation;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.Validators;

public class ShoppingListCreateDtoValidator : AbstractValidator<ShoppingListCreateDto>
{
    public ShoppingListCreateDtoValidator()
    {
        RuleFor(x => x.HouseholdId)
            .GreaterThan(0).WithMessage("家庭ID必须大于0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("清单名称不能为空")
            .Length(1, 100).WithMessage("清单名称长度必须在1-100个字符之间");

        RuleForEach(x => x.Items).SetValidator(new ShoppingListItemCreateDtoValidator());
    }
}

public class ShoppingListUpdateDtoValidator : AbstractValidator<ShoppingListUpdateDto>
{
    public ShoppingListUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .Length(1, 100).WithMessage("清单名称长度必须在1-100个字符之间")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleForEach(x => x.Items).SetValidator(new ShoppingListItemCreateDtoValidator())
            .When(x => x.Items != null);
    }
}

public class ShoppingListItemCreateDtoValidator : AbstractValidator<ShoppingListItemCreateDto>
{
    public ShoppingListItemCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("食材名称不能为空")
            .Length(1, 100).WithMessage("食材名称长度必须在1-100个字符之间");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("分类不能为空")
            .Length(1, 50).WithMessage("分类长度必须在1-50个字符之间");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("数量必须大于0");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("单位不能为空")
            .Length(1, 20).WithMessage("单位长度必须在1-20个字符之间");

        RuleFor(x => x.StorageLocation)
            .IsInEnum().WithMessage("存储位置无效")
            .When(x => x.StorageLocation.HasValue);

        RuleFor(x => x.ExpiryDays)
            .GreaterThan(0).WithMessage("保质期必须大于0")
            .When(x => x.ExpiryDays.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("备注长度不能超过500个字符")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class ShoppingListItemUpdateDtoValidator : AbstractValidator<ShoppingListItemUpdateDto>
{
    public ShoppingListItemUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .Length(1, 100).WithMessage("食材名称长度必须在1-100个字符之间")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Category)
            .Length(1, 50).WithMessage("分类长度必须在1-50个字符之间")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("数量必须大于0")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.Unit)
            .Length(1, 20).WithMessage("单位长度必须在1-20个字符之间")
            .When(x => !string.IsNullOrEmpty(x.Unit));

        RuleFor(x => x.StorageLocation)
            .IsInEnum().WithMessage("存储位置无效")
            .When(x => x.StorageLocation.HasValue);

        RuleFor(x => x.ExpiryDays)
            .GreaterThan(0).WithMessage("保质期必须大于0")
            .When(x => x.ExpiryDays.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("备注长度不能超过500个字符")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class ShoppingListConvertDtoValidator : AbstractValidator<ShoppingListConvertDto>
{
    public ShoppingListConvertDtoValidator()
    {
        RuleFor(x => x.ShoppingListId)
            .GreaterThan(0).WithMessage("采购清单ID必须大于0");

        RuleForEach(x => x.ItemOverrides).SetValidator(new ShoppingListItemConvertDtoValidator())
            .When(x => x.ItemOverrides != null);
    }
}

public class ShoppingListItemConvertDtoValidator : AbstractValidator<ShoppingListItemConvertDto>
{
    public ShoppingListItemConvertDtoValidator()
    {
        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("清单项ID必须大于0");

        RuleFor(x => x.StorageLocation)
            .IsInEnum().WithMessage("存储位置无效");

        RuleFor(x => x.ExpiryDays)
            .GreaterThan(0).WithMessage("保质期必须大于0");
    }
}
