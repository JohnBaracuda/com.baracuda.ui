using Baracuda.Bedrock.Input;

namespace Baracuda.UI
{
    /// <summary>
    ///     Interface to receive a callback when the window is opening, before its show animation is played.
    ///     This callback can be received on the window itself and on other components on the same gameObject.
    /// </summary>
    public interface IWindowOpening
    {
        /// <summary>
        ///     Called when the window is opening, before its show animation is played.
        /// </summary>
        void OnWindowOpening();
    }

    /// <summary>
    ///     Interface to receive a callback when the window has opened, after its show animation is completed.
    ///     This callback can be received on the window itself and on other components on the same gameObject.
    /// </summary>
    public interface IWindowOpened
    {
        /// <summary>
        ///     Called when the window has opened, after its show animation is played.
        /// </summary>
        void OnWindowOpened();
    }

    /// <summary>
    ///     Interface to receive a callback when the window is closing, before its hide animation is played.
    ///     This callback can be received on the window itself and on other components on the same gameObject.
    /// </summary>
    public interface IWindowClosing
    {
        /// <summary>
        ///     Called when the window is closing, before its hide animation is played.
        /// </summary>
        void OnWindowClosing();
    }

    /// <summary>
    ///     Interface to receive a callback when the window has closed, after its hide animation is completed.
    ///     This callback can be received on the window itself and on other components on the same gameObject.
    /// </summary>
    public interface IWindowClosed
    {
        /// <summary>
        ///     Called when the window has closed, after its hide animation is played.
        /// </summary>
        void OnWindowClosed();
    }

    /// <summary>
    ///     Interface to receive a callback when the window gains focus.
    ///     This callback can be received on the window itself and on other components on the same gameObject.
    /// </summary>
    public interface IWindowFocusGain
    {
        /// <summary>
        ///     Called when the window gains focus.
        /// </summary>
        void OnWindowGainedFocus();
    }

    /// <summary>
    ///     Interface to receive a callback when the window loses focus.
    ///     This callback can be received on the window itself and on other components on the same gameObject.
    /// </summary>
    public interface IWindowFocusLost
    {
        /// <summary>
        ///     Called when the window loses focus.
        /// </summary>
        void OnWindowLostFocus();
    }

    /// <summary>
    ///     Interface to handle both gaining and losing focus events on the window.
    ///     These callbacks can be received on the window itself and on other components on the same gameObject.
    /// </summary>
    public interface IWindowFocus : IWindowFocusGain, IWindowFocusLost
    {
    }

    /// <summary>
    ///     Interface to receive and optionally consume 'Back Pressed' or 'Return' callbacks on the UI.
    /// </summary>
    public interface IWindowEscapeHandler
    {
        /// <summary>
        ///     Override this method to handle 'Back Pressed' or 'Return' callbacks on the UI.
        /// </summary>
        /// <returns>
        ///     Returns an <see cref="EscapeUsage" /> value indicating whether the escape action was consumed or ignored.
        /// </returns>
        public EscapeUsage OnEscapePressed()
        {
            return EscapeUsage.IgnoredEscape;
        }
    }
}