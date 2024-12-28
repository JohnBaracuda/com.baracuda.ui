using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI
{
    public static class UIUtility
    {
        [Pure]
        public static Navigation WithOnTop(this Navigation navigation, Selectable target)
        {
            navigation.selectOnUp = target;
            navigation.mode = Navigation.Mode.Explicit;
            return navigation;
        }

        [Pure]
        public static Navigation WithOnRight(this Navigation navigation, Selectable target)
        {
            navigation.selectOnRight = target;
            navigation.mode = Navigation.Mode.Explicit;
            return navigation;
        }

        [Pure]
        public static Navigation WithOnDown(this Navigation navigation, Selectable target)
        {
            navigation.selectOnDown = target;
            navigation.mode = Navigation.Mode.Explicit;
            return navigation;
        }

        [Pure]
        public static Navigation WithOnLeft(this Navigation navigation, Selectable target)
        {
            navigation.selectOnLeft = target;
            navigation.mode = Navigation.Mode.Explicit;
            return navigation;
        }

        public static void SetSelectOnTop(this Selectable selectable, Selectable target)
        {
            selectable.navigation = selectable.navigation.WithOnTop(target);
        }

        public static void SetSelectOnRight(this Selectable selectable, Selectable target)
        {
            selectable.navigation = selectable.navigation.WithOnRight(target);
        }

        public static void SetSelectOnDown(this Selectable selectable, Selectable target)
        {
            selectable.navigation = selectable.navigation.WithOnDown(target);
        }

        public static void SetSelectOnLeft(this Selectable selectable, Selectable target)
        {
            selectable.navigation = selectable.navigation.WithOnLeft(target);
        }

        [MustUseReturnValue]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NavigationBuilder SetupNavigation(this Selectable selectable, Navigation.Mode mode = Navigation.Mode.Explicit)
        {
            return NavigationBuilder.Create(selectable, mode);
        }

        [MustUseReturnValue]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Selectable FindBestHorizontalSelectable(Selectable target, IList<Selectable> selectables)
        {
            if (target.transform is not RectTransform targetRectTransform)
            {
                return null;
            }

            var targetScreenPoint = targetRectTransform.GetCenter();
            var targetYPositionOnScreen = targetScreenPoint.y;

            Selectable bestSelectable = null;
            var closestYDifference = float.MaxValue;

            foreach (var selectable in selectables)
            {
                if (selectable == target || selectable.transform is not RectTransform selectableRectTransform)
                {
                    continue;
                }

                var selectableScreenPoint = selectableRectTransform.GetCenter();
                var selectableYPositionOnScreen = selectableScreenPoint.y;

                var yDifference = Mathf.Abs(selectableYPositionOnScreen - targetYPositionOnScreen);
                if (yDifference < closestYDifference)
                {
                    closestYDifference = yDifference;
                    bestSelectable = selectable;
                }
            }

            return bestSelectable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetCenter(this RectTransform rectTransform)
        {
            // Get the world position of the RectTransform
            var worldPosition = rectTransform.position;

            // Calculate the offset caused by the pivot
            var pivotOffset = new Vector3(
                (0.5f - rectTransform.pivot.x) * rectTransform.rect.width * rectTransform.lossyScale.x,
                (0.5f - rectTransform.pivot.y) * rectTransform.rect.height * rectTransform.lossyScale.y,
                0
            );

            // Add the offset to the world position
            return worldPosition + pivotOffset;
        }
    }
}