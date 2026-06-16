using FluentValidation;
using FridgeWatch.Application.DTOs;

namespace FridgeWatch.Application.Validators;

public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .Length(3, 50).WithMessage("用户名长度必须在3-50个字符之间");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("邮箱格式不正确");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码长度至少6个字符");
    }
}

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("用户名或邮箱不能为空");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空");
    }
}

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.Username)
            .Length(3, 50).WithMessage("用户名长度必须在3-50个字符之间")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("密码长度至少6个字符")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}
