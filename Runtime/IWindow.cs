using DG.Tweening;

namespace Baracuda.UI
{
    public interface IWindow
    {
        /// <summary>
        ///     Set the sorting order of the window relative to all other windows.
        /// </summary>
        public void SetSortingOrder(int sortingOrder);

        /// <summary>
        ///     Called on the component to play custom opening or fade in effects.
        /// </summary>
        public Sequence ShowAsync(UIContext context);

        /// <summary>
        ///     Called on the component to play custom closing or fade out effects.
        /// </summary>
        public Sequence HideAsync(UIContext context);

        /// <summary>
        ///     The UIGroup the window is assigned to by default.
        /// </summary>
        public UIGroupReference GetDefaultGroup();
    }
}