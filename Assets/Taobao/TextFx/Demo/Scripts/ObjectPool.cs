using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TextFx
{
public class ObjectPool<T> where T : UnityEngine.Component
{
	class PoolObjectData
	{
		public T m_type_obj;
		public GameObject m_gameObject;
		public Transform m_transform;
	}

	GameObject m_prefab_reference;
	List<PoolObjectData> m_pool;
	List<PoolObjectData> m_in_use_pool;
	Transform m_pool_container;
	string m_pool_name = "";
	int m_total_pool_size = 0;
	Dictionary<string, PoolObjectData> m_obj_hash_lookup;
	System.Action<T> m_object_creation_callback;
	System.Action<T> m_object_recycle_steps_override;

	public ObjectPool(GameObject prefab, int start_pool_size, string poolNameOverride = "", System.Action<T> object_creation_callback = null, System.Action<T> object_recycle_steps_override = null)
	{
		m_prefab_reference = prefab;

		m_object_creation_callback = object_creation_callback;
		m_object_recycle_steps_override = object_recycle_steps_override;

		// Initialise lists
		m_pool = new List<PoolObjectData>();
		m_in_use_pool = new List<PoolObjectData>();

		m_obj_hash_lookup = new Dictionary<string, PoolObjectData>();

		m_pool_name = poolNameOverride != "" ? poolNameOverride : typeof(T).Name;

		m_pool_container = (new GameObject("ObjectPool - " + m_pool_name)).transform;

		for (int idx = 0; idx < start_pool_size; idx++)
		{
			AddNewPoolItem();
		}
	}

	PoolObjectData AddNewPoolItem()
	{
		GameObject new_obj = MonoBehaviour.Instantiate(m_prefab_reference) as GameObject;
		Transform obj_transform = new_obj.transform;
		T obj_t_ref = new_obj.GetComponent<T>();

		PoolObjectData object_data = new PoolObjectData() { m_gameObject = new_obj, m_transform = obj_transform, m_type_obj = obj_t_ref };

		// Add hash code reference to lookup dictionary for later backwards lookup
		m_obj_hash_lookup.Add(obj_t_ref.GetHashCode().ToString(), object_data);

		// Add object data instance to pool list
		m_pool.Add(object_data);

		obj_transform.SetParent( m_pool_container );

		new_obj.SetActive(false);
		new_obj.name = m_pool_name + " #" + m_total_pool_size;

		m_total_pool_size++;

		if (m_object_creation_callback != null)
			m_object_creation_callback(obj_t_ref);

		return object_data;
	}

	// Gets an available object. Either from the object pool, or by instantiating a new one.
	public T GetObject(bool activateObject = true)
	{
		T obj = null;

		if (m_pool.Count > 0)
		{
			// Activate next object
			PoolObjectData object_data = m_pool[0];
			object_data.m_gameObject.SetActive(activateObject);

			obj = object_data.m_type_obj;

			m_pool.RemoveAt(0);
			m_in_use_pool.Add(object_data);
		}
		else
		{
			// Add a new object
			PoolObjectData object_data = AddNewPoolItem();

			m_pool.RemoveAt(0);
			m_in_use_pool.Add(object_data);

			// Activate new object instance
			object_data.m_gameObject.SetActive(activateObject);

			obj = object_data.m_type_obj;
		}

		return obj;
	}

	public void Recycle(T obj)
	{
		int hash_code = obj.GetHashCode();

		if (m_obj_hash_lookup.ContainsKey(hash_code.ToString()))
		{
			PoolObjectData object_data = m_obj_hash_lookup[hash_code.ToString()];

			if (m_in_use_pool.Contains(object_data))
			{
				object_data.m_transform.SetParent(m_pool_container);

				// Check to see if a recycle action override has been defined or not.
				if (m_object_recycle_steps_override != null)
					m_object_recycle_steps_override(object_data.m_type_obj);
				else
					object_data.m_gameObject.SetActive(false);

				m_in_use_pool.Remove(object_data);
				m_pool.Add(object_data);
			}
			//else object is already in the pool of inactive objects and doesn't need recycling
		}
		else
		{
			Debug.LogWarning("You're trying to \"recycle\" a pool object, which isn't already part of this pool");
		}
	}

	public void ResetPoolAll(System.Action<T> bespoke_callback = null)
	{
		foreach (PoolObjectData object_data in m_in_use_pool)
		{
			object_data.m_gameObject.SetActive(false);
			object_data.m_transform.parent = m_pool_container;

			m_pool.Add(object_data);

			if (bespoke_callback != null)
				bespoke_callback(object_data.m_type_obj);
		}

		m_in_use_pool = new List<PoolObjectData>();
	}
}
}