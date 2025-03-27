using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefractionCube : TeleportableObjects  
{
    public LineRenderer m_LaserRenderer;
    public float m_MaxDistance = 50.0f;
    private bool m_CreateRefraction = false;

    public static Action OnLaserReceived;

    public override void Update()
    {
        base.Update();

        m_LaserRenderer.gameObject.SetActive(m_CreateRefraction);
        m_CreateRefraction = false;
    }

    public void CreateRefraction()
    {
        if (m_CreateRefraction)
            return;

        m_CreateRefraction = true;

        Ray l_Ray = new Ray(m_LaserRenderer.transform.position, m_LaserRenderer.transform.forward);

        if (Physics.Raycast(l_Ray, out RaycastHit l_HitInfo, m_MaxDistance, m_LayerMask.value))
        {
            float l_Distance = l_HitInfo.distance;
            m_LaserRenderer.SetPosition(1, new Vector3(0, 0, l_Distance));
            m_LaserRenderer.gameObject.SetActive(true);

            if (l_HitInfo.collider.CompareTag("RefractionCube"))
            {
                //Reflect ray
                l_HitInfo.collider.GetComponent<RefractionCube>().CreateRefraction();
            }
            else if (l_HitInfo.collider.CompareTag("Player") && l_HitInfo.collider.TryGetComponent(out PlayerLifeController l_PlayerLifeController))
            {
                float l_LaserDuration = l_PlayerLifeController.m_TimeToKillPlayer;
                float l_PlayerHealth = l_PlayerLifeController.m_MaxPlayerHealth;
                float l_DamagePerSecond = l_PlayerHealth / l_LaserDuration;
                GameManager.instance.ReportPlayerDamaged(l_DamagePerSecond);
            }
            else if (l_HitInfo.collider.CompareTag("LaserReceiver"))
            {
                OnLaserReceived?.Invoke();
            }
            else if (l_HitInfo.collider.CompareTag("Portal"))
            {
                m_Portal = l_HitInfo.collider.GetComponent<Portal>();
                m_LaserRenderer.SetPosition(1, new Vector3(0, 0, l_HitInfo.distance + Vector3.Distance(l_HitInfo.point, m_Portal.transform.position) + 0.5f));
                m_Portal.RayReflection(l_Ray, l_HitInfo);
            }

        }
        else
            m_LaserRenderer.gameObject.SetActive(false);
    }
}
