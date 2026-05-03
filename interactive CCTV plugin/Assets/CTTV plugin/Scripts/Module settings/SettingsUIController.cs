using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Не забываем подключить TextMeshPro

namespace Surveillance.Settings
{
    // Пример того, как UI взаимодействует с модулем настроек
    public class SettingsUIController : MonoBehaviour
    {
        [Header("UI Элементы")]
        public Slider confidenceSlider;
        public Toggle showFallbackToggle;
        
        [Tooltip("Поле ввода для фильтрации классов (например: car, person)")]
        public TMP_InputField allowedClassesInput; // Поле для ввода списка
        
        private void Start()
        {
            // Читаем текущие настройки при открытии меню
            var current = ConfigurationManager.Instance.CurrentConfig;
            
            if (confidenceSlider) 
                confidenceSlider.value = current.RecognitionSettings.ConfidenceThreshold;
            
            if (showFallbackToggle) 
                showFallbackToggle.isOn = current.DisplaySettings.ShowFallbackWhenSourceMissing;
            
            // Выводим текущие разрешенные классы текстом через запятую
            if (allowedClassesInput && current.RecognitionSettings.AllowedClasses != null) 
            {
                allowedClassesInput.text = string.Join(", ", current.RecognitionSettings.AllowedClasses);
            }
        }

        // Вызывается кнопкой "Save" / "Применить" в UI
        public void OnSaveButtonClicked()
        {
            var config = ConfigurationManager.Instance.CurrentConfig;

            // Передача изменений слайдеров и чекбоксов
            if (confidenceSlider) 
                config.RecognitionSettings.ConfidenceThreshold = confidenceSlider.value;
            
            if (showFallbackToggle) 
                config.DisplaySettings.ShowFallbackWhenSourceMissing = showFallbackToggle.isOn;

            // --- СОХРАНЕНИЕ СПИСКА КЛАССОВ ---
            if (allowedClassesInput)
            {
                // Разбиваем строку на массив по запятым и пробелам, удаляя пустые вхождения.
                // Это позволяет вводить как "car,person", так и "car, person" или даже "car person"
                string[] parsedClasses = allowedClassesInput.text.Split(
                    new char[] { ',', ' ' }, 
                    StringSplitOptions.RemoveEmptyEntries
                );
                
                // Перезаписываем список в основном конфиге
                config.RecognitionSettings.AllowedClasses = new List<string>(parsedClasses);
            }

            // Даем команду Менеджеру применить настройки (разослать изменения всем модулям: инференсу, UI и т.д.)
            ConfigurationManager.Instance.ApplyConfiguration();
        }
    }
}