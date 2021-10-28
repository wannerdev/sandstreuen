using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    /*
   Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
   Converted to C# 27-02-13 - no credit wanted.
   Simple flycam I made, since I couldn't find any others made public.  
   Made simple to use (drag and drop, done) for regular keyboard layout  
   wasd : basic movement
   shift : Makes camera accelerate
   space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/

    // Docboy add vertical move and mouse button dependant 2020

    //public Camera cam;
    public float mainSpeed = 100.0f; //regular speed
    public float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    public float maxShift = 1000.0f; //Maximum speed when holdin gshift
    public float camSens = 0.25f; //How sensitive it with mouse
    public Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;

    public SandManager sandManager;
    private Camera cam;
    private int selected=0;

    private void Start()
    {
        //Check if the device running this is a handheld
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            this.enabled = false;
        }
        // GameObject[] unused = GameObject.FindGameObjectsWithTag("desktop");
        // foreach(GameObject u in unused ){
        //     u.gameObject.SetActive(false);
        // }
        lastMouse = Input.mousePosition;
        this.cam = this.GetComponent<Camera>();
        sandManager = sandManager.GetComponent<SandManager>();
    }
    void Update()
    {
        if (Input.GetMouseButton(0)){
            Vector3 place = cam.transform.position;
            place += cam.transform.forward;
            sandManager.add(place, selected);
        }
        if (Input.GetMouseButton(1)){
            selected +=1;
            if (selected ==4)selected=0;
            sandManager.changeMaterial(selected);
        }

        if (Input.GetKey(KeyCode.UpArrow)){
            selected +=1;
            if (selected ==4)selected=0;
            sandManager.changeMode("single");
        }
        
        if (Input.GetKey(KeyCode.DownArrow)){
            selected +=1;
            if (selected ==4)selected=0;
            sandManager.changeMode("cone");
        }
        //Move cones old object oriented way
        // for(int i=0; i < generator.GetComponent<DualContouring3D>().cones.Count;i++){
        //     if (Input.GetKey(KeyCode.LeftArrow))
        //     {
        //         generator.GetComponent<DualContouring3D>().cones[i].position[0] = -3 + generator.GetComponent<DualContouring3D>().cones[i].position[0];
        //         //transform.position += Vector3.left * speed * Time.deltaTime;
        //      generator.GetComponent<DualContouring3D>().regenerateMesh();
        //     }
        //     if (Input.GetKey(KeyCode.RightArrow))
        //     {
        //         generator.GetComponent<DualContouring3D>().cones[i].position[0] =3   + generator.GetComponent<DualContouring3D>().cones[i].position[0];
        //      generator.GetComponent<DualContouring3D>().regenerateMesh();
        //     }
        //     if (Input.GetKey(KeyCode.UpArrow))
        //     {
        //         generator.GetComponent<DualContouring3D>().cones[i].position[2] = 3  + generator.GetComponent<DualContouring3D>().cones[i].position[2];
        //      generator.GetComponent<DualContouring3D>().regenerateMesh();
        //     }
        //     if (Input.GetKey(KeyCode.DownArrow))
        //     {
        //         generator.GetComponent<DualContouring3D>().cones[i].position[2] = -3 + generator.GetComponent<DualContouring3D>().cones[i].position[2];
        //      generator.GetComponent<DualContouring3D>().regenerateMesh();
        //     }
        // }
        DoCalculation();
        lastMouse = Input.mousePosition;

    }

    private Vector3 GetBaseInput()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            p_Velocity += new Vector3(0, -1, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            p_Velocity += new Vector3(0, 1, 0);
        }
        return p_Velocity;
    }


    public void DoCalculation()
    {
        lastMouse = Input.mousePosition - lastMouse;
        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
        transform.eulerAngles = lastMouse;
        lastMouse = Input.mousePosition;
        //Mouse  camera angle done.  

        //Keyboard commands
        Vector3 p = GetBaseInput();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            totalRun += Time.deltaTime;
            p = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * mainSpeed;
        }

        p = p * Time.deltaTime;
        Vector3 newPosition = transform.position;
        if (Input.GetKey(KeyCode.Space))
        { //If player wants to move on X and Z axis only
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            transform.Translate(p);
        }
    }
}