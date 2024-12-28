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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddObserver(this LocalizedString localizedString, LocalizedString.ChangeHandler observer, string fallback = "MISSING")
        {
            if (localizedString != null)
            {
                localizedString.StringChanged += observer;
                observer(GetLocalizedStringOrDefault(localizedString, fallback));
            }
            else
            {
                observer(fallback);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveObserver(this LocalizedString localizedString, LocalizedString.ChangeHandler observer)
        {
            if (localizedString != null)
            {
                localizedString.StringChanged -= observer;
            }
        }
    }
}