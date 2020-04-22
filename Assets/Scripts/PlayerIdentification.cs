using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class used for Identification
public class PlayerIdentification : MonoBehaviour
{
    public int ColorIndex;
    public int SelfLayer;
    [Tooltip("0 is Team1, 1 is Team2")]
    public int PlayerTeamNumber;
    public string PlayerTeamNumberString { get { return PlayerTeamNumber == 0 ? "Team1" : "Team2"; } }
}
