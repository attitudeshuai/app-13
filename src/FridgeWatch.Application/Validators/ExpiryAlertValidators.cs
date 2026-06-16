using FluentValidation;
using FridgeWatch.Application.DTOs;

namespace FridgeWatch.Application.Validators;

public class ExpiryAlertCreateDtoValidator : AbstractValidator<ExpiryAlertCreateDto>
{
    public ExpiryAlertCreateDtoValidator()
    {
        RuleFor(x => x.FoodItemId)
            .GreaterThan(0).WithMessage("食材ID必须大于0");

        RuleFor(x => x.AlertDate)
            .NotEmpty().WithMessage("提醒日期不能为空");
    }
}

public class ExpiryAlertUpdateDtoValidator : AbstractValidator<ExpiryAlertUpdateDto>
{
    public ExpiryAlertUpdateDtoValidator()
    {
    }
}
