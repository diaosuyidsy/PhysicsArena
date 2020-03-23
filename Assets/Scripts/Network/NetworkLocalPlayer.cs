using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Rewired;

public class NetworkLocalPlayer : NetworkBehaviour
{
    private void Update()
    {
        if (!isLocalPlayer) return;
        CheckInput();
    }

    private void CheckInput()
    {
        if (ReInput.players.GetPlayer(0).GetButtonDown("Jump"))
        {
            CmdPressButton("Jump");
        }

        if (ReInput.players.GetPlayer(0).GetButtonDown("Block"))
        {
            CmdPressButton("Block");
        }

        if (ReInput.players.GetPlayer(0).GetButtonDown("Left Trigger"))
        {
            CmdPressButton("Left Trigger");
        }
    }

    [Command]
    private void CmdPressButton(string ButtonName)
    {
        EventManager.Instance.TriggerEvent(new ButtonPressed(ButtonName));
    }
}
