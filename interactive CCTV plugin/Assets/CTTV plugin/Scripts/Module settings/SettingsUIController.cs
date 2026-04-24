using UnityEngine;
using UnityEngine.UI;

namespace Surveillance.Settings
{
    // Пример того, как UI взаимодействует с модулем настроек
    public class SettingsUIController : MonoBehaviour
    {[Header("UI Элементы")]
        public Slider confidenceSlider;
        public Toggle showFallbackToggle;
        
        private void Start()
        {
            // Читаем текущие настройки при открытии меню
            var current = ConfigurationManager.Instance.CurrentConfig;
            
            if (confidenceSlider) confidenceSlider.value = current.RecognitionSettings.ConfidenceThreshold;
            
            if (showFallbackToggle) showFallbackToggle.isOn = current.DisplaySettings.ShowFallbackWhenSourceMissing;
        }

        // Вызывается кнопкой в UI
        public void OnSaveButtonClicked()
        {
            var config = ConfigurationManager.Instance.CurrentConfig;

            // Передача изменений 
            if (confidenceSlider) config.RecognitionSettings.ConfidenceThreshold = confidenceSlider.value;
            
            if (showFallbackToggle) config.DisplaySettings.ShowFallbackWhenSourceMissing = showFallbackToggle.isOn;

            // ИСПРАВЛЕНО: Даем команду Менеджеру применить настройки (разослать всем)
            ConfigurationManager.Instance.ApplyConfiguration();
        }
    }
}