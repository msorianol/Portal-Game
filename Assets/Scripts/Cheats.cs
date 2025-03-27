using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatsController : MonoBehaviour
{
#if UNITY_EDITOR    

    [SerializeField] private TMP_Text m_CheatsText;
    private bool m_GodMode;
    private bool m_Cheats;
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            m_Cheats = true;
        }

        if (m_Cheats)
        {
            if (Input.GetKey(KeyCode.LeftControl))
                Time.timeScale = 3.0f;
            else Time.timeScale = 1.0f;


            if (Input.GetKeyDown(KeyCode.J))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);


            if (Input.GetKeyDown(KeyCode.G))
                m_GodMode = true;


            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                m_GodMode = false;
                m_CheatsText.gameObject.SetActive(false);
                m_Cheats = false;
            }
        }
    }

#endif
}
