using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGunLine : MonoBehaviour
{
    public WaterGunData waterGunData;
    //public float maxLength = 16.0f;
    //public float spherecastRadius = 0.5f;
    public GameObject hitEffect;
    public Renderer meshRenderer1;
    //public Renderer meshRenderer2;
    public ParticleSystem[] hitPsArray;
    public int segmentCount = 32;
    public float globalProgressSpeed { get { return 1f / waterGunData.ShootMaxCD; } }
    public AnimationCurve shaderProgressCurve;
    public AnimationCurve lineWidthCurve;


    private LineRenderer _lr;
    private Vector3[] _resultVectors;
    private float _dist;
    private float _globalProgress;
    private Vector3 _hitPosition;
    private Vector3 _currentPosition;
    private float _maxLength;
    private float _spherecastRadius;

    void Start()
    {
        _globalProgress = 1f;
        _lr = this.GetComponent<LineRenderer>();
        _lr.positionCount = segmentCount;
        _resultVectors = new Vector3[segmentCount + 1];
        for (int i = 0; i < segmentCount + 1; i++)
        {
            _resultVectors[i] = transform.forward;
        }

        if (waterGunData)
        {
            _spherecastRadius = waterGunData.WaterCastRadius;
            _maxLength = waterGunData.WaterCastDistance +_spherecastRadius;
        }
        else
        {
            _maxLength = 5f;
            _spherecastRadius = 0.4f;
        }

    }

    void Update()
    {


        //Move LineRenderer One By One

        for (int i = segmentCount - 1; i > 0; i--)
        {
            _resultVectors[i] = _resultVectors[i - 1];
        }

        _resultVectors[0] = transform.forward;
        _resultVectors[segmentCount] = _resultVectors[segmentCount - 1];
        float blockLength = _maxLength / segmentCount;


        _currentPosition = new Vector3(0, 0, 0);

        for (int i = 0; i < segmentCount; i++)
        {
            _currentPosition = transform.position;
            for (int j = 0; j < i; j++)
            {
                _currentPosition += _resultVectors[j] * blockLength;
            }

            _lr.SetPosition(i, _currentPosition);
        }





        //Collision 

        for (int i = 0; i < segmentCount; i++)
        {

            _currentPosition = transform.position;
            for (int j = 0; j < i; j++)
            {
                _currentPosition += _resultVectors[j] * blockLength;
            }
            RaycastHit hit;

            if (i == segmentCount - 1)
            {
                _hitPosition = _currentPosition;

                if (hitEffect)
                {
                    hitEffect.transform.position = _hitPosition;
                    // hitEffect.transform.rotation = Quaternion.FromToRotation(hitEffect.transform.forward, resultVectors[i].normalized);

                }

                _dist = Vector3.Distance(_hitPosition, transform.position);

            }
            else if (Physics.SphereCast(_currentPosition, _spherecastRadius, _resultVectors[i], out hit, blockLength, waterGunData.WaterCanHitLayer))
            {
                _hitPosition = _currentPosition + _resultVectors[i] * hit.distance;
                _hitPosition = Vector3.MoveTowards(_hitPosition, _hitPosition + _resultVectors[i] * blockLength, 0.3f);
                //hitPosition = Vector3.MoveTowards(hitPosition, transform.position, 0.5f);
                if (hitEffect)
                {
                    hitEffect.transform.position = _hitPosition;

                }

                _dist = Vector3.Distance(_hitPosition, transform.position);

                break;
            }
        }



        //Emit Hit Particles 

        if (hitEffect)
        {
            hitEffect.transform.rotation = Quaternion.FromToRotation(Vector3.forward, _resultVectors[segmentCount - 1].normalized);
            if (_globalProgress < 0.75f)
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

        GetComponent<Renderer>().material.SetFloat("_Distance", _dist);
        GetComponent<Renderer>().material.SetVector("_Position", transform.position);

        // if (Input.GetMouseButton(0))
        // {
        //     globalProgress = 0f;
        // }

        if (_globalProgress <= 1f)
        {
            _globalProgress += Time.deltaTime * globalProgressSpeed;
        }


        float progress = shaderProgressCurve.Evaluate(_globalProgress);
        GetComponent<Renderer>().material.SetFloat("_Progress", progress);

        if (meshRenderer1 != null)
        {
            meshRenderer1.material.SetFloat("_Progress", progress);
        }

        float width = lineWidthCurve.Evaluate(_globalProgress);
        _lr.widthMultiplier = width;

        /*if (Input.GetMouseButtonDown(0) && hitEffect)
        {
            hitPsArray[1].Emit(100);
        }*/

    }

    public void OnFire(bool fire)
    {
        if (fire)
            _globalProgress = 0f;
        else
            _globalProgress = 1f;
    }

}
