using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGunLine : MonoBehaviour
{
    public float maxLength = 16.0f;
    public GameObject hitEffect;
    public Renderer meshRenderer1;
    public Renderer meshRenderer2;
    public ParticleSystem[] hitPsArray;
    public int segmentCount = 32;
    public float globalProgressSpeed = 1f;
    public AnimationCurve shaderProgressCurve;
    public AnimationCurve lineWidthCurve;

    private LineRenderer lr;
    private Vector3[] resultVectors;
    private float dist;
    private float globalProgress;
    private Vector3 hitPosition;
    private Vector3 currentPosition;

    void Start()
    {
        globalProgress = 1f;
        lr = this.GetComponent<LineRenderer>();
        lr.positionCount = segmentCount;
        resultVectors = new Vector3[segmentCount + 1];
        for (int i = 0; i < segmentCount + 1; i++)
        {
            resultVectors[i] = transform.forward;
        }
    }

    void Update()
    {
        //Move LineRenderer One By One

        for (int i = segmentCount - 1; i > 0; i--)
        {
            resultVectors[i] = resultVectors[i - 1];
        }

        resultVectors[0] = transform.forward;
        resultVectors[segmentCount] = resultVectors[segmentCount - 1];
        float blockLength = maxLength / segmentCount;


        currentPosition = new Vector3(0, 0, 0);

        for (int i = 0; i < segmentCount; i++)
        {
            currentPosition = transform.position;
            for (int j = 0; j < i; j++)
            {
                currentPosition += resultVectors[j] * blockLength;
            }

            lr.SetPosition(i, currentPosition);
        }





        //Collision 

        for (int i = 0; i < segmentCount; i++)
        {

            currentPosition = transform.position;
            for (int j = 0; j < i; j++)
            {
                currentPosition += resultVectors[j] * blockLength;
            }
            RaycastHit hit;

            if (i == segmentCount - 1)
            {
                hitPosition = currentPosition + resultVectors[i] * blockLength;
                hitPosition = Vector3.MoveTowards(hitPosition, transform.position, 0.5f);
                if (hitEffect)
                {
                    hitEffect.transform.position = hitPosition;

                }

                dist = Vector3.Distance(hitPosition, transform.position);

            }
            else if (Physics.Raycast(currentPosition, resultVectors[i], out hit, blockLength))
            {
                hitPosition = currentPosition + resultVectors[i] * hit.distance;
                hitPosition = Vector3.MoveTowards(hitPosition, transform.position, 0.5f);
                if (hitEffect)
                {
                    hitEffect.transform.position = hitPosition;

                }

                dist = Vector3.Distance(hitPosition, transform.position);

                break;
            }
        }



        //Emit Hit Particles 

        if (hitEffect)
        {
            if (globalProgress < 0.75f)
            {
                foreach (ParticleSystem ps in hitPsArray)
                {

                    var em = ps.emission;
                    em.enabled = true;
                    //ps.enableEmission = true;
                }
            }
            else
            {
                foreach (ParticleSystem ps in hitPsArray)
                {

                    var em = ps.emission;
                    em.enabled = false;
                    //ps.enableEmission = false;
                }
            }
        }

        //Emit Particles on Collision End

        GetComponent<Renderer>().material.SetFloat("_Distance", dist);
        GetComponent<Renderer>().material.SetVector("_Position", transform.position);

        // if (Input.GetMouseButton(0))
        // {
        //     globalProgress = 0f;
        // }

        if (globalProgress <= 1f)
        {
            globalProgress += Time.deltaTime * globalProgressSpeed;
        }


        float progress = shaderProgressCurve.Evaluate(globalProgress);
        GetComponent<Renderer>().material.SetFloat("_Progress", progress);

        if (meshRenderer1 != null && meshRenderer2 != null)
        {
            meshRenderer1.material.SetFloat("_Progress", progress);
            meshRenderer2.material.SetFloat("_Progress", progress);
        }

        float width = lineWidthCurve.Evaluate(globalProgress);
        lr.widthMultiplier = width;

        /*if (Input.GetMouseButtonDown(0) && hitEffect)
        {
            hitPsArray[1].Emit(100);
        }*/

    }

    public void OnFire(bool fire)
    {
        if (fire)
            globalProgress = 0f;
        else
            globalProgress = 1f;
    }
}
