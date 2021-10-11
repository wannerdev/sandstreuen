using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    InteractableCubeBehavior currentInteractableCube;
    // Text coordsValue;
    public GameObject generator;
    public Camera cam;

    void start(){
    }
    // Update is called once per frame
    void Update()
    {
        //DetectMouseClick();
        //DetectHandGestureTap();
        DetectHandGestureClick();
        //DetectHandGesturePinch();
        // DetectHandGestureRelease();
    }

    void DetectSandGestureIntuitiv()
    {
        

        //All the information of the hand
        HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;

        //HandInfo d;
        //d.gesture_info.hand_side;
        //mi one
        if (detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.CLICK)
        //detectedHand.gesture_info.hand_side == HandInfo. )
        {
            
            //Logic that should happen when I click
            if (currentInteractableCube)
            {
                currentInteractableCube.InteractWithCube();
            }
        }

    }

    void DetectHandGestureClick()
    {

        //All the information of the hand
        HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;

        //The click happens when I perform the Click Gesture : Open Pinch -> Closed Pinch -> Open Pinch 
        if (detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.CLICK)
        {
            
            //Logic that should happen when I click
            if (currentInteractableCube)
            {
                currentInteractableCube.InteractWithCube();
                // Vector3 point = detectedHand.tracking_info.palm_center;
                // point.z = detectedHand.tracking_info.depth_estimation;
                //0,0 bottom left of screen
                //top right is 1,1
                //depth estimation 0-1
                //create by palm position ? + scale to max 5
                //Vector3 place = new Vector3(detectedHand.tracking_info.palm_center.x*5, detectedHand.tracking_info.palm_center.y*5, Camera.main.transform.position.z*5);
                
                //create by camera position + offset for mesharea startpoint
                Vector3 place = new Vector3(cam.transform.position.x +20,cam.transform.position.y+1, Camera.main.transform.position.z+1);

                // coordsValue.text = place.ToString();
                generator.GetComponent<DualContouring3D>().add_cone(place, 5, 100);
                generator.GetComponent<DualContouring3D>().regenerateMesh();
            }
        }
        //  if(Input.GetMouseButtonDown(0)){
            
            //    Vector3 place = new Vector3(cam.transform.position.x +20,cam.transform.position.y+1, Camera.main.transform.position.z+1);
                
               // coordsValue.text = place.ToString();
        //        generator.GetComponent<DualConturing3D>().add_cone(place, 5, 100);
        //        generator.GetComponent<DualConturing3D>().regenerateMesh();
        // }

    }

    void DetectHandGesturePinch()
    {

        //All the information of the hand
        HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;

        //mi one
        if (detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.GRAB_GESTURE)
        {
            
            //Logic that should happen when I click
            if (currentInteractableCube)
            {
                currentInteractableCube.InteractWithCube();
            }
        }

    }


    void DetectHandGestureRelease()
    {

        //All the information of the hand
        HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;

        //
        if (detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.RELEASE_GESTURE)
        {
            
            //Logic that should happen when I click
            if (currentInteractableCube)
            {
                currentInteractableCube.InteractWithCube();
            }
        }

    }


    void DetectHandGestureTap()
    {

        //All the information of the hand
        HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;

        if (detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.PICK)
        {
            
            //Logic that should happen when I click
            if (currentInteractableCube)
            {
                currentInteractableCube.InteractWithCube();
            }
        }

    }

    void DetectMouseClick()
    {
    
        //The click happens when I release the left mouse buttons
        if (Input.GetMouseButtonUp(0))
        {

            //Logic that should happen when I click.

            if (currentInteractableCube)
            {
                currentInteractableCube.InteractWithCube();

            }

        }
    }

    //TODO Code Challenge: Use a smart & creative way to decide which object should be the currentInteractableCube.
    //TODO email abraham@manomotion.com with your ideas and code snipets :) 
    void FindWhichCubeShouldBetheCurrentInteractable()
    {
     //gizmo raycast   
    }
}
