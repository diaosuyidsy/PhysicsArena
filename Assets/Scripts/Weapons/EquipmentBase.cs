using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentBase : MonoBehaviour
{
    public EquipmentDataBase EquipmentDataBase;
    public GameObject Owner { get; set; }
    public bool CanBePickedUp { get; set; }

    protected Vector3 m_TargetPosition;
    protected Vector3 m_TargetRotation;
    protected int _ammo;

    protected virtual void Awake()
    {
        OnSpawn();
    }

    public virtual void OnUse()
    {
        _weaponUsedOnce();
    }
    protected virtual void _weaponUsedOnce()
    {
        _ammo--;
        if (_ammo <= 0)
        {
            CanBePickedUp = false;
            if (Owner != null)
                Owner.GetComponent<PlayerController>().ForceDropEquipment(EquipmentDataBase.EquipmentPositionType);
        }
    }

    /// <summary>
    /// When Euipment is spawned from spawner
    /// </summary>
    public virtual void OnSpawn()
    {
        CanBePickedUp = true;
        _ammo = EquipmentDataBase.Ammo;
    }

    public virtual void OnPickUp(GameObject owner)
    {
        Owner = owner;
        GetComponent<Rigidbody>().isKinematic = true;
        gameObject.layer = owner.layer;
    }

    protected virtual void _OnDespawn()
    {
        Owner = null;
        GetComponent<Rigidbody>().isKinematic = false;
        gameObject.layer = LayerMask.NameToLayer("Pickup");
        _ammo = EquipmentDataBase.Ammo;
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        gameObject.SetActive(false);
    }

    protected virtual void Update()
    {
        // Adjust Position and Rotation
        if (Owner != null)
        {
            switch (EquipmentDataBase.EquipmentPositionType)
            {
                case EquipmentPositionType.OnBack:
                    m_TargetPosition = Owner.GetComponent<PlayerController>().Chest.transform.position;
                    m_TargetPosition += transform.right * EquipmentDataBase.PositionAdjustment.XOffset;
                    m_TargetPosition += transform.up * EquipmentDataBase.PositionAdjustment.YOffset;
                    m_TargetPosition += transform.forward * EquipmentDataBase.PositionAdjustment.ZOffset;
                    m_TargetRotation = Owner.GetComponent<PlayerController>().Chest.transform.eulerAngles;
                    m_TargetRotation.x += EquipmentDataBase.PositionAdjustment.XRotation;
                    m_TargetRotation.y += EquipmentDataBase.PositionAdjustment.YRotation;
                    m_TargetRotation.z += EquipmentDataBase.PositionAdjustment.ZRotation;
                    break;
            }
            transform.eulerAngles = m_TargetRotation;
            transform.position = m_TargetPosition;
        }
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
