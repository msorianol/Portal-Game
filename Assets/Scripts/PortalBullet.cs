using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PortalBullet : MonoBehaviour
{
    private Vector3 m_Direction; 
    public float m_Speed;
    private bool m_Moving; 
    public bool m_Colisioned = false;

    void Update()
    {
        if (m_Moving)
        {
            RaycastHit l_hit;
            Vector3 l_PositionNextFrame = transform.position + m_Direction * m_Speed * Time.deltaTime;
            float l_Distance = Vector3.Distance(l_PositionNextFrame, transform.position);
    
            if(Physics.Raycast(transform.position, m_Direction,out l_hit, l_Distance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (l_hit.collider.CompareTag("WhiteWall"))
                {
                    m_Moving = false;   
                    this.gameObject.SetActive(false);
                    m_Colisioned = true;    
                }
            }

            transform.position = l_PositionNextFrame;
        }
    }

    public void Shoot(Vector3 origin, Vector3 direction)
    {
        transform.position = origin;    
        m_Direction = direction.normalized;    
        m_Moving = true;
        m_Colisioned = false;
        transform.forward = m_Direction; 
    }
}
