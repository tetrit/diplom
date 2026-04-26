using Surveillance.Cameras;
using Unity.VisualScripting;
using UnityEngine;

public class MovableVirtualCamera : VirtualCameraSource
{
    [Min(0.1f)][SerializeField] private float rotateSpeed = 1;
    [Range(10, 180)][SerializeField] private int maxAngle = 30;

    private Quaternion _startRotation;
    private float _currentAngleDif = 0;
    private int _direction = 1;

    

    void Awake()
    {
        base.Awake();
        _startRotation = transform.rotation;
    }


    void Update()
    {
        base.Update();

        if (!_isStreaming) return;
        
        _currentAngleDif += rotateSpeed * _direction * Time.deltaTime;

        if (_currentAngleDif >= maxAngle)
        {
            _direction = -1;
            _currentAngleDif = maxAngle;
        }

        if (_currentAngleDif <= -maxAngle)
        {
            _direction = 1;
            _currentAngleDif = -maxAngle;
        }
        transform.rotation = _startRotation * Quaternion.Euler(0, _currentAngleDif, 0);
    }

    
}
