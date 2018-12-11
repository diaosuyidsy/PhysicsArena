using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPath : MonoBehaviour
{
    public Transform WaypointsHolder;
    public float speed;
    public int current;
    public float RotationSpeed = 0.01f;
    public float maxAnglePerSecond = 10f;

    private int dir;
    private bool ending = false;
    private int winner;
    private Transform[] target;

    private int team1Tracker = 0;
    private int team2Tracker = 0;

    private void Start ()
    {
        target = new Transform[WaypointsHolder.childCount];
        for (int i = 0; i < WaypointsHolder.childCount; i++)
        {
            target[i] = WaypointsHolder.GetChild (i);
        }
    }

    private void Update ()
    {
        if (ending)
        {
            GameManager.GM.GameOver (winner);
            return;
        }

        // When the car gets to Team 1's home, it stops and print out "Team 1 wins."
        if (current == target.Length)
        {
            GetComponent<Rigidbody> ().velocity = Vector3.zero;
            ending = true;
            winner = 1;
            return;
        }

        // When the car gets to Team 2's home, it stops and print out "Team 2 wins."
        if (current == -1)
        {
            GetComponent<Rigidbody> ().velocity = Vector3.zero;
            ending = true;
            winner = 2;
            return;
        }
        // When Team 1 player enters trigger, move the car toward next element in an array.
        else if (team1Tracker > 0 && team2Tracker == 0 && current < target.Length)
        {
            if (transform.position != target[current].position)
            {
                // If wrong direction, change [current].
                if (dir == 2)
                {
                    current++;
                }

                dir = 1;
                // Move car toward target location.
                Vector3 pos = Vector3.MoveTowards (transform.position, target[current].position, speed * Time.deltaTime);
                //GetComponent<Rigidbody> ().MovePosition (pos);
                transform.position = pos;
                smoothRotation (target[current].transform, false);
                //Vector3 relativePos = -target[current].position + transform.position;
                //Quaternion wantedRotation = Quaternion.LookRotation (relativePos);
                //transform.rotation = Quaternion.Lerp (transform.rotation, wantedRotation, RotationSpeed);
            }
            // If it became the same position, then turn toward the next target location
            else
            {
                current++;
            }
        }
        // When Team 2 player enters trigger, move the car toward previous element in array.
        else if (team2Tracker > 0 && team1Tracker == 0 && current >= 0)
        {
            if (transform.position != target[current].position)
            {
                // If wrong direction, change [current]   
                if (dir == 1)
                {
                    current--;
                }

                dir = 2;
                // Move car toward target location.
                Vector3 pos = Vector3.MoveTowards (transform.position, target[current].position, speed * Time.deltaTime);
                //GetComponent<Rigidbody> ().MovePosition (pos);
                transform.position = pos;
                smoothRotation (target[current].transform, true);

            }
            else
            {
                current--;
                //Vector3 relativePos = target[current].position - transform.position;
                //Quaternion wantedRotation = Quaternion.LookRotation (relativePos);
                //transform.rotation = Quaternion.Lerp (transform.rotation, wantedRotation, RotationSpeed);
            }
        }
    }

    private void smoothRotation (Transform _target, bool invert)
    {
        // first calculate the look vector as normal
        Vector3 targetDirection = invert ? (-_target.position + transform.position).normalized : (_target.position - transform.position).normalized;
        Vector3 currentForward = transform.forward;
        Vector3 newForward = Vector3.Slerp (currentForward, targetDirection, Time.deltaTime * RotationSpeed);

        // now check if the new vector is rotating more than allowed
        float angle = Vector3.Angle (currentForward, newForward);
        float maxAngle = maxAnglePerSecond * Time.deltaTime;
        if (angle > maxAngle)
        {
            // it's rotating too fast, clamp the vector
            newForward = Vector3.Slerp (currentForward, newForward, maxAngle / angle);
        }

        // assign the new forward to the transform
        transform.forward = newForward;
    }

    public void IncrementTracker (bool increment, TeamNum tn)
    {
        if (tn == TeamNum.Team1)
        {
            team1Tracker += increment ? 1 : -1;
        }
        else
        {
            team2Tracker += increment ? 1 : -1;
        }
    }
}
