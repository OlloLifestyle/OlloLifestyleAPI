using AutoMapper;
using OlloLifestyleAPI.Application.DTOs.Master;
using OlloLifestyleAPI.Core.Entities.Master;

namespace OlloLifestyleAPI.Application.Mappings;

public class MasterMappingProfile : Profile
{
    public MasterMappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Companies, opt => opt.MapFrom(src => src.UserCompanies.Select(uc => uc.Company)))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role)));

        CreateMap<User, UserInfo>();

        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.UserCompanies, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password)); // This will be handled by password hashing service

        CreateMap<UpdateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.UserCompanies, opt => opt.Ignore());

        // Company mappings
        CreateMap<Company, CompanyInfo>();

        // Role mappings
        CreateMap<Role, RoleInfo>();

        // Permission mappings
        CreateMap<Permission, PermissionInfo>();
    }
}

public class PermissionInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}