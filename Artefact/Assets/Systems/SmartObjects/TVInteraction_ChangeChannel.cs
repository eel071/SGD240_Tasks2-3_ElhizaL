using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SmartObjectTV))]
public class TVInteraction_ChangeChannel : SimpleInteraction
{
    protected SmartObjectTV LinkedTV;

    protected void Awake()
    {
        LinkedTV = GetComponent<SmartObjectTV>();
    }

    public override bool CanPerform()
    {
        return base.CanPerform() && LinkedTV.IsOn;
    }
}
