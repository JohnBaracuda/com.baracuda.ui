using System;

namespace Baracuda.UI
{
    [Flags]
    public enum TransitionSettings
    {
        None = 0,
        HideUnderlyingWindows = 1,
        HideWhenLosingFocus = 2,
        CompleteTransitionsSequential = 4
    }
}