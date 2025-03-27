using UnityEngine;

public class PlayerLauncher : MonoBehaviour
{
    [SerializeField] private CharacterController m_CharacterController;
    [SerializeField] private Transform m_TargetLandingPosition;
    [SerializeField] private float m_LaunchHeight;
    private Vector3 m_HorizontalVelocity;
    private float m_VerticalSpeed;
    private bool m_IsLaunching = false;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_LaunchSound;

    private void OnEnable()
    {
        Player_Controller.OnPlayerLaunched += LaunchToTarget;
    }

    private void OnDisable()
    {
        Player_Controller.OnPlayerLaunched -= LaunchToTarget;
    }

    private void FixedUpdate()
    {
        if (!m_IsLaunching) return;

        m_VerticalSpeed += Physics.gravity.y * Time.fixedDeltaTime;

        Vector3 l_Movement = m_HorizontalVelocity * Time.fixedDeltaTime;
        l_Movement.y = m_VerticalSpeed * Time.fixedDeltaTime;

        m_CharacterController.Move(l_Movement);

        if (m_CharacterController.isGrounded && m_VerticalSpeed <= 0)
        {
            m_IsLaunching = false;
            m_VerticalSpeed = 0;
            m_HorizontalVelocity = Vector3.zero;
            m_CharacterController.GetComponent<Player_Controller>().m_CanMove = true;
        }
    }

    public void LaunchToTarget()
    {
        m_CharacterController.GetComponent<Player_Controller>().m_CanMove = false;                                                     
        m_IsLaunching = true;

        SoundsManager.instance.PlaySoundClip(m_LaunchSound, transform, 0.2f);

        Vector3 l_StartPosition = transform.position;
        Vector3 l_TargetPosition = m_TargetLandingPosition.position;

        float l_Gravity = Mathf.Abs(Physics.gravity.y);
        float l_VerticalDistance = l_TargetPosition.y - l_StartPosition.y;

        Vector3 l_HorizontalDisplacement = new Vector3(l_TargetPosition.x - l_StartPosition.x, 0, l_TargetPosition.z - l_StartPosition.z);
        float l_HorizontalDistance = l_HorizontalDisplacement.magnitude;

        float l_TimeToPeak = Mathf.Sqrt(2 * m_LaunchHeight / l_Gravity);
        float l_TimeToFall = Mathf.Sqrt(2 * (m_LaunchHeight - l_VerticalDistance) / l_Gravity);
        float l_TotalFlightTime = l_TimeToPeak + l_TimeToFall;

        m_VerticalSpeed = Mathf.Sqrt(2 * l_Gravity * m_LaunchHeight);

        Vector3 horizontalDirection = l_HorizontalDisplacement.normalized;
        m_HorizontalVelocity = horizontalDirection * (l_HorizontalDistance / l_TotalFlightTime);
    }
}
