using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApocalypseArenaManager : MonoBehaviour
{
    public GameObject LeftTrap;
    public GameObject RightTrap;

    // Start is called before the first frame update
    void Start()
    {
        TrapSetUp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void TrapSetUp()
    {
        float r = Random.Range(0.0f, 1.0f);

        if (r < 0.5f)
        {
            LeftTrap.GetComponent<NormalTrap>().SetUp(true);
            RightTrap.GetComponent<NormalTrap>().SetUp(false);
        }
        else
        {
            LeftTrap.GetComponent<NormalTrap>().SetUp(false);
            RightTrap.GetComponent<NormalTrap>().SetUp(true);
        }
    }
}
