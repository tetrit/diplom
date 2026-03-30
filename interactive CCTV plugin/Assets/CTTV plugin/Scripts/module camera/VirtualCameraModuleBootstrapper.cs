using UnityEngine;

namespace Surveillance.Cameras
{
    [DefaultExecutionOrder(-1000)]
    public sealed class VirtualCameraModuleBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = true;

        private bool _ownsService;
        private IVirtualCameraService _service;

        private void Awake()
        {
            if (ServiceLocator.Has<IVirtualCameraService>())
            {
                _service = ServiceLocator.Get<IVirtualCameraService>();

                if (dontDestroyOnLoad)
                    Destroy(gameObject);

                return;
            }

            _service = new VirtualCameraService();
            ServiceLocator.Register<IVirtualCameraService>(_service);
            _ownsService = true;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_ownsService && ServiceLocator.Has<IVirtualCameraService>())
            {
                ServiceLocator.Unregister<IVirtualCameraService>();
            }
        }
    }
}