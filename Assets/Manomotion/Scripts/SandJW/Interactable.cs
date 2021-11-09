using System;
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
    private int selected;

    private bool wave;
    private bool wave2;
    private string flipL;
    private string flipR;
    private int flag;
    private float rotX,rotY,rotZ;
    // private float palmCY;
    private bool isDown;
    void start(){
        selected =((int)sandManager.coneAngle)*10;
        rotX=0; rotY=0; rotZ = 0;
        flag=0;
        uimanager = uimanager.GetComponent<JWUIManager>();
		// ManomotionManager.OnManoMotionFrameProcessed += HandleManoMotionFrameUpdated;
        flipL = "";
        flipR = "";
        // palmCY=0;
        isDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        //DetectHandGestureTap();
        //DetectHandGestureWave();
        DetectHandGestureClick();
        DetectHandGestureClosed();
        
        DetectPalmSwitchL();
        // DetectPalmSwitchR();
        if(sandManager.isRotateSetting){
            rotate();        
        }
    }

    void rotate(){
         
        HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;
        if(ManomotionManager.Instance != null){
            Warning warning = detectedHand.warning;
            //experimental constant 
            // if( detectedHand.gesture_info.mano_class != ManoClass.NO_HAND && detectedHand.tracking_info.bounding_box.top_left.y <0.4 && detectedHand.tracking_info.bounding_box.top_left.y >0.3 ){
            //      rotX +=1;
            // };
            
            // if( detectedHand.gesture_info.mano_class != ManoClass.NO_HAND && detectedHand.tracking_info.bounding_box.top_left.y-detectedHand.tracking_info.bounding_box.height <= 0 ){
            //       rotX +=1;
            // };
            if(isDown || detectedHand.tracking_info.palm_center.y  < 0.3 ){
                isDown =true;
                rotX +=1;
            }
            // palmCY = detectedHand.tracking_info.palm_center.y;
            HighlightEdgeWarning(warning);
            //rotation            
            var rotate = new Vector3(transform.eulerAngles.x + rotX, transform.eulerAngles.y + rotY, 0);
            cam.transform.eulerAngles += rotate;
            // uimanager.depth.text ="Test"+rotate.ToString() ;
            rotX=0; rotY=0; rotZ = 0;
        }
               
    }

    

    private void DetectPalmSwitchL()
    {
        //All the information of the hand
        if(ManomotionManager.Instance != null){
            HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;
        
            // if(detectedHand.gesture_info.mano_class == ManoClass.POINTER_GESTURE){
            if (detectedHand.gesture_info.hand_side == HandSide.Palmside && flipL=="closed" )
            {
                flipL = "open";
                
                sandManager.changeMode(true);
            }else if (detectedHand.gesture_info.hand_side == HandSide.Backside && flipL=="open")
            {
                flipL = "closed";
                
                sandManager.changeMode(false );
            }
        }
    }


    void DetectHandGestureClick()
    {

        //All the information of the hand
        if(ManomotionManager.Instance != null){
            HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;
            
            // uimanager.depth.text = "Depth:"+(int)(detectedHand.tracking_info.depth_estimation*10);
                // ":"+detectedHand.gesture_info.mano_class+
                // ","+detectedHand.gesture_info.mano_gesture_trigger;
        

            //The click happens when I perform the Click Gesture : Open Pinch -> Closed Pinch -> Open Pinch 
           if (detectedHand.gesture_info.mano_class == ManoClass.PINCH_GESTURE && detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.CLICK)
            {
                
                //Logic that should happen when I click
                //0,0 bottom left of screen
                //top right is 1,1
                //depth estimation 0-1

                // Create by camera position 
                // Vector3 place = cam.transform.position;
                // place += Vector3.forward;
                
                // Palm center
                // Vector3 point = detectedHand.tracking_info.palm_center;
                // point.z = detectedHand.tracking_info.depth_estimation;
                

                //create by position of interest (between fingers of click gesture)
                Vector3 place = detectedHand.tracking_info.poi;
                //place.z = detectedHand.tracking_info.depth_estimation*9;
                //place = cam.transform.position+cam.ScreenToWorldPoint(place);//Adapt placing coords to orientation
                
                place = ManoUtils.Instance.CalculateWorldPosition(place,detectedHand.tracking_info.depth_estimation*10);
                
                //place += cam.transporm.position;
                // place = cam.transform.forward * detectedHand.tracking_info.depth_estimation*5;
                // float state = detectedHand.gesture_info.state; //needs probably a better variable
                // float angle = 0.5f;//+anglechange/10; *(state/12)
                float height = 3;
                //divide by 10 to switch slow
                sandManager.add(place,selected/10,height);          
            }
        }

    }

    //Change material
    void DetectHandGestureClosed()
    {
        if(ManomotionManager.Instance != null){
            //All the information of the hand
            HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;
            if (detectedHand.gesture_info.mano_gesture_continuous == ManoGestureContinuous.CLOSED_HAND_GESTURE )
            {
                // flag=1;
                //Logic that should happen when I click
                selected +=1;
                if (selected ==40)selected=0;
                sandManager.changeMaterial(selected/10);
            }
        }
    }

    void HighlightEdgeWarning(Warning warning)
	 {
	 	switch (warning)
	 	{

	 		case Warning.WARNING_APPROACHING_LEFT_EDGE:
                rotY -= 2f;
                 wave = true;isDown =false;
	 			break;

	 		case Warning.WARNING_APPROACHING_RIGHT_EDGE:
                rotY += 2f;
                 wave2 =true;
                 isDown =false;
	 			break;
	 		case Warning.WARNING_APPROACHING_UPPER_EDGE:
	 			rotX +=-1;
                 isDown =false;
                 break;

	 		default:
	 			break;
        }
        //uimanager.depth.text ="WARN" ;
	 }

    //If we had a Pro License we could use the information which hand
    //  private void DetectPalmSwitchR()
    // {
    //     //All the information of the hand
    //     if(ManomotionManager.Instance != null){
    //         HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;
    //         //The click happens when I perform the Click Gesture : Open Pinch -> Closed Pinch -> Open Pinch 
    //         // if(detectedHand.gesture_info.mano_class == ManoClass.POINTER_GESTURE){
    //         if (detectedHand.gesture_info.mano_gesture_continuous == ManoGestureContinuous.OPEN_HAND_GESTURE && flipR=="closed" &&detectedHand.hand == Hand.RIGHT)
    //         {
    //             flipR = "open";
    //             sandManager.changeMode(false);
    //         }else if (detectedHand.gesture_info.mano_gesture_continuous == ManoGestureContinuous.CLOSED_HAND_GESTURE && flipR=="open" &&detectedHand.hand == Hand.RIGHT)
    //         {
    //             flipR = "closed";    
    //             sandManager.changeMode(true);
    //         }
    //     }
    // }

    // void DetectHandGestureGrab()
    // {
    //     if(ManomotionManager.Instance != null){
    //         //All the information of the hand
    //         HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;

    //         if (detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.GRAB_GESTURE)
    //         {
    //                 sandManager.changeMode(true);
    //         }
    //     }

    // }

    // void DetectHandGestureWave()
    // {
    //     //All the information of the hand
    //     HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;
    //     
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

//  void DetectHandGestureRelease()
    //  {
    //      //All the information of the hand
    //      HandInfo detectedHand = ManomotionManager.Instance.Hand_infos[0].hand_info;
    // //     //
    //      if (detectedHand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.RELEASE_GESTURE)
    //      {
    //      }
    //  }

	/// <summary>
	/// Handles the information from the processed frame in order to use the warning information to highlight the user position approaching to the edges.
	/// </summary>

	/// <summary>
	/// Visually illustrated the users hand approaching the edges of the screen
	/// </summary>
	// /// <param name="warning"></param>    
}
