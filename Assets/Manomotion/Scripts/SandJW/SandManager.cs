using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandManager : MonoBehaviour
{
    
    public enum modes{CONE, SINGLE, REMOVE,GROW};
    public modes mode;
    public DualContouring3D dc3D;
    public JWUIManager uimanager;
    public Bodies.types coneAngle;// select;
    public bool isRotateSetting;
    private Vector3 placeCache;
    private float cacheheight;

    void Start()
    {
        // isRotateSetting = false;
        dc3D = dc3D.GetComponent<DualContouring3D>();
        if(dc3D.scale != 1){
            mode =modes.SINGLE;
            uimanager.changeMode("single");
        }else{
            mode= modes.CONE;
            uimanager.changeMode("cone");
        }
        cacheheight=5; //used for grow
        coneAngle=Bodies.types.SANDDRY;
    }

    public void add(Vector3 place,  int selected,float height=5 )
    {
        bool isAdded=false;
        switch(mode){
            case modes.CONE:
                isAdded = dc3D.add_cone(place, height, selected);
                break;
                
            case modes.REMOVE:
                isAdded = dc3D.add_single(place,selected,true);
                break;
                
            case modes.GROW:
                if(place == placeCache){
                    cacheheight+=0.5f;
                }else{
                    cacheheight=height;
                }
                placeCache = place;

                place.y=cacheheight;
                isAdded = dc3D.add_cone(place,cacheheight,selected);
                break;

            case modes.SINGLE:
                isAdded = dc3D.add_single(place, selected);
                break;
        }
        if(isAdded){
            dc3D.regenerateMesh();
            uimanager.resetWarning();
            uimanager.conePosition(place);
        }else{
            uimanager.warnOutside();
        }
    }

    public void changeRotating(){
        isRotateSetting = !isRotateSetting;
    }

    public void changeMode(bool plus){
        if(dc3D.scale !=1){ //Disable cone mode if scaled
            mode = modes.REMOVE == mode ? modes.SINGLE : modes.REMOVE; 
        }else{
            if(plus){
            this.mode = this.mode+1;
            }
            else{
                this.mode = this.mode-1;
            }
            if(mode<0)mode = modes.GROW;
            if(((int)mode)>3)mode = modes.CONE;
        }
        uimanager.changeMode(mode.ToString());
        
    }
    public void changeMaterial(int selected){
        uimanager.changeMaterial(selected);
    }
}