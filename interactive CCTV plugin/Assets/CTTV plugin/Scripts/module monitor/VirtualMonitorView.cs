using UnityEngine;

namespace Surveillance.Monitors
{
    public sealed class VirtualMonitorView : MonoBehaviour
    {[Header("Material configuration")]
        [SerializeField] private int materialIndex = 0;
        [SerializeField] private string texturePropertyName = "_BaseMap"; // _BaseMap для URP, _MainTex для Standard

        private Renderer _targetRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private int _texturePropertyId;
        private bool _isInitialized;

        private void Awake()
        {
            LazyInit();
        }
        
        private void LazyInit()
        {
            if (_isInitialized) return;

            _targetRenderer = GetComponentInChildren<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _texturePropertyId = Shader.PropertyToID(texturePropertyName);
            
            _isInitialized = true;
        }

        public void Show(Texture texture)
        {
            LazyInit();

            if (_targetRenderer == null || texture == null)
                return;

            _targetRenderer.GetPropertyBlock(_propertyBlock, materialIndex);
            _propertyBlock.SetTexture(_texturePropertyId, texture);
            _targetRenderer.SetPropertyBlock(_propertyBlock, materialIndex);
        }
    }
}