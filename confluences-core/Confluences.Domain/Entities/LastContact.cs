using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confluences.Domain.Entities
{
    public class LastContact
    {
        [Key]
        public int LastContactId { get; set; }
        public DateTime? DateOfContact { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        public int EntrepriseId { get; set; }
        [ForeignKey(nameof(EntrepriseId))]
        public virtual Entreprise? Entreprise { get; set; }
    }
}
