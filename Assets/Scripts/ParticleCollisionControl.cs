using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Obi;

[RequireComponent (typeof (ObiSolver))]
public class ParticleCollisionControl : MonoBehaviour
{
    private bool test = true;
    ObiSolver solver;

    Obi.ObiSolver.ObiCollisionEventArgs collisionEvent;

    void Awake ()
    {
        solver = GetComponent<Obi.ObiSolver> ();
    }

    void OnEnable ()
    {
        solver.OnCollision += Solver_OnCollision;
    }

    void OnDisable ()
    {
        solver.OnCollision -= Solver_OnCollision;
    }

    void Solver_OnCollision (object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
    {
        if (!test) return;
        foreach (Oni.Contact contact in e.contacts)
        {
            // this one is an actual collision:
            if (contact.distance < 0.01)
            {
                var col = ObiCollider.idToCollider[contact.other];
                if (col != null && GameManager.GM.AllPlayers == (GameManager.GM.AllPlayers | (1 << col.gameObject.layer)))
                {
                    if (col.gameObject.GetComponent<Rigidbody> ().velocity.magnitude >= GameManager.GM.DropWeaponVelocityThreshold)
                    {
                        print ("Drop");
                        test = false;
                        col.SendMessageUpwards ("DropHelper");
                    }
                }
            }
        }
    }
}
