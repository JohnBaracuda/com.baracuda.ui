namespace Baracuda.UI.Components
{
    public interface IWindowOpening
    {
        void OnWindowOpening();
    }

    public interface IWindowOpened
    {
        void OnWindowOpened();
    }

    public interface IWindowClosing
    {
        void OnWindowClosing();
    }

    public interface IWindowClosed
    {
        void OnWindowClosed();
    }

    public interface IWindowTransitions : IWindowOpening, IWindowClosing, IWindowOpened, IWindowClosed
    {
    }

    public interface IWindowFocusGain
    {
        void OnWindowGainedFocus();
    }

    public interface IWindowFocusLost
    {
        void OnWindowLostFocus();
    }

    public interface IWindowFocus : IWindowFocusGain, IWindowFocusLost
    {
    }

    public interface IWindowEscapeHandler
    {
        /// <summary>
        ///     Override this method to receive and optionally consume 'Back Pressed' or 'Return' callbacks on the UI.
        /// </summary>
        public EscapeUsage OnEscapePressed()
        {
            return EscapeUsage.IgnoredEscape;
        }
    }
}