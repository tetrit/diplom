using System;
using UnityEngine;

namespace Surveillance.Settings
{[DefaultExecutionOrder(-100)] 
    public class ConfigurationManager : MonoBehaviour
    {
        public static ConfigurationManager Instance { get; private set; }[Header("Единый файл настроек системы")]
        [SerializeField] private SystemConfigurationSO currentConfig;

        public SystemConfigurationSO CurrentConfig => currentConfig;

    
        public event Action<SystemConfigurationSO> OnConfigurationChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            ApplyConfiguration();
        }
        
        public void ApplyConfiguration()
        {
            if (currentConfig != null)
            {
                OnConfigurationChanged?.Invoke(currentConfig);
                Debug.Log("[Настройки] Новые параметры применены к системе.");
            }
        }
    }
}