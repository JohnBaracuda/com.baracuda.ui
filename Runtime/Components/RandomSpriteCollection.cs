using Baracuda.Utility.Utilities;
using UnityEngine;

namespace Baracuda.UI.Components
{
    public class RandomSpriteCollection : ScriptableObject
    {
        [SerializeField] private Sprite[] sprites;

        private Sprite _lastElement;

        public Sprite GetRandomElement()
        {
            var sprite = RandomUtility.GetRandomItemWithExceptionOf(sprites, _lastElement);
            _lastElement = sprite;
            return sprite;
        }
    }
}