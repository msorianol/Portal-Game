using System;
using System.Collections;
using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    [SerializeField] private LineRenderer m_LaserRenderer;
    [SerializeField] private AudioClip m_LaserSound;
    [SerializeField] private float m_LaserDistance = 8f;

    private bool m_LaserActive = true;
    private bool m_SoundPlayed = false;

    private void Awake()
    {
        m_LaserRenderer.positionCount = 2;
    }

    public void Update()
    {
        if (!m_LaserActive)
        {
            m_LaserRenderer.enabled = false;
            return;
        }

        if (!m_SoundPlayed)
        {
            //SoundsManager.instance.PlayLongSound3D(m_LaserSound, transform, 0.2f, laserOnTime);
            m_SoundPlayed = true;
        }

        m_LaserRenderer.enabled = true;

        Ray l_Ray;
        RaycastHit l_RayHit;
        l_Ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(l_Ray, out l_RayHit))
        {
            m_LaserRenderer.SetPosition(0, transform.position);
            m_LaserRenderer.SetPosition(1, l_RayHit.point);

            if (l_RayHit.collider.CompareTag("Player") && l_RayHit.collider.TryGetComponent(out PlayerLifeController l_PlayerLifeController))
            {
                float l_LaserDuration = l_PlayerLifeController.m_TimeToKillPlayer;
                float l_PlayerHealth = l_PlayerLifeController.m_MaxPlayerHealth;
                float l_DamagePerSecond = l_PlayerHealth / l_LaserDuration;
                GameManager.instance.ReportPlayerDamaged(l_DamagePerSecond);
            }

            if (l_RayHit.collider.CompareTag("RefractionCube"))
            {
                l_RayHit.collider.GetComponent<RefractionCube>().CreateRefraction();
            }
            else if (l_RayHit.collider.CompareTag("Portal"))
            {
                Portal l_Portal = l_RayHit.collider.GetComponent<Portal>();
                l_Portal.RayReflection(l_Ray, l_RayHit);
            }
        }
        else
        {
            m_LaserRenderer.SetPosition(0, transform.position);
            m_LaserRenderer.SetPosition(1, transform.position + transform.forward * m_LaserDistance);
        }
    }
}
