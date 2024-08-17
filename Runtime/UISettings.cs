using Baracuda.Bedrock.Collections;
using UnityEngine;

namespace Baracuda.UI
{
    public class UISettings : ScriptableObject
    {
        [Header("Groups")]
        [SerializeField] private Map<UIGroup, UIGroupSettings> groupSettings;

        public Map<UIGroup, UIGroupSettings> GroupSettings => groupSettings;
    }
}