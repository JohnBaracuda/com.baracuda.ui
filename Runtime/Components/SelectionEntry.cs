using Baracuda.UI.Localization;
using UnityEngine.Localization;

namespace Baracuda.UI.Components
{
    public class SelectionEntry
    {
        public int Index;
        public int EnumValue;
        public string UniqueIdentifier;
        public string Name => LocalizedName.GetLocalizedStringOrDefault(SystemName);
        public LocalizedString LocalizedName;
        public string SystemName;
        public object BoxedValue;
    }
}