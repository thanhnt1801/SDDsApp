using AutoMapper;
using DiseaseService.DTOs;
using DiseaseService.Models;

namespace DiseaseService.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Disease, DiseaseDTO>().ReverseMap();
            CreateMap<Pesticide, PesticideDTO>().ReverseMap();
            CreateMap<PreventativeMeasure, PreventativeMeasureDTO>().ReverseMap();
            CreateMap<Symptom, SymptomDTO>().ReverseMap();
        }
    }
}
