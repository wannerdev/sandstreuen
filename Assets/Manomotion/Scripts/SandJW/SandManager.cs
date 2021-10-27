using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandManager : MonoBehaviour
{
    
    // JWUIManager uimanager = uimanager.GetComponent<JWUIManager>();
    public DualContouring3D dc3D;
    public JWUIManager uimanager;
    public modes mode;
    public Bodies.types coneAngle;// select;
    public enum modes{CONE, SINGLE, REMOVE};

    void Start()
    {
         mode= modes.CONE;
         uimanager.changeMode("cone");
         coneAngle=Bodies.types.SANDDRY;
         dc3D = dc3D.GetComponent<DualContouring3D>();
    }

    public void add(Vector3 place,  int selected,float height=5 )
    {
        if( mode == modes.CONE){
            if(dc3D.add_cone(place, height, selected)){
                dc3D.regenerateMesh();
                uimanager.resetWarning();
                uimanager.conePosition(place);
            }else{
                uimanager.warnOutside();
            }
        }else if( mode == modes.REMOVE){
            if(dc3D.add_single(place,selected,true)){
                dc3D.regenerateMesh();
                uimanager.resetWarning();
                uimanager.conePosition(place);
            }else{
                uimanager.warnOutside();
            }
        }else if( mode == modes.SINGLE){
            if(dc3D.add_single(place, selected)){
                dc3D.regenerateMesh();
                uimanager.resetWarning();
                
                // place.x +=Math.Abs(d3D.offset.x);
                // place.y +=Math.Abs(d3D.offset.y);
                // place.z +=Math.Abs(d3D.offset.z);
                uimanager.conePosition(place);
            }else{
                uimanager.warnOutside();
            }
        }
    }


    public void changeMode(bool plus){
        if(plus){
        this.mode = this.mode+1;
        }
        else{
            this.mode = this.mode-1;
        }
        if(mode<0)mode=modes.REMOVE;
        if(((int)mode)>2)mode=modes.CONE;

        uimanager.changeMode(mode.ToString());
    }
    public void changeMaterial(int selected){
        uimanager.changeMaterial(selected);
    }
}