using Baracuda.Bedrock.PlayerLoop;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace Baracuda.UI
{
    public static class DOTweenExtensions
    {
        public static TweenerCore<float, float, FloatOptions> DOCharacterSpacing(this TMP_Text target, float endValue,
            float duration)
        {
            return DOTween.To(() => target.characterSpacing, x => target.characterSpacing = x, endValue, duration);
        }

        public static void ShutdownTweens(this Component target)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode is false)
            {
                return;
            }
            if (Gameloop.IsQuitting)
            {
                return;
            }
#endif
            target.DOKill();
        }

        public static Tween DOTypeWriter(this TMP_Text textField, string text, float speed = 15,
            Action onCharAdded = null, Ease ease = Ease.Linear)
        {
            var stringBuilder = new StringBuilder();
            const string OpenTag = "<color=#00000000>";
            const string CloseTag = "</color>";
            var length = text.Length;
            var duration = length / speed;

            stringBuilder.Append(OpenTag);
            stringBuilder.Append(text);
            stringBuilder.Append(CloseTag);

            var tween = DOVirtual.Int(1, length, duration, index =>
            {
                stringBuilder.Replace(OpenTag, "");
                stringBuilder.Insert(index, OpenTag);
                textField.SetText(stringBuilder);
                onCharAdded?.Invoke();
            }).SetEase(ease);

            return tween;
        }
    }
}