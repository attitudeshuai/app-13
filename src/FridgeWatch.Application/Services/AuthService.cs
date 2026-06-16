using AutoMapper;
using Microsoft.AspNetCore.Identity;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtService = jwtService;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<LoginResponseDto> RegisterAsync(UserRegisterDto dto)
    {
        if (await _unitOfWork.Users.ExistsAsync(u => u.Username == dto.Username))
        {
            throw new BusinessException("用户名已存在");
        }

        if (await _unitOfWork.Users.ExistsAsync(u => u.Email == dto.Email))
        {
            throw new BusinessException("邮箱已被注册");
        }

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return await GenerateLoginResponseAsync(user);
    }

    public async Task<LoginResponseDto> LoginAsync(UserLoginDto dto)
    {
        var user = await _unitOfWork.Users.GetByUsernameOrEmailAsync(dto.UsernameOrEmail);
        if (user == null)
        {
            throw new BusinessException("用户名或密码错误");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new BusinessException("用户名或密码错误");
        }

        return await GenerateLoginResponseAsync(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new BusinessException("用户不存在");
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateCurrentUserAsync(int userId, UserUpdateDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new BusinessException("用户不存在");
        }

        if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
        {
            if (await _unitOfWork.Users.ExistsAsync(u => u.Username == dto.Username && u.Id != userId))
            {
                throw new BusinessException("用户名已存在");
            }
        }

        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            if (await _unitOfWork.Users.ExistsAsync(u => u.Email == dto.Email && u.Id != userId))
            {
                throw new BusinessException("邮箱已被使用");
            }
        }

        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
        }

        _mapper.Map(dto, user);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    private Task<LoginResponseDto> GenerateLoginResponseAsync(User user)
    {
        var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email);
        var expiryMinutes = 1440;

        var response = new LoginResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            User = _mapper.Map<UserDto>(user)
        };

        return Task.FromResult(response);
    }
}
