using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Action Manager takes in all happening action in one frame 
// and process then at the very beginning of the next frame
public class ActionManager
{
    // Key: FrameCount
    // Value: List of Actions that Happened that Frame
    private Dictionary<int, List<GameActions>> _actionDict;
    public ActionManager()
    {
        _actionDict = new Dictionary<int, List<GameActions>>();
    }

    public void RegisterAction(int frameCount, GameActions ev)
    {
        // If action dict does not contain current framecount
        // Create a new list then add the gameactions to the list
        if (!_actionDict.ContainsKey(frameCount))
        {
            _actionDict.Add(frameCount, new List<GameActions>());
            _actionDict[frameCount].Add(ev);
        }
        else
        {
            _actionDict[frameCount].Add(ev);
        }
    }

    public void Update()
    {
        int frameCount = Time.frameCount;
        if (_actionDict.ContainsKey(frameCount - 1))
        {
            Execute(frameCount - 1);
            _actionDict.Remove(frameCount - 1);
        }
    }

    private void Execute(int frameCount)
    {
        Debug.Assert(_actionDict.ContainsKey(frameCount), "Aciton Dictionary does not contain required frame");
        var gameactions = _actionDict[frameCount];
        for (int i = 0; i < gameactions.Count; i++)
        {
            GameActions actions = gameactions[i];
            actions.Execute();
        }
    }

    public void Destroy()
    {

    }
}
