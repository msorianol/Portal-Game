using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionController : TeleportableObjects
{
    public static Action OnCubeDestroyed; 

    public override void Update()
    {
        base.Update();

        if (GameManager.instance.m_Restart)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        OnCubeDestroyed?.Invoke();
    }
}