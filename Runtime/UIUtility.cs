using JetBrains.Annotations;
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

        public static void WithSelectOnTop(this Selectable selectable, Selectable target)
        {
            selectable.navigation = selectable.navigation.WithOnTop(target);
        }

        public static void WithSelectOnRight(this Selectable selectable, Selectable target)
        {
            selectable.navigation = selectable.navigation.WithOnRight(target);
        }

        public static void WithSelectOnDown(this Selectable selectable, Selectable target)
        {
            selectable.navigation = selectable.navigation.WithOnDown(target);
        }

        public static void WithSelectOnLeft(this Selectable selectable, Selectable target)
        {
            selectable.navigation = selectable.navigation.WithOnLeft(target);
        }
    }
}