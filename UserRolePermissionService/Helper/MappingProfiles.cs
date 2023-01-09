using AutoMapper;
using UserRolePermissionService.DTOs;
using UserService.Models;

namespace DiseaseService.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<User, UserDTO>().ReverseMap();
        }
    }
}
