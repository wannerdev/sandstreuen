using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JWUIManager : MonoBehaviour
{
    [SerializeField]
    public Text ConeValueText, CameraValue,depth,warning,sand,mode; 
    
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        
        cam =  this.GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // var size =  generator.GetComponent<DualContouring3D>().cones.Count;
        // ConeValueText.text = "X: "+generator.GetComponent<DualContouring3D>().cones[size-1].position[0];
        // ConeValueText.text += "Y: "+generator.GetComponent<DualContouring3D>().cones[size-1].position[1];
        // ConeValueText.text += "Z: "+generator.GetComponent<DualContouring3D>().cones[size-1].position[2];
        // ConeValueText.text
        
        CameraValue.text = "Camera Position X:"+cam.transform.position.ToString();
        // CameraValue.text += " Y:"+cam.transform.position.y;//ToString();
        // CameraValue.text += " Z:"+cam.transform.position.z;//ToString();
        // CameraValue.text = "Y: "+GetComponent<DualContouring3D>().cones[size-1].bot[1];
        // CameraValue.text = "Z: "+GetComponent<DualContouring3D>().cones[size-1].bot[2];
    }

    internal void resetWarning()
    {
        warning.text = "";
    }

    internal void changeMaterial(int selected)
    {
        sand.text = "Material:"+Bodies.sandname[selected];
    }

    internal void conePosition(Vector3 place)
    {
        ConeValueText.text = place.ToString();
    }
    
    internal void changeMode(String modeVal)
    {
        mode.text = "Mode: "+modeVal;
        
    }

    internal void warnOutside()
    {
        warning.text = "Outside of Area";
    }
}
