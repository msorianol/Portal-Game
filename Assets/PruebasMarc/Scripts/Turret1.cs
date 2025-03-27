using UnityEngine;

public class Turret1 : MonoBehaviour
{
    [SerializeField] private LineRenderer m_Laser;
    [SerializeField] private LayerMask m_PhysicsLayerMask;
    [SerializeField] private float m_MaxDistance = 50.0f;

    private void Update()
    {
        Ray l_Ray = new Ray(m_Laser.transform.position, m_Laser.transform.forward);
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxDistance, m_PhysicsLayerMask.value))
        {
            m_Laser.SetPosition(1, new Vector3(0.0f, 0.0f, l_RaycastHit.distance));
            m_Laser.gameObject.SetActive(true);

            if (l_RaycastHit.collider.CompareTag("RefractionCube"))
            {
                m_Laser.SetPosition(1, new Vector3(0.0f, 0.0f, l_RaycastHit.distance));
                l_RaycastHit.collider.GetComponent<RefractionCube1>().CreateRefraction();

            }
        }
        else
            m_Laser.gameObject.SetActive(false);
    }
}