using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCamara : MonoBehaviour {

    //Hecho por Edson || Canal del Youtube "Edsonxn" || SUSCRIBETE!!!
    public Transform Objetivo;
    public Vector3 PosCam;
    public float VelZoom;
    public float MinZoom;
    public float MaxZoom;
    public float VelRot = 100;
    private float ZoomActual = 1;
    private float AlturaCam=-5;
    public float AlturaCamInput = 1;
    public float Maxaltura = -10;
    public float Minaltura = -0.5f;
    private float Rotinput;

	// Use this for initialization
	void Start () {
        AlturaCam = PosCam.y;
        Screen.lockCursor = true;
        
	}
	
	// Update is called once per frame
	void Update () {
        ZoomActual -= Input.GetAxis("Mouse ScrollWheel") * VelZoom;
        AlturaCam = -Input.GetAxis("Mouse Y")*5 * Time.deltaTime;
        AlturaCamInput -= AlturaCam;
        
        AlturaCamInput = Mathf.Clamp(AlturaCamInput, Maxaltura, Minaltura);


       
        PosCam.y = AlturaCamInput;
        ZoomActual = Mathf.Clamp(ZoomActual, MinZoom, MaxZoom);

        Rotinput -= Input.GetAxis("Mouse X") * VelRot * Time.deltaTime;

		
	}

    void LateUpdate()
    {
        transform.position = Objetivo.position - PosCam*ZoomActual;
        transform.LookAt(Objetivo.position);
        transform.RotateAround(Objetivo.position, Vector3.up, -Rotinput);
    }
}
