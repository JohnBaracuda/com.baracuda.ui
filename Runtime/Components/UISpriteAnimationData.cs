using UnityEngine;

namespace Baracuda.UI.Components
{
    public class UISpriteAnimationData : ScriptableObject
    {
        [SerializeField] private Sprite[] sprites;
        public Sprite[] Sprites => sprites;
    }
}