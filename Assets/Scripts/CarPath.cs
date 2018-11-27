using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPath : MonoBehaviour
{
    public Transform WaypointsHolder;
    public float speed;
    public int current;
    public SpriteRenderer DotLine;
    public Color Team1Color;
    public Color Team2Color;
    public Color NeutralColor;

    private int dir;
    private bool ending = false;
    private int winner;
    private Transform[] target;

    private int team1Tracker = 0;
    private int team2Tracker = 0;

    private void Start()
    {
        target = new Transform[WaypointsHolder.childCount];
        for (int i = 0; i < WaypointsHolder.childCount; i++)
        {
            target[i] = WaypointsHolder.GetChild(i);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Team1"))
        {
            team1Tracker++;
        }

        if (other.CompareTag("Team2"))
        {
            team2Tracker++;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Team1"))
        {
            team1Tracker--;
        }

        if (other.CompareTag("Team2"))
        {
            team2Tracker--;
        }
    }

    private void Update()
    {
        if (ending)
        {
            GameManager.GM.GameOver(winner);
            return;
        }

        // When the car gets to Team 1's home, it stops and print out "Team 1 wins."
        if (current == target.Length)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            ending = true;
            winner = 1;
        }

        // When the car gets to Team 2's home, it stops and print out "Team 2 wins."
        if (current == -1)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            ending = true;
            winner = 2;
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
                // Rotate car facing target location.			
                Vector3 relativePos = target[current].position - transform.position;
                transform.rotation = Quaternion.LookRotation(relativePos);
                // Move car toward target location.
                Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
                GetComponent<Rigidbody>().MovePosition(pos);
                // Change dotted line to Team1
                DotLine.color = Team1Color;
            }
            else current++;
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
                // Rotate car facing target location.
                Vector3 relativePos = target[current].position - transform.position;
                transform.rotation = Quaternion.LookRotation(relativePos);
                // Move car toward target location.
                Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
                GetComponent<Rigidbody>().MovePosition(pos);
                // Change dotted line to Team2 color
                DotLine.color = Team2Color;
            }
            else current--;
        }
        else if (team1Tracker == 0 && team2Tracker == 0)
        {
            //车不动惹
            DotLine.color = NeutralColor;
        }
        else if (team1Tracker > 0 && team2Tracker > 0)
        {
            //车也不动惹
            DotLine.color = NeutralColor;
        }
    }

/*    void turnTowardsTarget(Transform _target)
    {
        Vector3 diff = _target.position - transform.position;
        diff.Normalize();

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, rot_z, 0f);
    }*/

}
