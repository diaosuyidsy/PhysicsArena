using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Image StartImage;
    public Image IntroImage;
    public Image GameImage;
    public Image WinImage;

    private Player _player1;
    private Player _player2;
    private Player _player3;
    private Player _player4;
    private Player _player5;
    private Player _player6;
    private bool _pressedFirst = false;
    private bool _pressedSecond = false;
    private bool _allowClickTwice = false;
    private bool _allowClickThrice = false;


    private void Awake ()
    {
        _player1 = ReInput.players.GetPlayer (0);
        _player2 = ReInput.players.GetPlayer (1);
        _player3 = ReInput.players.GetPlayer (2);
        _player4 = ReInput.players.GetPlayer (3);
        _player5 = ReInput.players.GetPlayer (4);
        _player6 = ReInput.players.GetPlayer (5);
    }


    private void Update ()
    {
        Color temp = StartImage.color;
        temp.a = Mathf.Sin (Time.time);
        if (temp.a <= 0f) temp.a *= -1f;
        StartImage.color = temp;

        Color temp2 = GameImage.color;
        temp2.a = Mathf.Sin (Time.time);
        if (temp2.a <= 0f) temp2.a *= -1f;
        GameImage.color = temp2;

        if (!_pressedFirst && (_player1.GetButton ("Jump") ||
           _player2.GetButton ("Jump") ||
           _player3.GetButton ("Jump") ||
           _player4.GetButton ("Jump") ||
           _player5.GetButton ("Jump") ||
           _player6.GetButton ("Jump"))
           )
        {
            _pressedFirst = true;
            print ("Pressed Once");
            IntroImage.gameObject.SetActive (true);
            StartCoroutine (AllowClickTwice (5f));
        }
        if (!_pressedSecond && _allowClickTwice && (_player1.GetButton ("Jump") ||
           _player2.GetButton ("Jump") ||
           _player3.GetButton ("Jump") ||
           _player4.GetButton ("Jump") ||
           _player5.GetButton ("Jump") ||
           _player6.GetButton ("Jump"))
           )
        {
            _pressedSecond = true;
            WinImage.gameObject.SetActive (true);
            StartCoroutine (AllowClickThrice (3f));
        }

        if (_allowClickThrice && (_player1.GetButton ("Jump") ||
           _player2.GetButton ("Jump") ||
           _player3.GetButton ("Jump") ||
           _player4.GetButton ("Jump") ||
           _player5.GetButton ("Jump") ||
           _player6.GetButton ("Jump"))
           )
        {
            SceneManager.LoadScene (1);
        }
        if (Input.GetKeyDown (KeyCode.Space))
        {
            SceneManager.LoadScene (1);
        }
    }

    IEnumerator AllowClickTwice (float time)
    {
        yield return new WaitForSeconds (time);
        _allowClickTwice = true;
        GameImage.gameObject.SetActive (true);
    }

    IEnumerator AllowClickThrice (float time)
    {
        yield return new WaitForSeconds (time);
        _allowClickThrice = true;
    }
}
