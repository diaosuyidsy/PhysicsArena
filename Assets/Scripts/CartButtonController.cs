using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartButtonController : MonoBehaviour
{
    public TeamNum Team;
    public CarPath CarScript;

    private bool _pushingDown = false;
    private bool _liftingUp = false;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private bool _pushDownRan = false;


    private void Start ()
    {
        Transform temp = transform.GetChild (0);
        _startPosition = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
        _endPosition = new Vector3 (temp.position.x, temp.position.y, temp.position.z);
    }

    private void OnCollisionEnter (Collision collision)
    {
        if (collision.collider.CompareTag (Team.ToString ()))
        {
            collision.transform.parent.parent.parent = transform.parent.parent.parent;
            if (!_pushingDown)
            {
                _pushingDown = true;
                StopAllCoroutines ();
                StartCoroutine (StartPushingDown (0.3f, Team));
            }
        }
    }

    private void OnCollisionExit (Collision collision)
    {
        if (collision.collider.CompareTag (Team.ToString ()))
        {
            collision.transform.parent.parent.parent = null;

            if (_pushingDown)
            {
                _pushingDown = false;
                StopAllCoroutines ();
                // If we stopped the pushing down coroutine before it's done
                if (!_pushDownRan)
                    CarScript.IncrementTracker (true, Team);

                StartCoroutine (StartLiftingUp (1f, Team));
            }
        }
    }

    IEnumerator StartLiftingUp (float time, TeamNum teamNum)
    {
        float elapsedTime = 0f;
        CarScript.IncrementTracker (false, teamNum);

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp (new Vector3 (transform.position.x, _endPosition.y, transform.position.z), new Vector3 (transform.position.x, _startPosition.y, transform.position.z), elapsedTime / time);
            yield return new WaitForEndOfFrame ();
        }
    }

    IEnumerator StartPushingDown (float time, TeamNum teamNum)
    {
        float elapsedTime = 0f;
        _pushDownRan = false;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp (new Vector3 (transform.position.x, _startPosition.y, transform.position.z), new Vector3 (transform.position.x, _endPosition.y, transform.position.z), elapsedTime / time);
            yield return new WaitForEndOfFrame ();
        }
        CarScript.IncrementTracker (true, teamNum);
        _pushDownRan = true;
    }
}

public enum TeamNum
{
    Team1,
    Team2,
};
