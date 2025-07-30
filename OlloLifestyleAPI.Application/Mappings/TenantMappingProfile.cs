using AutoMapper;
using OlloLifestyleAPI.Application.DTOs.Tenant;
using OlloLifestyleAPI.Core.Entities.FactoryFlowTracker;

namespace OlloLifestyleAPI.Application.Mappings;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        // Factory Flow Tracker User mappings
        CreateMap<User, FactoryFlowTrackerUserDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.Active));

        CreateMap<UpdateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (UserStatus)src.Status));
    }
}