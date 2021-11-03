using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileControl : MonoBehaviour {
    private Vector3 touchStart,touchStart2;
    public Camera cam;
    public Joystick moveJoystick;
    public Joystick rotateJoystick;
    public float groundZ = 0;
    public float rotX=0, rotY=0, rotZ = 0, rotW=0;

    void Start(){
         //Check if the device running this is a handheld
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            FixedJoystick[] k = this.GetComponentsInChildren<FixedJoystick>();
            if(k !=null && k.Length>0){
                // popUp1 = GameObject.FindWithTag (tagName);
                //Move UI out of camera, disabling doesn't work
                Vector3 currentPosition = k[0].transform.localPosition;
                k[0].transform.localPosition = new Vector3 (1000, 1000);
                Vector3 currentPosition2 = k[1].transform.localPosition;
                k[1].transform.localPosition = new Vector3 (1000, 1000);
            }
            this.moveJoystick.enabled = false;
            this.rotateJoystick.enabled = false;
            this.enabled = false;
        }else{
            
            this.enabled = true;
            this.moveJoystick.enabled = true;
            this.rotateJoystick.enabled = true;
        }
    }
	// Update is called once per frame
	void Update () {
        Vector3 direction= new Vector3(0,0,0);
        rotX=0; rotY=0; rotZ = 0; rotW=0;
        if (moveJoystick.Horizontal >= .2f)
        {
            direction = cam.transform.right;
            // direction = new Vector3(0.1f, 0, 0);
        }
        else if (moveJoystick.Horizontal <= -.2f)
        {
            direction = -cam.transform.right;
            // direction = new Vector3(-0.1f, 0, 0);
        }
 
        float verticalMove = moveJoystick.Vertical;
 
        if (verticalMove >= .2f ) {
            direction = cam.transform.forward;//new Vector3(0, 0, 0.1f);

        }else if(verticalMove <= -.2f){

            direction = -cam.transform.forward;//new Vector3(0, 0,-0.1f);
        }

        //rotation
        if (rotateJoystick.Horizontal >= .2f)
        {
            rotY += 1f;
        }
        else if (rotateJoystick.Horizontal <= -.2f)
        {
            rotY += -1f;
        }
 
        verticalMove = rotateJoystick.Vertical;
 
        if (verticalMove >= .2f ) {
            rotX += 1f;

        }else if(verticalMove <= -.2f){

            rotX += -1f;
        }
        direction = direction*0.3f;
        //direction = cam.transform.TransformDirection(direction)*0.3f;
         //cam.transform.Rotate(rotX,rotY,rotZ,Space.Self);
        var rotate = new Vector3(transform.eulerAngles.x + rotX, transform.eulerAngles.y + rotY, 0);
        transform.eulerAngles = rotate;
        cam.transform.position += direction;
        // cam.transform.localRotation = Quaternion.Euler(rotX, rotY, 0);
        //cam.transform.rotation = quaternion;
        // if (Input.GetMouseButtonDown(0)){
        //     touchStart = GetWorldPosition(groundZ);
        // }
        // if (Input.GetMouseButton(0)){
        //     Vector3 direction = touchStart - GetWorldPosition(groundZ);
        //     cam.transform.position += direction;
        // }


        // Quaternion quaternion = new Quaternion(
        //              rotX,
        //              rotY,
        //              rotZ,
        //              cam.transform.rotation.w);
        
        // if (Input.GetMouseButtonDown(1)){
        //     touchStart2 = GetWorldPosition(groundZ);
        // }
        // if (Input.GetMouseButton(1)){
        //     Vector3 direction = touchStart2 - GetWorldPosition(groundZ);
        //     cam.transform.eulerAngles = transform.eulerAngles - new Vector3(0,2,0);
        // }
    }
}