using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Core.Entities
{
    public class PausePeriod
    {
        [Required(ErrorMessage = "La date de début de la pause est obligatoire.")]
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
    }

}
