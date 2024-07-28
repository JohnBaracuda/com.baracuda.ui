using System;
using System.Collections.Generic;

namespace Baracuda.UI
{
    public interface IUICommand
    {
        IWindow Window { get; }
        Type WindowType { get; }
        UICommandType CommandType { get; }
        List<Type> AboveTypes { get; }
        List<Type> BellowTypes { get; }
        bool SkipAnimation { get; }
        bool ExecuteImmediate { get; }
        int Priority { get; }
        UIGroup? Group { get; }
    }
}