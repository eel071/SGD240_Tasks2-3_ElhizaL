using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObjectTV : SmartObject
{
    public bool IsOn { get; protected set; } = false;

    public void ToggleState()
    {
        IsOn = !IsOn;

        Debug.Log($"Tv is now {(IsOn ? "ON" : "OFF")}");
    }
    
}
