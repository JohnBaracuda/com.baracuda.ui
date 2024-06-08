using Baracuda.Bedrock.Services;

namespace Baracuda.UI
{
    public static class UILocator
    {
        public static T Get<T>() where T : UIWindow
        {
            return ServiceLocator.Get<UIManager>().Container.Get<T>();
        }
    }
}