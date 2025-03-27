using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowPortalController : MonoBehaviour
{
    [SerializeField] private Portal m_Portal; 
    private CloneObjectController m_CloneObjectController;    

    public void SetCloneObject(CloneObjectController cloneObjectController)
    {
        m_CloneObjectController = cloneObjectController;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CompanionCube"))
        {
            m_Portal.CloneObject(other.gameObject, m_Portal.m_MirrorPortal);  
        }
        else if (other.CompareTag("Weapon"))
        {
            m_Portal.CloneObject(other.gameObject, m_Portal.m_MirrorPortal);
        }
        else if (other.CompareTag("Turret"))
        {
            m_Portal.CloneObject(other.gameObject, m_Portal.m_MirrorPortal);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CompanionCube"))
        {
            m_CloneObjectController.m_Clone = false; 
        }
        else if (other.CompareTag("Weapon"))
        {
            m_CloneObjectController.m_Clone = false;
        }
        else if (other.CompareTag("Turret"))
        {
            m_CloneObjectController.m_Clone = false;
        }
    }
}
