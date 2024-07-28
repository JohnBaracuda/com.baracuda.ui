using DG.Tweening;

namespace Baracuda.UI
{
    /// <summary>
    ///     Interface representing a UI window with methods for handling sorting order,
    ///     custom opening/closing effects, and default group assignment.
    /// </summary>
    public interface IWindow
    {
        /// <summary>
        ///     Sets the sorting order of the window relative to all other windows.
        /// </summary>
        /// <param name="sortingOrder">The sorting order value to set.</param>
        void SetSortingOrder(int sortingOrder);

        /// <summary>
        ///     Called to play custom opening or fade-in effects for the window.
        /// </summary>
        /// <param name="context">The UIContext in which the window is being shown.</param>
        /// <returns>A DOTween Sequence representing the opening animation.</returns>
        Sequence ShowAsync(UIContext context);

        /// <summary>
        ///     Called to play custom closing or fade-out effects for the window.
        /// </summary>
        /// <param name="context">The UIContext in which the window is being hidden.</param>
        /// <returns>A DOTween Sequence representing the closing animation.</returns>
        Sequence HideAsync(UIContext context);

        /// <summary>
        ///     Gets the default UIGroup to which the window is assigned.
        /// </summary>
        /// <returns>The default UIGroupReference for the window.</returns>
        UIGroup GetDefaultGroup();
    }
}