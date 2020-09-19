using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

[BoltGlobalBehaviour("BrawlModeReforgedBolt")]
public class BoltNetworkCallback : GlobalEventListener
{
    public override void SceneLoadLocalDone(string scene)
    {
        var spawnPosition = new Vector3(14.5f, 0.5f, -17f);
        BoltNetwork.Instantiate(BoltPrefabs.Bolt_Player_Wind, spawnPosition, Quaternion.identity);
        // BoltNetwork.Instantiate(BoltPrefabs.Bolt_Player_Phoenix_Yellow, spawnPosition, Quaternion.identity);

    }
}
