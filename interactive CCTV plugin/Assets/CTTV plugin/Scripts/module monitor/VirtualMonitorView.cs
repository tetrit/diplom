using UnityEngine;

namespace Surveillance.Monitors
{
    [RequireComponent(typeof(Renderer))]
    public sealed class VirtualMonitorView : MonoBehaviour
    {
        [Header("Renderer")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private int materialIndex = 0;

        [Header("Material properties")]
        [SerializeField] private string texturePropertyName = "_BaseMap";
        [SerializeField] private string colorPropertyName = "_BaseColor";

        [Header("Visual states")]
        [SerializeField] private Texture fallbackTexture;
        [SerializeField] private Color activeTint = Color.white;
        [SerializeField] private Color noSignalTint = new(0.1f, 0.1f, 0.1f, 1f);

        private MaterialPropertyBlock _propertyBlock;
        private int _texturePropertyId;
        private int _colorPropertyId;

        private void Awake()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            _propertyBlock = new MaterialPropertyBlock();
            _texturePropertyId = Shader.PropertyToID(texturePropertyName);
            _colorPropertyId = Shader.PropertyToID(colorPropertyName);
        }

        public void Show(Texture texture)
        {
            if (targetRenderer == null)
                return;

            targetRenderer.GetPropertyBlock(_propertyBlock, materialIndex);

            _propertyBlock.SetTexture(_texturePropertyId, texture);
            _propertyBlock.SetColor(_colorPropertyId, activeTint);

            targetRenderer.SetPropertyBlock(_propertyBlock, materialIndex);
        }

        public void ShowFallback()
        {
            if (targetRenderer == null)
                return;
            if (_propertyBlock != null)
            {
                targetRenderer.GetPropertyBlock(_propertyBlock, materialIndex);

                _propertyBlock.SetTexture(_texturePropertyId, fallbackTexture);
                _propertyBlock.SetColor(_colorPropertyId, noSignalTint);

                targetRenderer.SetPropertyBlock(_propertyBlock, materialIndex);
            }
        }

        public void SetActiveTint(Color color)
        {
            activeTint = color;
        }

        public void SetNoSignalTint(Color color)
        {
            noSignalTint = color;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();
        }
#endif
    }
}