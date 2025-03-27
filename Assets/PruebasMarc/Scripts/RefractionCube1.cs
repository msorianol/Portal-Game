using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefractionCube1 : MonoBehaviour
{
    [SerializeField] private LineRenderer m_Laser;
    [SerializeField] private LayerMask m_PhysicsLayerMask;
    [SerializeField] private float m_MaxDistance = 50.0f;
    private bool m_CreateReFraction = false;

    private void Update()
    {
        m_Laser.gameObject.SetActive(m_CreateReFraction);
        m_CreateReFraction = false;
    }

    public void CreateRefraction()
    {
        if (m_CreateReFraction)
            return;

        m_CreateReFraction = true;

        Ray l_Ray = new Ray(m_Laser.transform.position, m_Laser.transform.forward);
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxDistance, m_PhysicsLayerMask.value))
        {
            m_Laser.SetPosition(1, new Vector3(0.0f, 0.0f, l_RaycastHit.distance));
            m_Laser.gameObject.SetActive(true);

            if (l_RaycastHit.collider.CompareTag("RefractionCube"))
            {
                m_Laser.SetPosition(1, new Vector3(0.0f, 0.0f, l_RaycastHit.distance));
            }
        }
        else
            m_Laser.gameObject.SetActive(false);
    }
}

