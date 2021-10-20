using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectivePan : MonoBehaviour {
    private Vector3 touchStart,touchStart2;
    public Camera cam;
    public Joystick moveJoystick;
    public Joystick rotateJoystick;
    public float groundZ = 0;
    public float rotX=0, rotY=0, rotZ = 0;

    void Start(){
         //Check if the device running this is a handheld
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            this.enabled = false;
        }
    }
	// Update is called once per frame
	void Update () {
        Vector3 direction= new Vector3(0,0,0);

        if (moveJoystick.Horizontal >= .2f)
        {
            direction = new Vector3(0.1f, 0, 0);
        }
        else if (moveJoystick.Horizontal <= -.2f)
        {
            direction = new Vector3(-0.1f, 0, 0);
        }
 
        float verticalMove = moveJoystick.Vertical;
 
        if (verticalMove >= .2f ) {
            direction = new Vector3(0, 0, 0.1f);

        }else if(verticalMove <= -.2f){

            direction = new Vector3(0, 0,-0.1f);
        }

        //rotation
        if (rotateJoystick.Horizontal >= .2f)
        {
            rotX += 0.0051f;
        }
        else if (rotateJoystick.Horizontal <= -.2f)
        {
            rotX += -0.0051f;
        }
 
        verticalMove = rotateJoystick.Vertical;
 
        if (verticalMove >= .2f ) {
            rotY += 0.0051f;

        }else if(verticalMove <= -.2f){

            rotY += -0.0051f;
        }
        Quaternion quaternion = new Quaternion(
        cam.transform.rotation.x+ rotX,cam.transform.rotation.y+rotY,cam.transform.rotation.z+rotZ,
        cam.transform.rotation.w);
        cam.transform.position += direction;
        cam.transform.rotation = quaternion;
        // if (Input.GetMouseButtonDown(0)){
        //     touchStart = GetWorldPosition(groundZ);
        // }
        // if (Input.GetMouseButton(0)){
        //     Vector3 direction = touchStart - GetWorldPosition(groundZ);
        //     cam.transform.position += direction;
        // }

        
        // if (Input.GetMouseButtonDown(1)){
        //     touchStart2 = GetWorldPosition(groundZ);
        // }
        // if (Input.GetMouseButton(1)){
        //     Vector3 direction = touchStart2 - GetWorldPosition(groundZ);
        //     cam.transform.eulerAngles = transform.eulerAngles - new Vector3(0,2,0);
        // }
    }
    private Vector3 GetWorldPosition(float z){
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.forward, new Vector3(0,0,z));
        float distance;
        ground.Raycast(mousePos, out distance);
        return mousePos.GetPoint(distance);
    }
}