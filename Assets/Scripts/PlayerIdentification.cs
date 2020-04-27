using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class used for Identification
public class PlayerIdentification : ObjectID
{
    public int ColorIndex;
    [Tooltip("0 is Team1, 1 is Team2")]
    public int PlayerTeamNumber;
}
