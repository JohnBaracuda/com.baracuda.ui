using JetBrains.Annotations;
using UnityEngine.UI;

namespace Baracuda.UI
{
    public ref struct NavigationBuilder
    {
        private Selectable _target;
        private Selectable _onUp;
        private Selectable _onDown;
        private Selectable _onLeft;
        private Selectable _onRight;
        private Navigation.Mode _mode;

        [MustUseReturnValue]
        public static NavigationBuilder Create(Selectable target, Navigation.Mode mode = Navigation.Mode.Explicit)
        {
            return new NavigationBuilder
            {
                _mode = mode,
                _target = target
            };
        }

        [MustUseReturnValue]
        public NavigationBuilder WithOnUp(Selectable selectable)
        {
            _onUp = selectable;
            return this;
        }

        [MustUseReturnValue]
        public NavigationBuilder WithOnDown(Selectable selectable)
        {
            _onDown = selectable;
            return this;
        }

        [MustUseReturnValue]
        public NavigationBuilder WithOnLeft(Selectable selectable)
        {
            _onLeft = selectable;
            return this;
        }

        [MustUseReturnValue]
        public NavigationBuilder WithOnRight(Selectable selectable)
        {
            _onRight = selectable;
            return this;
        }

        public readonly void Build()
        {
            _target.navigation = new Navigation
            {
                mode = _mode,
                selectOnUp = _onUp,
                selectOnDown = _onDown,
                selectOnLeft = _onLeft,
                selectOnRight = _onRight
            };
        }
    }
}