using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EnemyCameraRotation : MonoBehaviour
{
    [SerializeField] private GameObject m_ArmCamera; 
    [SerializeField] private GameObject m_EyeCamera;
    
    private Player_Controller m_Player;
    private void Start()
    {
        m_Player = GameManager.instance.GetPlayer().GetComponent<Player_Controller>();     
    }

    void Update()
    {
        //ARM CAMERA
        Vector3 l_Direction = m_Player.transform.position - m_ArmCamera.transform.position;
        Quaternion l_Rotation = Quaternion.LookRotation(l_Direction);

        l_Rotation.x = 0; 
        l_Rotation.z = 0;
        m_ArmCamera.transform.rotation = l_Rotation;

        //EYE CAMERA
        Quaternion l_EyeRotation = Quaternion.LookRotation(l_Direction);

        m_EyeCamera.transform.rotation = l_EyeRotation;    
    }
}
