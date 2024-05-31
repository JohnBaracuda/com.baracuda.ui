using UnityEngine;

namespace Baracuda.UI
{
    public class UISpriteAnimationData : ScriptableObject
    {
        [SerializeField] private Sprite[] sprites;
        public Sprite[] Sprites => sprites;
    }
}