using Baracuda.Bedrock.Assets;
using Baracuda.Utilities.Collections;
using UnityEngine;

namespace Baracuda.UI
{
    public class UISettings : ScriptableAsset
    {
        [Header("Groups")]
        [SerializeField] private Map<UIGroupReference, UIGroupSettings> groupSettings;

        public Map<UIGroupReference, UIGroupSettings> GroupSettings => groupSettings;
    }
}