using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dedos : MonoBehaviour {

    public float Fuerza = 4000;
    Rigidbody rb;
    bool tomado = false;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
    void OnCollisionEnter(Collision col)
    {
        if (!tomado)
        {
            SpringJoint sp = gameObject.AddComponent<SpringJoint>();
            sp.connectedBody = col.rigidbody;
            sp.spring = 12000;
            sp.breakForce = Fuerza;
            tomado = true;
        }
    }

    void OnJointBreak()
    {
        tomado = false;
        
    }
	// Update is called once per frame
	void Update () {
		
	}
}
