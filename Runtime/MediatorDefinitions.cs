using Baracuda.Mediator.Generation;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static Baracuda.UI.MediatorDefinitions;

[assembly: GenerateMediatorFor(typeof(PlayerInput),
    MediatorTypes = MediatorTypes.ValueAsset,
    NameSpace = NameSpace,
    Subfolder = Subfolder)]

[assembly: GenerateMediatorFor(typeof(EventSystem),
    MediatorTypes = MediatorTypes.ValueAsset,
    NameSpace = NameSpace,
    Subfolder = Subfolder)]

namespace Baracuda.UI
{
    public static class MediatorDefinitions
    {
        public const string NameSpace = "Baracuda.UI";
        public const string Subfolder = "Mediator";
    }
}