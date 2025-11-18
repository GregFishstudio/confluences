using Confluences.Domain.Entities;

namespace Api.ViewModel
{
    public class EntrepriseVm: Entreprise
    {
        public string Domaines { get; set; }
        public int TotalInterships { get; set; }
    }
}
