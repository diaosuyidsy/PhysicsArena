using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtBirdFood : MonoBehaviour
{
    [HideInInspector]
    public int LastHolder = 7; // Initializ the Last Holder to an error value to assert game register before use

    public void RegisterLastHolder(int playernumber)
    {
        LastHolder = playernumber;
    }
}
