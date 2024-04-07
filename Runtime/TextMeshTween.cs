using PrimeTween;
using TMPro;

namespace Baracuda.UI
{
    public readonly ref struct TextMeshTween
    {
        #region Text Font Size

        public static Tween FontSize(TMP_Text textField, float fontSize, float duration)
        {
            return Tween.Custom(textField, textField.fontSize, fontSize, duration,
                (text, value) => { text.fontSize = value; });
        }

        #endregion


        #region Text Character Spacing

        public static Tween CharacterSpacing(TMP_Text textField, float spacing, float duration)
        {
            return Tween.Custom(textField, textField.characterSpacing, spacing, duration,
                (text, value) => { text.characterSpacing = value; });
        }

        #endregion
    }
}