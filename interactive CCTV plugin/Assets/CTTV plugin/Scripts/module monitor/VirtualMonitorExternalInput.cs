using System;
using UnityEngine;

namespace Surveillance.Monitors
{
    public sealed class VirtualMonitorExternalInput : MonoBehaviour
    {
        public event Action<Texture> TextureChanged;

        public Texture CurrentTexture { get; private set; }

        public void SetTexture(Texture texture)
        {
            CurrentTexture = texture;
            TextureChanged?.Invoke(CurrentTexture);
        }

        public void Clear()
        {
            CurrentTexture = null;
            TextureChanged?.Invoke(null);
        }
    }
}