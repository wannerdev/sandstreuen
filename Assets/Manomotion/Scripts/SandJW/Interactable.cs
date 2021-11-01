using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using generator = DualContouring3D;

public class Interactable : MonoBehaviour
{
    public SandManager sandManager;
    public Camera cam;
    public JWUIManager uimanager;
    private int selected=0;

    private bool wave;
    private bool wave2;
    private int flag=0;

    void start(){
        selected =((int)sandManager.coneAngle)*10;
        
        uimanager = uimanager.GetComponent<JWUIManager>();
		// ManomotionManager.OnManoMotionFrameProcessed += HandleManoMotionFrameUpdated;
    }
    // Update is called once per frame
    void Update()
    {
        //DetectMouseClick();
        DetectHandGestureTap();
        DetectHandGestureClick();
        // DetectHandGestureWave();
        //DetectHandGesturePinch();
        // DetectHandGestureRelease();
    }


    void DetectHandGestureClick()
    {

        //All the information of the hand
        if(ManomotionManager.Instance != null){
            HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;
            
            uimanager.depth.text = "depth:"+detectedHand.tracking_info.depth_estimation+" ";
        

            //The click happens when I perform the Click Gesture : Open Pinch -> Closed Pinch -> Open Pinch 
            if (detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.CLICK)
            {
                
                //Logic that should happen when I click
                // Vector3 point = detectedHand.tracking_info.palm_center;
                // point.z = detectedHand.tracking_info.depth_estimation;
                //0,0 bottom left of screen
                //top right is 1,1
                //depth estimation 0-1
                //create by palm position ? + scale to max 2
                //Vector3 place = new Vector3(detectedHand.tracking_info.palm_center.x*5, detectedHand.tracking_info.palm_center.y*5, Camera.main.transform.position.z*5);
                
                //create by camera position 
                //Vector3 place = cam.transform.position;
                //place += Vector3.forward;
                
                //create by palm position 
                Vector3 place = detectedHand.tracking_info.palm_center;
                // place.z = detectedHand.tracking_info.depth_estimation*9;
                //place = cam.transform.position+cam.ScreenToWorldPoint(place);//Adapt placing coords to orientation
                
                place = ManoUtils.Instance.CalculateWorldPosition(place,detectedHand.tracking_info.depth_estimation*10);
                
                //place += cam.transporm.position;
                // place = cam.transform.forward * detectedHand.tracking_info.depth_estimation*5;
                float state = detectedHand.gesture_info.state; //needs probably a better variable
                // float angle = 0.5f;//+anglechange/10; *(state/12)
                float height = 3;
                //divide by 10 to switch slow
                sandManager.add(place,selected/10,height);          
            }
        }

    }


    void DetectHandGestureTap()
    {
        if(ManomotionManager.Instance != null){
            //All the information of the hand
            HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;

            if (detectedHand.gesture_info.mano_gesture_continuous == ManoGestureContinuous.POINTER_GESTURE)
            {
                // flag=1;
                //Logic that should happen when I click
            
                selected +=1;
                if (selected ==40)selected=0;
                uimanager.changeMaterial(selected/10);
            } 
        }

    }

    // void DetectHandGesturePinch()
    // {

    //     //All the information of the hand
    //     HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;

    //     //mi one
    //     if (detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.GRAB_GESTURE)
    //     {
            
    //         //Logic that should happen when I click
    //         if (currentInteractableCube)
    //         {
    //             currentInteractableCube.InteractWithCube();
    //         }
    //     }

    // }
    // void DetectHandGestureWave()
    // {

    //     //All the information of the hand
    //     HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;

    //     //
    //     if (detectedHand.gesture_info.mano_gesture_continuous == ManoGestureContinuous.OPEN_HAND_GESTURE)
    //     {            
    //         //Logic that should happen when I click
    //         if (wave&&wave2)
    //         {
    //             selected +=1;
    //             if (selected ==4)selected=0;
    //             uimanager.changeMaterial(selected);
    //             //improve timer based reset?
    //             wave=false;
    //             wave2=false;
    //         }
    //     }


    // }


	/// <summary>
	/// Handles the information from the processed frame in order to use the warning information to highlight the user position approaching to the edges.
	/// </summary>
	void HandleManoMotionFrameUpdated()
	{
		Warning warning = ManomotionManager.Instance.Hand_infos[0].hand_info.warning;

	 	HighlightEdgeWarning(warning);
    }

	/// <summary>
	/// Visually illustrated the users hand approaching the edges of the screen
	/// </summary>
	// /// <param name="warning"></param>
	 void HighlightEdgeWarning(Warning warning)
	 {
	 	switch (warning)
	 	{

	 		case Warning.WARNING_APPROACHING_LEFT_EDGE:
                 wave = true;
	 			break;

	 		case Warning.WARNING_APPROACHING_RIGHT_EDGE:
                 wave2 =true;
	 			break;
	 		case Warning.WARNING_APPROACHING_UPPER_EDGE:
	 			break;


	 		default:
	 			break;
         }
	 }

    // void DetectHandGestureRelease()
    // {

    //     //All the information of the hand
    //     HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;

    //     //
    //     if (detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.RELEASE_GESTURE)
    //     {
            
    //         //Logic that should happen when I click
    //         if (currentInteractableCube)
    //         {
    //             currentInteractableCube.InteractWithCube();
    //         }
    //     }

    // }



    // void DetectMouseClick()
    // {
    
    //     //The click happens when I release the left mouse buttons
    //     if (Input.GetMouseButtonUp(0))
    //     {

    //         //Logic that should happen when I click.

    //         if (currentInteractableCube)
    //         {
    //             currentInteractableCube.InteractWithCube();

    //         }

    //     }
    // }
}
