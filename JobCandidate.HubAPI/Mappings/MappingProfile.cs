using AutoMapper;
using JobCandidate.HubAPI.Entities;
using JobCandidate.HubAPI.ViewModels;

namespace JobCandidate.HubAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Candidate, CandidateModel>();
            CreateMap<CreateModel, Candidate>();
        }
    }
}
