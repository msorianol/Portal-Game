using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    private Animator m_FadeAnimator;

    void Start()
    {
        m_FadeAnimator = GetComponent<Animator>();
    }

    public void StartFade()
    {
        m_FadeAnimator.SetTrigger("FadeStarting");
    }

    public void RestartPosition()
    {
        GameManager.instance.RestartPosition();
    }
}
