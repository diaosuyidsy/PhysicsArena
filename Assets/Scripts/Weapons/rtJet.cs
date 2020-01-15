using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtJet : EquipmentBase
{
    public JetData m_JetData { get; set; }

    protected override void Awake()
    {
        base.Awake();
        m_JetData = EquipmentDataBase as JetData;
    }

    // public override void OnPickUp(GameObject owner)
    // {
    //     base.OnPickUp(owner);
    //     transform.parent = owner.GetComponent<PlayerController>().Chest.transform;
    // }
}
