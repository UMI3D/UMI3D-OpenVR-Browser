using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDeactivateMonobehaviour : MonoBehaviour
{
    public event Action<bool> activated;

    private void OnEnable()
    {
        activated?.Invoke(true);
    }

    private void OnDisable()
    {
        activated?.Invoke(false);
    }
}
