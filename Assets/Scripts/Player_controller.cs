using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_controller : MonoBehaviour {

    Rigidbody rb;
    CapsuleCollider caps;
    public float Resistencia = 10;
    public Animator anim;
    public float VelSalto = 5;
    public float Velocidad = 10;
    bool caido = false;
    
    

    void OnCollisionEnter(Collision col)
    {
        if (col.relativeVelocity.magnitude > Resistencia)
        {
            caps.enabled = false;
            rb.constraints = RigidbodyConstraints.None;
            anim.SetBool("golpeado", true);
            caido = true;
        }
    }

    void Salto()
    {
        if (!caido)
        {
            rb.AddForce(new Vector3(0, VelSalto * 100, 0), ForceMode.Impulse);
            Invoke("Salto", Random.Range(2, 10));
        }
        
    }
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        caps = GetComponent<CapsuleCollider>();
        Invoke("Salto", Random.Range(2, 10));
    }
	
	// Update is called once per frame
	void Update () {
       
    }

   void FixedUpdate()
    {
        rb.AddForce(new Vector3(transform.forward.x, 0, transform.forward.z) * Velocidad);
    }
}
