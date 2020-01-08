using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentBase : MonoBehaviour
{
    public EquipmentDataBase EquipmentDataBase;
    public GameObject Owner;
    public bool CanBePickedUp;

    public virtual void OnSpawn()
    {
        CanBePickedUp = true;
    }

    public virtual void OnPickUp(GameObject owner)
    {
        Owner = owner;
    }

    protected virtual void _OnDespawn()
    {
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            _OnDespawn();
            return;
        }
    }


}
