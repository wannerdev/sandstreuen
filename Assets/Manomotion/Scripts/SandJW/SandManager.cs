using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandManager : MonoBehaviour
{
    
    // JWUIManager uimanager = uimanager.GetComponent<JWUIManager>();
    public DualContouring3D d3D;
    public JWUIManager uimanager;
    public string modeVal;
    public Bodies.types coneAngle;// select;

    void Start()
    {
         modeVal="cone";
         uimanager.changeMode("cone");
         coneAngle=Bodies.types.SANDDRY;
        // uimanager = 
        d3D = d3D.GetComponent<DualContouring3D>();
    }

    public void add(Vector3 place, float height, int selected )
    {
        if( modeVal == "cone"){
            if(d3D.add_cone(place, height, selected)){
                d3D.regenerateMesh();
                uimanager.resetWarning();
                uimanager.conePosition(place);
            }else{
                uimanager.warnOutside();
            }
        }else{
            if(d3D.add_single(place, selected)){
                d3D.regenerateMesh();
                uimanager.resetWarning();
                uimanager.conePosition(place);
            }else{
                uimanager.warnOutside();
            }
        }
    }


    public void changeMode(string mode){
        this.modeVal = mode;
        uimanager.changeMode(mode);
    }
    public void changeMaterial(int selected){
        uimanager.changeMaterial(selected);
    }
}