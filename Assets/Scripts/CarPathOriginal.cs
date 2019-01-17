using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPathOriginal : MonoBehaviour
{
    public Transform WaypointsHolder;
    public float speed;
    public int current;
    public float RotationSpeed = 0.01f;
    public float maxAnglePerSecond = 10f;
    public BombVFXController VFXController;
    public SpriteRenderer Dotline;
    public Color DotlineNormalColor;
    public Color Team1Color;
    public Color Team2Color;

    private int dir;
    private bool ending = false;
    private int winner;
    private Transform[] target;
    private HashSet<int> _playerInCircle;

    private int team1Tracker = 0;
    private int team2Tracker = 0;

    private void Start()
    {
        _playerInCircle = new HashSet<int>();
        target = new Transform[WaypointsHolder.childCount];
        for (int i = 0; i < WaypointsHolder.childCount; i++)
        {
            target[i] = WaypointsHolder.GetChild(i);
        }
    }

    private void Update()
    {
        if (ending)
        {
            Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
            transform.position = pos;
            GameManager.GM.GameOver(winner, gameObject);
            if (winner == 2)
            {
                GameManager.GM.TeamRed1Explosion.SetActive(true);
            }
            else
            {
                GameManager.GM.TeamBlue2Explosion.SetActive(true);
            }
            return;
        }

        // When the car gets to Team 1's home, it stops and print out "Team 1 wins."
        if (current == target.Length - 1)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            ending = true;
            winner = 1;
            Camera.main.GetComponent<CameraController>().OnWinCameraZoom(transform);
            return;
        }

        // When the car gets to Team 2's home, it stops and print out "Team 2 wins."
        if (current == 0)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            ending = true;
            winner = 2;
            Camera.main.GetComponent<CameraController>().OnWinCameraZoom(transform);
            return;
        }
        // When Team 1 player enters trigger, move the car toward next element in an array.
        else if (team1Tracker > 0 && team2Tracker == 0 && current < target.Length)
        {
            // Change the color to Team1's
            Dotline.color = Team1Color;
            // Statistics: Record everyone that is on cart for the during of their stay
            _recordPlayerOnCart();
            // Statistics End
            if (transform.position != target[current].position)
            {
                // If wrong direction, change [current].
                if (dir == 2)
                {
                    current++;
                }

                dir = 1;
                // Move car toward target location.
                Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
                transform.position = pos;
                smoothRotation(target[current].transform, false);
            }
            // If it became the same position, then turn toward the next target location
            else
            {
                // Apply Bomb Visual Effect
                VFXController.VisualEffect(true, current, WaypointsHolder.childCount);
                // End Apply Bomb Visual Effect
                current++;
            }
        }
        // When Team 2 player enters trigger, move the car toward previous element in array.
        else if (team2Tracker > 0 && team1Tracker == 0 && current >= 0)
        {
            // Change Dotline Color to Team2
            Dotline.color = Team2Color;
            // Statistics: Record everyone that is on cart for the during of their stay
            _recordPlayerOnCart();
            // Statistics End
            if (transform.position != target[current].position)
            {
                // If wrong direction, change [current]   
                if (dir == 1)
                {
                    current--;
                }

                dir = 2;
                // Move car toward target location.
                Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
                transform.position = pos;
                smoothRotation(target[current].transform, true);

            }
            else
            {
                // Apply Bomb Visual Effect
                VFXController.VisualEffect(true, current, WaypointsHolder.childCount);
                // End Apply Bomb Visual Effect
                current--;
            }
        }
        else
        {
            // Apply Bomb Visual Effect
            VFXController.VisualEffect(false, 0, 0);
            // End Apply Bomb Visual Effect
            // Change Dotline Color to normal
            Dotline.color = DotlineNormalColor;
        }
    }

    private void _recordPlayerOnCart()
    {
        foreach (int playernum in _playerInCircle)
        {
            // Make sure that the controller number does not go wrong
            if (GameManager.GM.CartTime.Count > playernum)
            {
                GameManager.GM.CartTime[playernum] += Time.deltaTime;
            }
            else
            {
                Debug.LogError("Something is wrong with the controller number");
            }
        }
    }

    // This function detects if anyone went inside the thing
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Team1"))
        {
            team1Tracker++;
            _playerInCircle.Add(other.GetComponent<PlayerController>().PlayerNumber);
        }
        else if (other.CompareTag("Team2"))
        {
            team2Tracker++;
            _playerInCircle.Add(other.GetComponent<PlayerController>().PlayerNumber);
        }
    }

    // This function detects if anyone went outside the thing
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Team1"))
        {
            team1Tracker--;
            _playerInCircle.Remove(other.GetComponent<PlayerController>().PlayerNumber);
        }
        else if (other.CompareTag("Team2"))
        {
            team2Tracker--;
            _playerInCircle.Remove(other.GetComponent<PlayerController>().PlayerNumber);
        }
    }

    public void EnableBombVFX()
    {
        VFXController.VisualEffect(true, current, WaypointsHolder.childCount);
    }

    private void smoothRotation(Transform _target, bool invert)
    {
        // first calculate the look vector as normal
        Vector3 targetDirection = invert ? (-_target.position + transform.position).normalized : (_target.position - transform.position).normalized;
        Vector3 currentForward = transform.forward;
        Vector3 newForward = Vector3.Slerp(currentForward, targetDirection, Time.deltaTime * RotationSpeed);

        // now check if the new vector is rotating more than allowed
        float angle = Vector3.Angle(currentForward, newForward);
        float maxAngle = maxAnglePerSecond * Time.deltaTime;
        if (angle > maxAngle)
        {
            // it's rotating too fast, clamp the vector
            newForward = Vector3.Slerp(currentForward, newForward, maxAngle / angle);
        }

        // assign the new forward to the transform
        transform.forward = newForward;
    }

    public void IncrementTracker(bool increment, TeamNum tn)
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
