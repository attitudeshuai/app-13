using FluentValidation;
using FridgeWatch.Application.DTOs;

namespace FridgeWatch.Application.Validators;

public class ConsumptionRecordCreateDtoValidator : AbstractValidator<ConsumptionRecordCreateDto>
{
    public ConsumptionRecordCreateDtoValidator()
    {
        RuleFor(x => x.FoodItemId)
            .GreaterThan(0).WithMessage("食材ID必须大于0");

        RuleFor(x => x.ConsumedQuantity)
            .GreaterThan(0).WithMessage("消耗数量必须大于0");
    }
}

public class ConsumptionRecordUpdateDtoValidator : AbstractValidator<ConsumptionRecordUpdateDto>
{
    public ConsumptionRecordUpdateDtoValidator()
    {
        RuleFor(x => x.ConsumedQuantity)
            .GreaterThan(0).WithMessage("消耗数量必须大于0")
            .When(x => x.ConsumedQuantity.HasValue);
    }
}
