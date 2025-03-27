using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportableObjects : MonoBehaviour, ITeleport
{
    public LayerMask m_LayerMask;
    public BoxCollider m_BoxCollider;
    public Rigidbody m_Rigidbody;
    public Portal m_Portal;
    public Vector3 m_MovementDirection;
    public Vector3 m_StartSize;
    public bool m_EnterPortal;
    public bool m_Catched;
    private int m_LayerMaskWeapon;
    private Player_Controller m_Player;


    public virtual void Start()
    {
        m_Player = GameManager.instance.GetPlayer();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_BoxCollider = GetComponent<BoxCollider>();
        m_StartSize = transform.localScale;
        m_LayerMaskWeapon = LayerMask.NameToLayer("Weapon");
    }

    public virtual void FixedUpdate()
    {
        if (m_Rigidbody.velocity.magnitude > 0.01f)
        {
            m_MovementDirection = m_Rigidbody.velocity.normalized;
        }
        else
        {
            m_MovementDirection = Vector3.zero;
        }
    }
    public virtual void Update()
    {
        if (m_EnterPortal)
        {
            Vector3 l_Offset = m_Portal.transform.position - transform.position;
            float l_Dot = Vector3.Dot(m_Portal.transform.forward, l_Offset.normalized);

            if (l_Dot > 0.5f && !m_Catched)
            {
                Teleport(m_Portal);
                m_EnterPortal = false;
            }
        }

        if (m_Catched)
        {
            ChangeLayer(m_LayerMaskWeapon);

            Vector3 l_Direction = m_Player.transform.position - transform.position;
            Ray l_ray = new Ray(transform.position, l_Direction);

            if (Physics.Raycast(l_ray, out RaycastHit l_Hit, m_LayerMask.value))
            {
                if (l_Hit.collider.CompareTag("Player") || m_EnterPortal)
                {
                    ChangeLayer(m_LayerMaskWeapon);
                }
                else
                    ChangeLayer(0);
            }

        }
        else
        {
            ChangeLayer(0);

            if (!m_EnterPortal)
            {
                if (m_Portal != null)
                    Physics.IgnoreCollision(m_Portal.m_WallPortaled, m_BoxCollider, false);
            }
        }
    }

    public void Teleport(Portal l_portal)
    {
        float l_Velocity = m_Rigidbody.velocity.magnitude;
        l_portal.m_MirrorPortal.CloneObject(this.gameObject, l_portal);

        Vector3 l_Position = transform.position;
        Vector3 l_LocalPosition = l_portal.m_OtherPortalTransform.InverseTransformPoint(l_Position);
        Vector3 l_WorldPosition = l_portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        Vector3 l_LocalForward = l_portal.m_OtherPortalTransform.InverseTransformDirection(transform.forward);
        Vector3 l_WorldForward = l_portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        transform.position = l_WorldPosition;
        transform.forward = l_WorldForward;


        m_Rigidbody.velocity = l_portal.m_MirrorPortal.transform.forward * l_Velocity;
        transform.localScale = m_StartSize * l_portal.m_MirrorPortal.m_PortalSize;
        Physics.IgnoreCollision(m_Portal.m_WallPortaled, m_BoxCollider, false);
    }

    public void ChangeLayer(int layer)
    {
        foreach (Transform children in this.transform)
        {
            children.gameObject.layer = layer;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            m_Portal = other.GetComponent<Portal>();
            m_EnterPortal = true;
            Physics.IgnoreCollision(m_Portal.m_WallPortaled, m_BoxCollider, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            Physics.IgnoreCollision(m_Portal.m_WallPortaled, m_BoxCollider, false);
        }
    }
}
