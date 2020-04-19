using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectiveManager
{
    protected Transform GameUI;
    protected Transform TutorialCanvas;
    public virtual void Destroy() { }
    public virtual void Update() { }
    public ObjectiveManager()
    {
        GameUI = GameObject.Find("GameUI").transform;
        TutorialCanvas = GameObject.Find("TutorialCanvas").transform;
    }
}
