using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualConturing3D : MonoBehaviour
{
    public const int areaSize = 40;
    public bool adaptive = true;
    // Start is called before the first frame update
    public List<Vector3> verticies = new List<Vector3>();
    public Vector3[,,] vertexe = new Vector3[areaSize + 1, areaSize + 1, areaSize + 1];
    public List<int> indicies = new List<int>();
    public Mesh mesh;
    public MeshFilter filter;
    public List<Cone> cones;

    void Start()
    {

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        meshRenderer.material.SetColor("_Color", Color.red);
        //meshRenderer.
        mesh = new Mesh();
        cones = new List<Cone>();
        Vector3 point=new Vector3(0,0,0);
        add_cone(point, 10, 100);

        filter = gameObject.AddComponent<MeshFilter>();
        var mesh2 = new Mesh();
        var adaptedV = new List<Vector3>();

        var faces = new List<Vector3>();

        Vector3[,,] vertexe = new Vector3[areaSize + 1, areaSize + 1, areaSize + 1];

        for (int x = 0; x <= areaSize; x++)
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++)
                {
                    vertexe[x, y, z] = find_vertex(x, y, z, new Vector3(0, 0, 0));
                    if (vertexe[x, y, z] == null)
                    {
                        continue;
                    }
                }
            }
        }

        // If one point is inside and not all points are inside place vertex -
        // another way to phrase it is just show the edges where a sign switch happens
        for (int x = 0; x <= areaSize; x++) //start a bit to the right to have neighbors
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++)
                {
                    if (x > 0 && y > 0)//respect area boundries
                    { 
                        //check edge for sign change
                        bool solid1 = isInside(x, y, z + 0);
                        bool solid2 = isInside(x, y, z + 1);

                        if (solid1 ^ solid2)
                        {
                            // if()
                            if (!solid2)
                            {
                                verticies.Add(vertexe[x - 1, y - 1, z]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x - 0, y - 1, z]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x - 1, y, z]);
                                indicies.Add(verticies.Count - 1);
                            }
                            else
                            {
                                verticies.Add(vertexe[x - 1, y, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x - 0, y - 1, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x - 1, y - 1, z]);
                                indicies.Add(verticies.Count - 1);

                            }
                        }
                    }

                    if (x > 0 && z > 0)//respect area boundries
                    { 
                        //check edge for sign change
                        bool solid1 = isInside(x, y + 0, z);
                        bool solid2 = isInside(x, y + 1, z);

                        if (solid1 ^ solid2)
                        {
                            if (solid2)
                            {
                                verticies.Add(vertexe[x - 1, y, z - 1]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y, z - 1]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x - 1, y, z]);
                                indicies.Add(verticies.Count - 1);
                            }
                            else
                            {

                                verticies.Add(vertexe[x - 1, y, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y, z - 1]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x - 1, y, z - 1]);
                                indicies.Add(verticies.Count - 1);
                            }
                        }
                    }

                    if (y > 0 && z > 0)//respect area boundries
                    { 
                        //check edges for sign change
                        bool solid1 = isInside(x, y, z);
                        bool solid2 = isInside(x + 1, y, z);

                        if (solid1 ^ solid2)
                        {

                            if (!solid2)
                            {
                                verticies.Add(vertexe[x, y - 1, z - 1]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y, z - 1]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y - 1, z]);
                                indicies.Add(verticies.Count - 1);
                            }
                            else
                            {

                                verticies.Add(vertexe[x, y - 1, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y, z - 1]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y - 1, z - 1]);
                                indicies.Add(verticies.Count - 1);
                            }
                        }
                    }
                }
            }
        }
        mesh.vertices = verticies.ToArray();
        Vector3[] vertices = mesh.vertices;
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Quads, 0);

        filter.mesh = mesh;
    }

    private Vector3 find_vertex(float x, float y, float z, Vector3 normal)
    {
        if(adaptive){
            return new Vector3(x + 0.5f, y + 0.5f, z+0.5f);
        }
        //Vector3 vec =  new Vector3(x, y, z);
        float [,,]edges = new float[,,];
        for(int j =0; j < 2;j++){
            for(int k =0; k < 2;k++){
                for(int l =0; l < 2;l++){
                    edges[j,k,l] = isInside(x+j,y+k,+l);
                }
            }
        }

        return vec;
    }

