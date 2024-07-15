namespace Baracuda.UI
{
    public readonly struct UIContext
    {
        public readonly IWindow From;
        public readonly IWindow To;
        public readonly bool IsFlush;

        private UIContext(IWindow from, IWindow to, bool isFlush)
        {
            From = from;
            To = to;
            IsFlush = isFlush;
        }

        public static Builder Create()
        {
            return new Builder();
        }

        public ref struct Builder
        {
            private IWindow _from;
            private IWindow _to;
            private bool _isFlush;

            public Builder FromWindow(IWindow from)
            {
                _from = from;
                return this;
            }

            public Builder ToWindow(IWindow to)
            {
                _to = to;
                return this;
            }

            public Builder Flush()
            {
                _isFlush = true;
                return this;
            }

            public UIContext Build()
            {
                return new UIContext(_from, _to, _isFlush);
            }
        }
    }
}