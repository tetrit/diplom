using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

namespace Surveillance.Settings
{

    public class SettingsUIController : MonoBehaviour
    {
        [Header("UI Элементы")]
        public Slider confidenceSlider;
        public Toggle showFallbackToggle;
        
        [Tooltip("Поле ввода для фильтрации классов (например: car, person)")]
        public TMP_InputField allowedClassesInput; 
        
        private void Start()
        {

            var current = ConfigurationManager.Instance.CurrentConfig;
            
            if (confidenceSlider) 
                confidenceSlider.value = current.RecognitionSettings.ConfidenceThreshold;
            
            if (showFallbackToggle) 
                showFallbackToggle.isOn = current.DisplaySettings.ShowFallbackWhenSourceMissing;
            

            if (allowedClassesInput && current.RecognitionSettings.AllowedClasses != null) 
            {
                allowedClassesInput.text = string.Join(", ", current.RecognitionSettings.AllowedClasses);
            }
        }


        public void OnSaveButtonClicked()
        {
            var config = ConfigurationManager.Instance.CurrentConfig;


            if (confidenceSlider) 
                config.RecognitionSettings.ConfidenceThreshold = confidenceSlider.value;
            
            if (showFallbackToggle) 
                config.DisplaySettings.ShowFallbackWhenSourceMissing = showFallbackToggle.isOn;


            if (allowedClassesInput)
            {

                string[] parsedClasses = allowedClassesInput.text.Split(
                    new char[] { ',', ' ' }, 
                    StringSplitOptions.RemoveEmptyEntries
                );
                
                config.RecognitionSettings.AllowedClasses = new List<string>(parsedClasses);
            }
            
            ConfigurationManager.Instance.ApplyConfiguration();
        }
    }
}