private float adapt(float  v0, float v1){
    //v0 and v1 are numbers of opposite sign. This returns how far you need to interpolate from v0 to v1 to get to 0
    //assert((v1 > 0) != (v0 > 0));
    if (adaptive)
        return (0 - v0) / (v1 - v0);
    else
        return 0.5f;
}

    void swap(ref List<int> indicies)
    {
        int cache = indicies[indicies.Count - 2];
        indicies[indicies.Count - 2] = indicies[indicies.Count - 1];
        indicies[indicies.Count - 1] = cache;

    }

    bool isInside(float x, float y, float z)
    {
        //check cones?
        float[] vert={x-5,y,z-5}; // move to center
        foreach( Cone cone in cones ){
            if(isLyingInCone(vert, cone.tip, cone.bot,cone.aperture))return true;
        }

        // float[] coord ={x-5,y,z-5};
        // float[] tip = {0,10,0};
        // float[] bot = {0,0,0};
        //isLyingInCone(coord, tip, bot, 100)
        
        return  y<1;  //floor
        // ball_function(x, y, z) > 0 ||

    }


    double ball_function(float x, float y, float z)
    {
        //move 
        x -= 5;
        y -= 5;
        z -= 5;
        return 2.5 - Mathf.Sqrt(x * x + y * y + z * z); 
    }

    double circle_function(float x, float y, float z)
    {
        //move circle to the right
        x = x - 5;
        y = y - 5;
        return 2.5 - Mathf.Sqrt(x * x + y * y); //+z*z
    }

    public void add_cone(Vector3 coord, float height, float aperture)
    {
        float x=coord.x;
        float y=coord.y;
        float z=coord.z;
        float[] tip = {x,y+height,y};
        float[] bot = {x,y,z};
        
        // float[] tip = {0,10,0};
        // float[] bot = {0,0,0};
        Cone cone = new Cone(tip,bot,aperture);
        cones.Add(cone);
    }

    // Update is called once per frame
    void Update()
    {

        // If one point is inside and not all points are inside place vertex -
        // another way to phrase it is just show the edges where a sign switch happens
        for (int x = 0; x <= areaSize; x++) //start a bit to the right to have neighbors
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++)
                {
                    if (x > 0 && y > 0)
                    { //respect area boundries
                        //check edge for sign change
                        bool solid1 = isInside(x, y, z + 0);
                        bool solid2 = isInside(x, y, z + 1);

                        if (solid1 ^ solid2)
                        {
                            // if()
                            if (!solid2)
                            {
                                verticies.Add(vertexe[x - 1, y - 1, z]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x - 0, y - 1, z]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x - 1, y, z]);
                                indicies.Add(verticies.Count - 1);
                            }
                            else
                            {
                                verticies.Add(vertexe[x - 1, y, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x - 0, y - 1, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x - 1, y - 1, z]);
                                indicies.Add(verticies.Count - 1);

                            }
                        }
                    }

                    if (x > 0 && z > 0)
                    { //respect area boundries
                        //check edge for sign change
                        bool solid1 = isInside(x, y + 0, z);
                        bool solid2 = isInside(x, y + 1, z);

                        if (solid1 ^ solid2)
                        {
                            if (solid2)
                            {
                                verticies.Add(vertexe[x - 1, y, z - 1]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y, z - 1]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x - 1, y, z]);
                                indicies.Add(verticies.Count - 1);
                            }
                            else
                            {

                                verticies.Add(vertexe[x - 1, y, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y, z - 1]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x - 1, y, z - 1]);
                                indicies.Add(verticies.Count - 1);
                            }
                        }
                    }

                    if (y > 0 && z > 0)
                    { //respect area boundries
                        //check edges for sign change
                        bool solid1 = isInside(x, y, z);
                        bool solid2 = isInside(x + 1, y, z);

                        if (solid1 ^ solid2)
                        {

                            if (!solid2)
                            {
                                verticies.Add(vertexe[x, y - 1, z - 1]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y, z - 1]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);

                                verticies.Add(vertexe[x, y - 1, z]);
                                indicies.Add(verticies.Count - 1);
                            }
                            else
                            {

                                verticies.Add(vertexe[x, y - 1, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y, z]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y, z - 1]);
                                indicies.Add(verticies.Count - 1);
                                verticies.Add(vertexe[x, y - 1, z - 1]);
                                indicies.Add(verticies.Count - 1);
                            }
                        }
                    }
                }
            }
        }
        mesh.vertices = verticies.ToArray();
        Vector3[] vertices = mesh.vertices;
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Quads, 0);

        filter.mesh = mesh;
        /* pseudocode to create authentic sand

        start when hand gesture fist detected && angle correct 
            get coord from hand
            add coords to list of vertices inside / or add cone object to list?
            increase height of cone
            Maybe Hyperboloid https://en.wikipedia.org/wiki/Hyperboloid?
            https://stackoverflow.com/questions/12826117/how-can-i-detect-if-a-point-is-inside-a-cone-or-not-in-3d-space
            https://stackoverflow.com/questions/10768142/verify-if-point-is-inside-a-cone-in-3d-space
            https://stackoverflow.com/questions/41443171/how-to-determine-if-point-is-inside-skewed-conical-frustum
        */
    }

    /** Credit @https://stackoverflow.com/questions/10768142/verify-if-point-is-inside-a-cone-in-3d-space
    * @param x coordinates of point to be tested 
    * @param t coordinates of apex point of cone
    * @param b coordinates of center of basement circle
    * @param aperture in radians
        Maybe move into cone
    */
    static public bool isLyingInCone(float[] x, float[] t, float[] b, 
                                        float aperture){

        // This is for our convenience
        float halfAperture = aperture/2.0f;

        // Vector pointing to X point from apex
        float[] apexToXVect = dif(t,x);

        // Vector pointing from apex to circle-center point.
        float[] axisVect = dif(t,b);

        // X is lying in cone only if it's lying in 
        // infinite version of its cone -- that is, 
        // not limited by "round basement".
        // We'll use dotProd() to 
        // determine angle between apexToXVect and axis.
        bool isInInfiniteCone = dotProd(apexToXVect,axisVect)
                                /magn(apexToXVect)/magn(axisVect)
                                    >
                                // We can safely compare cos() of angles 
                                // between vectors instead of bare angles.
                                Math.Cos(halfAperture);


        if(!isInInfiniteCone) return false;

        // X is contained in cone only if projection of apexToXVect to axis
        // is shorter than axis. 
        // We'll use dotProd() to figure projection length.
        bool isUnderRoundCap = dotProd(apexToXVect,axisVect)
                                /magn(axisVect)
                                    <
                                magn(axisVect);
        return isUnderRoundCap;
    }

    static public float dotProd(float[] a, float[] b){
        return a[0]*b[0]+a[1]*b[1]+a[2]*b[2];
    }

    static public float[] dif(float[] a, float[] b){
        return (new float[]{
                a[0]-b[0],
                a[1]-b[1],
                a[2]-b[2]
        });
    }

    static public float magn(float[] a){
        return (float) (Math.Sqrt(a[0]*a[0]+a[1]*a[1]+a[2]*a[2]));
    }
}

public struct Cone
{    
    public float[] tip;
    public float[] bot;
    public float aperture;

    public Cone(float[] tip, float[] bot,float aperture=100)
    {
        this.tip = tip;
        this.bot = bot;
        this.aperture = aperture;
    }
}