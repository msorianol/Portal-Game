using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    [Header("Audios")]
    [SerializeField] private AudioClip m_SoundToPlay;
    [SerializeField] private float m_DelayTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(PlayDelayedSound());

            PortalWeaponController l_PortalWeaponController = other.GetComponent<PortalWeaponController>();
            l_PortalWeaponController.NewSector();
        }
    }

    private IEnumerator PlayDelayedSound()
    {
        yield return new WaitForSeconds(m_DelayTime);
        SoundsManager.instance.PlaySoundClip(m_SoundToPlay, transform, 0.04f);
        Destroy(gameObject);
    }
}
