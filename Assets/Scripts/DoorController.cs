using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Animator m_DoorAnimator;

    public enum DoorType
    {
        TriggerDoor,
        ButtonDoor
    }

    public DoorType m_DoorType;

    public bool m_OpenDoor { get; set; }

    [Header("Sounds")]
    [SerializeField] private AudioClip m_OpenDoorSound;
    [SerializeField] private AudioClip m_CloseDoorSound;

    private void Update()
    {
        HandleButtonDoor();
    }

    private void HandleButtonDoor()
    {
        if (m_OpenDoor == true && m_DoorType == DoorType.ButtonDoor)
        {
            m_DoorAnimator.SetBool("OpenDoor", true);
            m_DoorAnimator.SetBool("CloseDoor", false);
        }
        else if (m_OpenDoor == false && m_DoorType == DoorType.ButtonDoor)
        {
            m_DoorAnimator.SetBool("OpenDoor", false);
            m_DoorAnimator.SetBool("CloseDoor", true);
        }
    }

    public void PlayOpenDoorSound()
    {
        SoundsManager.instance.PlaySoundClip(m_OpenDoorSound, transform, 0.2f);
    }

    public void PlayCloseDoorSound()
    {
        SoundsManager.instance.PlaySoundClip(m_CloseDoorSound, transform, 0.2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && m_DoorType == DoorType.TriggerDoor)
        {
            m_DoorAnimator.SetBool("OpenDoor", true);
            m_DoorAnimator.SetBool("CloseDoor", false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && m_DoorType == DoorType.TriggerDoor)
        {
            m_DoorAnimator.SetBool("OpenDoor", false);
            m_DoorAnimator.SetBool("CloseDoor", true);
        }
    }
}
