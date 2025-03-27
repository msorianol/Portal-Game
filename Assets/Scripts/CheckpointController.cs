using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckpointController : MonoBehaviour
{
    [SerializeField] private GameObject m_SavedGameIcon;

    public UnityEvent m_OnCheckpointEntered;

    private bool isChecked = false;

    private void OnEnable()
    {
        Player_Controller.OnCheckpointEntered += ShowIcon;
    }

    private void OnDisable()
    {
        Player_Controller.OnCheckpointEntered -= ShowIcon;
    }

    private void Start()
    {
        m_SavedGameIcon.SetActive(false);
    }

    public Vector3 CheckPointPosition()
    {
        return transform.position;
    }

    public bool IsChecked()
    {
        return isChecked;
    }

    public void SetChecked(bool value)
    {
        isChecked = value;
    }

    private void ShowIcon(bool l_Checkpoint)
    {
        if (l_Checkpoint)
        {
            StartCoroutine(ShowIconCoruotine());
            m_OnCheckpointEntered?.Invoke();
        }
    }

    private IEnumerator ShowIconCoruotine()
    {
        m_SavedGameIcon.SetActive(true);
        yield return new WaitForSeconds(4.0f);
        m_SavedGameIcon.SetActive(false);
    }
}
