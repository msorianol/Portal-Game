using UnityEngine;
using UnityEngine.Events;

public class LaserReceiver : MonoBehaviour
{
    public UnityEvent m_OnLaserReceived;

    private void OnEnable()
    {
        RefractionCube.OnLaserReceived += LaserReceived;
        Portal.OnLaserReceived += LaserReceived;
    }

    private void OnDisable()
    {
        RefractionCube.OnLaserReceived -= LaserReceived;
        Portal.OnLaserReceived -= LaserReceived;
    }

    private void Awake()
    {
        gameObject.SetActive(true);
    }

    private void LaserReceived()
    {
        m_OnLaserReceived?.Invoke();
    }
}
