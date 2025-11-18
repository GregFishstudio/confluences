using Api.ViewModel;
using AutoMapper;
using Confluences.Domain.Entities;

namespace Api.Utility
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<Entreprise, EntrepriseVm>(); // means you want to map from Entrepris to Entreprise
        }
    }
}
