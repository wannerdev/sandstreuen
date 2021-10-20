using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectivePan : MonoBehaviour {
    private Vector3 touchStart,touchStart2;
    public Camera cam;
    public float groundZ = 0;

    void Start(){
         //Check if the device running this is a handheld
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            this.enabled = false;
        }
    }
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0)){
            touchStart = GetWorldPosition(groundZ);
        }
        if (Input.GetMouseButton(0)){
            Vector3 direction = touchStart - GetWorldPosition(groundZ);
            cam.transform.position += direction;
        }

        
        if (Input.GetMouseButtonDown(1)){
            touchStart2 = GetWorldPosition(groundZ);
        }
        if (Input.GetMouseButton(1)){
            Vector3 direction = touchStart2 - GetWorldPosition(groundZ);
            cam.transform.eulerAngles = transform.eulerAngles - new Vector3(0,2,0);
        }
    }
    private Vector3 GetWorldPosition(float z){
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.forward, new Vector3(0,0,z));
        float distance;
        ground.Raycast(mousePos, out distance);
        return mousePos.GetPoint(distance);
    }
}