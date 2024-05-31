using UnityEngine.Localization;

namespace Baracuda.UI
{
    public class DropdownEntry
    {
        public int Index;
        public int EnumValue;
        public string UniqueIdentifier;
        public string Name => LocalizedName.GetLocalizedStringOrDefault(SystemName);
        public LocalizedString LocalizedName;
        public string SystemName;
    }
}