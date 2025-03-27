using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DestroySurface : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private AudioClip m_CubeDestroyedSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CompanionCube"))
        {
            SoundsManager.instance.PlaySoundClip(m_CubeDestroyedSound, transform, 0.2f);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Turret"))
        {
            Turret l_Turret = other.GetComponent<Turret>();
            StartCoroutine(l_Turret.TurretDeathCoroutine(l_Turret, other.gameObject));
        }
        else if (other.CompareTag("RefractionCube"))
        {
            SoundsManager.instance.PlaySoundClip(m_CubeDestroyedSound, transform, 0.2f);
            Destroy(other.gameObject);
        }
    }
}
