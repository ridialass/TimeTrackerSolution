using Microsoft.Extensions.Localization;
using TimeTracker.Core.Resources;

namespace TimeTracker.Infrastructure.Services
{
    public class LocalizationService
    {
        private readonly IStringLocalizer<EnumLabels> _localizer;

        public LocalizationService(IStringLocalizer<EnumLabels> localizer)
        {
            _localizer = localizer;
        }

        public string GetLocalizedMessage(string key)
        {
            // Récupérer le message localisé en utilisant la clé
            return _localizer[key];
        }
    }

}