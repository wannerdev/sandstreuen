using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JWUIManager : MonoBehaviour
{
    [SerializeField]
    Text ConeValueText, CameraValue; 
    public GameObject generator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var size =  generator.GetComponent<DualContouring3D>().cones.Count;
        ConeValueText.text = "X: "+generator.GetComponent<DualContouring3D>().cones[size-1].position[0];
        ConeValueText.text += "Y: "+generator.GetComponent<DualContouring3D>().cones[size-1].position[1];
        ConeValueText.text += "Z: "+generator.GetComponent<DualContouring3D>().cones[size-1].position[2];

        
        var cam =  this.GetComponentInParent<Camera>();
        CameraValue.text = "Camera Position X:"+cam.transform.position.x;//ToString();
        CameraValue.text += " Y:"+cam.transform.position.y;//ToString();
        CameraValue.text += " Z:"+cam.transform.position.z;//ToString();
        // CameraValue.text = "Y: "+GetComponent<DualContouring3D>().cones[size-1].bot[1];
        // CameraValue.text = "Z: "+GetComponent<DualContouring3D>().cones[size-1].bot[2];
    }
}
