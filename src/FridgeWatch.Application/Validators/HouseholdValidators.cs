using FluentValidation;
using FridgeWatch.Application.DTOs;

namespace FridgeWatch.Application.Validators;

public class HouseholdCreateDtoValidator : AbstractValidator<HouseholdCreateDto>
{
    public HouseholdCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("家庭名称不能为空")
            .Length(2, 100).WithMessage("家庭名称长度必须在2-100个字符之间");
    }
}

public class HouseholdUpdateDtoValidator : AbstractValidator<HouseholdUpdateDto>
{
    public HouseholdUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .Length(2, 100).WithMessage("家庭名称长度必须在2-100个字符之间")
            .When(x => !string.IsNullOrEmpty(x.Name));
    }
}
