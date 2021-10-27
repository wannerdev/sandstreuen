using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Qef;
using static Bodies;

public class DualContouring3D : MonoBehaviour
{
    //settings
    public Material material;
    public int areaSize = 20;
    public bool notAdaptive = true;
    public float floor=1;


    internal Vector3[,,] vertexe;
    //Quads
    internal List<Vector3> verticies;
    internal List<int> indicies;

    public bool[,,] grid;
    internal float[,,] sdfgrid;
    public Vector3 offset;

    internal Mesh mesh;
    internal MeshFilter filter;
    internal MeshRenderer meshRenderer;

    //debug
    private int flag=0;
    //regarding schmitz adaptivity
    private readonly int MaxParticleIterations=50;

    // Start is called before the first frame update
    void Start()
    {

        //Move  0,0,0 in the middle
        this.transform.position -= new Vector3(areaSize/2,0,areaSize/2);
        offset = gameObject.transform.position;

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        //inits
        mesh = new Mesh();

        //
        vertexe = new Vector3[areaSize + 1, areaSize + 1, areaSize + 1];
        
        //generated space
        grid = new bool [areaSize +2, areaSize+2 , areaSize+2 ];
        //sdfgrid = new float [areaSize +2, areaSize+2 , areaSize+2 ];
        
        //Surface Quads
        indicies = new List<int>();
        verticies = new List<Vector3>();

        //sdf = new float[areaSize*areaSize*areaSize*3];
        //in the future maybe use onedimensional array with the float values instead of boolean


        filter = gameObject.AddComponent<MeshFilter>();
        var mesh2 = new Mesh();
        var adaptedV = new List<Vector3>();

        var faces = new List<Vector3>();

        //Generate vertexes
        for (int x = 0; x <= areaSize; x++)
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++)
                {
                    Vector3 vertex = find_vertex(x, y, z, new Vector3(0, 0, 0));
                    if (vertex == Vector3.negativeInfinity)
                    {
                        continue;
                    }
                    vertexe[x, y, z] = vertex;
                    //add indicies?
                }
            }
        }

        // If one point is inside and not all points are inside place vertex -
        // another way to phrase it is just show the edges where a sign switch happens
        for (int x = 0; x <= areaSize; x++) 
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


        
		mesh.vertices = vertices;
        filter.mesh = mesh;
        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    private Vector3 find_vertex(float x, float y, float z, Vector3 normal)
    {
        if (notAdaptive)
        {
            return new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
        }
        Vector3 vec = new Vector3(x, y, z);
        float[,,] edges = new float[2, 2, 2];
        for (int j = 0; j < 2; j++)
        {
            for (int k = 0; k < 2; k++)
            {
                for (int l = 0; l < 2; l++)
                {
                    Vector3 pos = new Vector3(x + j, y + k, +l);
                    edges[j, k, l] = sdConeExact(pos, new Vector2(0.5f,0.5f),5);
                }
            }
        }

        var changes = new List<Vector3>();

        for (int dx = 0; dx < 2; dx++)
        {
            for (int dy = 0; dy < 2; dy++)
            {
                if ((edges[dx, dy, 0] > 0) ^ (edges[dx, dy, 1] > 0))
                {
                    changes.Add(new Vector3(x + dx, y + dy, z + adapt(edges[dx, dy, 0], edges[dx, dy, 1])));
                }
            }
        }


        for (int dx = 0; dx < 2; dx++)
        {
            for (int dz = 0; dz < 2; dz++)
            {
                if ((edges[dx, 0, dz] > 0) ^ (edges[dx, 1, dz] > 0))
                {
                    changes.Add(new Vector3(x + dx, y + adapt(edges[dx, 0, dz], edges[dx, 1, dz]), z + dz));
                }
            }
        }


        for (int dy = 0; dy < 2; dy++)
        {
            for (int dz = 0; dz < 2; dz++)
            {
                if ((edges[0, dy, dz] > 0) ^ (edges[1, dy, dz] > 0))
                {
                    changes.Add(new Vector3(x + adapt(edges[0, dy, dz], edges[1, dy, dz]), y + dy, z + dz));
                }
            }
        }

        if (changes.Count <= 1)
            return new Vector3(x + 0.5f, y + 0.5f, z + 0.5f); 

        float threshold = 0.001f;

        var normals = new List<Vector3>();
        foreach (Vector3 v in changes ){
            normals.Add(normal_from_Cone(v[0], v[1],v[2]));
            // Vector3 n = normal_from_Ball(v[0], v[1],v[2]);
            // normals.Add(n.x);
            // normals.Add(n.y);
            // normals.Add(n.z);
        }

        // changes.Count
        float count = changes.Count;
        //opensource repo
        //https://github.com/theSoenke/ProceduralTerrain/blob/master/Assets/ProceduralTerrain/Core/Scripts/Voxel/Meshing/DualControuringUniform.cs
        // start mass point
        // calculated by mean of intersection points
        Vector3 c = new Vector3();
        for (int i = 0; i < count; i++)
        {
            c += changes[i];
        }
        c /= count;

        for (int i = 0; i < MaxParticleIterations; i++)
        {
            // force that acts on mass
            Vector3 force = new Vector3();

            for (int j = 0; j < count; j++)
            {
                Vector3 xPoint = changes[j];
                Vector3 xNormal = normals[j];

                force += xNormal * -1 * Vector3.Dot(xNormal, c - xPoint);
            }

            // dampen force
            float damping = 1 - (float)i / MaxParticleIterations;
            c += force * damping / count;

            if (force.sqrMagnitude < threshold)
            {
                break;
            }
        }

        return c;
        //b ist b = [v[0] * n[0] + v[1] * n[1] + v[2] * n[2] for v, n in zip(positions, normals)]
        //a
        //Q.Qef solver = new Qef(); 
        // Mat3 ata, Vec3 atb, Vec4 pointaccum,out Vec3 x)
        // Qef.Vec3 result = new Qef.Vec3(x, y, z);
        // Qef.Vec3 col0 = new Qef.Vec3(x, y, z),
        //        col1 = new Qef.Vec3(x, y, z),
        //        col2 = new Qef.Vec3(x, y, z);
        // Qef.Mat3 aTa = new Qef.Mat3(col0, col1, col2);
        // Qef.Vec3 aTb = new Qef.Vec3(x, y, z);
        // Qef.Vec4 pointaccum = new Qef.Vec4(x, y, z, z);
        // Qef

        //numpy.
        // Numpy.np.linalg.lstsq(a,b, 0.05f);

        // float res = Qef.Solve(aTa, aTb, pointaccum, out result);

        // return new Vector3(result.x, result.y, result.z);
        // return new Vector3(x, y, z);
        //return Qef.(positions, normals);
        // return Qef.solve_qef_3d(x, y, changes, normals);
    }

    private float adapt(float v0, float v1)
    {
        //v0 and v1 are numbers of opposite sign. This returns how far you need to interpolate from v0 to v1 to get to 0
        //assert((v1 > 0) != (v0 > 0));
        if (notAdaptive)
        {
            return 0.5f;
        }
        else
        {
            return (0 - v0) / (v1 - v0);
        }
    }

    void swap(ref List<int> indicies)
    {
        int cache = indicies[indicies.Count - 2];
        indicies[indicies.Count - 2] = indicies[indicies.Count - 1];
        indicies[indicies.Count - 1] = cache;

    }

    //approximate of normal
    private Vector3 normal_from_Ball(float x, float y, float z, float d = 0.01f)
    {
        return new Vector3(ball_function(x + d, y, z) - ball_function(x - d, y, z) / 2 / d,
                           ball_function(x, y + d, z) - ball_function(x, y - d, z) / 2 / d,
                           ball_function(x, y, z + d) - ball_function(x, y, z - d) / 2 / d).normalized;
    }

    private Vector3 normal_from_Cone(float x, float y, float z, float d = 0.01f)
    {
        return new Vector3(sdConeExact(new Vector3(x + d, y, z),new Vector2(0.5f,0.5f),5 ) - sdConeExact(new Vector3(x - d, y, z),new Vector2(0.5f,0.5f),5) / 2 / d,
                           sdConeExact(new Vector3(x, y + d, z),new Vector2(0.5f,0.5f),5 ) - sdConeExact(new Vector3(x, y - d, z),new Vector2(0.5f,0.5f),5) / 2 / d,
                           sdConeExact(new Vector3(x, y, z + d),new Vector2(0.5f,0.5f),5 ) - sdConeExact(new Vector3(x, y, z - d),new Vector2(0.5f,0.5f),5) / 2 / d).normalized;
    }

    //approximate of normal
    private Vector3 normal_from_F(Func<float, float, float, float> function, float x, float y, float z, float d = 0.01f)
    {
        return new Vector3(function(x + d, y, z) - function(x - d, y, z) / 2 / d,
                           function(x, y + d, z) - function(x, y - d, z) / 2 / d,
                           function(x, y, z + d) - function(x, y, z - d) / 2 / d).normalized;
    }

    bool isInside(int x, int y, int z)
    {
        //check cones add floor again or generate noise
        // if(notAdaptive){
            return grid[x,y,z];
        // }else{
        //     if( sdConeExact(new Vector3(x,y,z), new Vector2(0.5f, 0.5f), 5) < 0 ){
        //         return true;
        //     }
        //     return false;
        // }
   
        // return y < floor; 
        
    }

    public bool add_single(Vector3 coord, int material){
        if(checkBounds(coord)){
            coord.x +=Math.Abs(offset.x);
            coord.y +=Math.Abs(offset.y);
            coord.z +=Math.Abs(offset.z);
            grid[Math.Abs((int)coord.x), Math.Abs((int)coord.y) , Math.Abs((int)coord.z)]=true;
            return true;
        }
        return false;
    }

    public bool add_cone(Vector3 coord, float height, int material)
    {
        float aperture = Bodies.sands[material];
        Debug.Log("executed at " + coord.ToString());
        if(!checkBounds(coord)) return false;
        Vector2 angle = new Vector2((float)Math.Sin(aperture),(float) Math.Cos(aperture));
        bool success = true;
        for (int x = 0; x <= areaSize; x++) 
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++){
                    //Vector3 position = new Vector3(x-coord.x,y-coord.y ,z-coord.z);
                    Vector3 position = new Vector3(x-coord.x,y-coord.y ,z-coord.z);
                    position += offset;
                    if( sdConeExact(position, angle, height) < 1) {//to test 0.5
                        grid[x,y,z]=true;
                    }
                }
            }
        }
        return success;
        // Debug.Log("Is flag "+flag);
        // flag=0;
    }

    private bool checkBounds(Vector3 coord){
        if(coord.x >(areaSize/2) || coord.y > (areaSize/2) || coord.z >(areaSize/2) ||
           coord.x <(-areaSize/2) || coord.y < (-areaSize/2) || coord.z <(-areaSize/2) ){
            return false; //out of bounds
        }
        return true;
    }





    // Update is called once per frame
     void Update()
     {
        regenerateMesh();       
     }

    //optimize by dynamically adapt areasize to be changed by creating a cone
    //add check if after two stacked move
    public void gravity()
    {
          for (int x = 0; x <= areaSize; x++) 
        {
            for (int y = 1; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++){
                    if(grid[x,y,z]){
                         if(!grid[x,y-1,z]){
                            //no ground
                            grid[x,y,z] = false;
                            grid[x,y-1,z] =true;
                        }else if(!grid[x+1,y-1,z]){

                            grid[x,y,z] = false;
                            grid[x+1,y-1,z] =true;
                        }else if(x!=0 && !grid[x-1,y-1,z]){

                            grid[x,y,z] = false;
                            grid[x-1,y-1,z] =true;
                        }else if(!grid[x,y-1,z+1]){

                            grid[x,y,z] = false;
                            grid[x,y-1,z+1] =true;
                        }else if(z !=0 && !grid[x-1,y-1,z-1]){

                            grid[x,y,z] = false;
                            grid[x,y-1,z-1] =true;
                        }else if( y==0 || grid[x,y-1,z] ){
                            //ground below
                        }
                    }
                        // flag=1;
                }
            }
        }
    }
    
    //Maybe regenerate only a specific area? depending on cones dimension
    public void regenerateMesh()
    {
        //Without Clearing you can only place a certain amount of cones
        //it seems there is a maximum to the verticies you can add, no error is displayed when it happens!
        verticies.Clear();
        indicies.Clear();

        mesh.triangles.Initialize(); // = null;
        // If one point is inside and not all points are inside place vertex -
        // another way to phrase it is just show the edges where a sign switch happens
        for (int x = 0; x <= areaSize; x++) 
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
		mesh.vertices = vertices;
        filter.mesh = mesh;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        gravity();

    }
}
