using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Core.DTOs
{
    public class PasswordResetEmailModel
    {
        public string ResetLink { get; set; }
        public string UserName { get; set; } // Si tu veux personnaliser le mail avec le nom/prénom
    }
}
