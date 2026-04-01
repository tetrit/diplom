using UnityEngine;

namespace Surveillance.Recognition
{
    [DefaultExecutionOrder(-900)]
    public sealed class RecognitionModuleBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = true;

        private bool ownsService;

        private void Awake()
        {
            if (ServiceLocator.Has<IRecognitionService>())
            {
                if (dontDestroyOnLoad)
                    Destroy(gameObject);

                return;
            }

            ServiceLocator.Register<IRecognitionService>(new RecognitionService());
            ownsService = true;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (ownsService && ServiceLocator.Has<IRecognitionService>())
                ServiceLocator.Unregister<IRecognitionService>();
        }
    }
}