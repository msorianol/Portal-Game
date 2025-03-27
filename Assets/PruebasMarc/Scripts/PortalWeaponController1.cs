using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalWeaponController1 : MonoBehaviour
{
    [SerializeField] private GameObject m_PreviewPortal;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private List<Transform> m_ValidPoints = new List<Transform>();

    private void Update()
    {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(l_Ray, out RaycastHit l_Hit))
        {
            if (l_Hit.collider.CompareTag("WhiteWall"))
            {
                m_PreviewPortal.transform.rotation = Quaternion.LookRotation(-l_Hit.normal);
                m_PreviewPortal.transform.position = l_Hit.point + l_Hit.normal * 0.01f;

                if (IsValidPosition(l_Hit))
                {
                    m_PreviewPortal.SetActive(true);
                }
                else
                {
                    m_PreviewPortal.SetActive(false);
                }
            }
        }
        else
        {
            m_PreviewPortal.SetActive(false);
        }
    }

    public bool IsValidPosition(RaycastHit l_Hit)
    {
        foreach (Transform l_ValidPoint in m_ValidPoints)
        {
            Vector3 l_Direction = l_ValidPoint.position - m_Camera.transform.position;
            Debug.DrawRay(m_Camera.transform.position, l_Direction, Color.red);

            if (Physics.Raycast(m_Camera.transform.position, l_Direction.normalized, out RaycastHit l_ValidPointHit))
            {
                float distance = Vector3.Distance(l_ValidPointHit.point, l_ValidPoint.position);
                if (distance >= 0.1f)
                {
                    return false;
                }

                float angle = Vector3.Angle(l_Hit.normal, l_ValidPoint.forward);
                if (angle >= 2f)
                {
                    return false;
                }

                if (!l_ValidPointHit.collider.CompareTag("WhiteWall"))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}
