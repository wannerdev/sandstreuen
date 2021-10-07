using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    InteractableCubeBehavior currentInteractableCube;
    DualConturing3D generator;


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
                Vector3 point = detectedHand.tracking_info.palm_center;
                point.z = detectedHand.tracking_info.depth_estimation;
                generator.add_cone(point, 5, 100);
            }
        }

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
