using System.Runtime.CompilerServices;
using UnityEngine.Localization;

namespace Baracuda.UI.Localization
{
    public static class LocalizationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLocalizedStringOrDefault(this LocalizedString localizedString, string fallback = "MISSING")
        {
            return localizedString is null || localizedString.IsEmpty ? fallback : localizedString.GetLocalizedString();
        }
    }
}