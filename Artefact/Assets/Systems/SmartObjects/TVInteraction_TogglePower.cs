using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SmartObjectTV))]
public class TVInteraction_TogglePower : SimpleInteraction
{
    protected SmartObjectTV LinkedTV;

    protected void Awake()
    {
        LinkedTV = GetComponent<SmartObjectTV>();
    }

    public override void Perform(MonoBehaviour performer, UnityAction<BaseInteraction> onCompleted)
    {
        LinkedTV.ToggleState();
        base.Perform(performer, onCompleted);
    }
}
