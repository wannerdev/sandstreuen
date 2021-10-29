using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Bodies;

public class DualContouring3D : MonoBehaviour
{
    //settings
    public Material material;
    public int areaSize = 20;
    public bool notAdaptive = true;
    public float floor=1;

    //Default cone  
    static Vector2 defAngle =new Vector2(0.5f,0.5f);
    const float  defHeight = 10;

    //grid to generate quads by
    internal Vector3[,,] vertexGrid;
    //Quads
    internal List<Vector3> vertices;
    internal List<int> indicies;

    //public bool[,,] grid;
    //sdf = new float[areaSize*x*areaSize*y*areaSize*z]; //??
    //in the future maybe use onedimensional array - compiler heavily optimized for one dimension
    internal float[,,] sdfgrid;
    public Vector3 offset;

    internal Mesh mesh;
    internal MeshFilter filter;
    internal MeshRenderer meshRenderer;

    //debug
    private int flag=0;
    //regarding schmitz adaptivity
    private readonly int MaxParticleIterations=50;
    float threshold = 0.001f;

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
        
        vertexGrid = new Vector3[areaSize + 1, areaSize + 1, areaSize + 1];

        //generated SDF space
        sdfgrid = new float [areaSize +2, areaSize+2 , areaSize+2 ];
        Vector3 pos;
        for ( int i = 0; i < (areaSize +2);i++ ) {
            for ( int j = 0; j < (areaSize +2);j++ ) {
                for ( int k = 0; k <(areaSize +2);k++ ) {
                    pos = new Vector3(i,j,k);
                    pos.y +=-8;
                    pos += offset;
                    sdfgrid[i,j,k] = sdConeExact(pos,defAngle,defHeight);
                    // Debug.Log(sdfgrid[i,j,k]);
                    // Mathf.PerlinNoise(i/10+0.32342f+areaSize,j+0.568f+areaSize);//float.MaxValue;
                }
            }
        }
        //Surface Quads
        indicies = new List<int>();
        this.vertices = new List<Vector3>();



        filter = gameObject.AddComponent<MeshFilter>();

        generateVertices(defAngle,defHeight);

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
                                vertices.Add(vertexGrid[x - 1, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 0, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {
                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 0, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 1, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

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
                                vertices.Add(vertexGrid[x - 1, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 1, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
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
                                vertices.Add(vertexGrid[x, y - 1, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {

                                vertices.Add(vertexGrid[x, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y - 1, z - 1]);
                                indicies.Add(vertices.Count - 1);
                            }
                        }
                    }
                }
            }
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Quads, 0);


        filter.mesh = mesh;
        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    private void generateVertices(Vector2 angle,float height)
    {
         for (int x = 0; x <= areaSize; x++)
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++)
                {
                    Vector3 vertex = find_vertex(x, y, z, angle, height);
                    if (vertex == Vector3.negativeInfinity)
                    {
                        continue;
                    }
                    vertexGrid[x, y, z] = vertex;
                    //add indicies?
                }
            }
        }
    }

    private Vector3 find_vertex(float x, float y, float z, Vector2 angle, float height)
    {
        if (notAdaptive)
        {
            return new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
        }
        threshold*= threshold;
        Vector3 vec = new Vector3(x, y, z);
        float[,,] edges = new float[2, 2, 2];

        // Vector3 pos;
        for (int j = 0; j < 2; j++)
        {
            for (int k = 0; k < 2; k++)
            {
                for (int l = 0; l < 2; l++)
                {
                    // pos = new Vector3(x + j, y + k, +l);
                    // pos.y +=-10;
                    // pos += offset;
                    edges[j, k, l] = sdfgrid[(int)x + j,(int) y + k,(int) z+l];//sdConeExact(pos, new Vector2(0.5f,0.5f),10);
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


        var normals = new List<Vector3>();
        foreach (Vector3 v in changes ){
            normals.Add(normal_from_Cone(v[0], v[1],v[2],angle,height));
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
        // flag++;
        // if(flag >= 5){
        //     Debug.Log("Vert."+vec.ToString()+" C:" +c.ToString());
        //     flag=int.MinValue;
        // }
        return c;
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

    private Vector3 normal_from_Cone6(float x, float y, float z, float d = 0.01f)
    {
        return new Vector3(sdConeExact(new Vector3(x + d, y, z),new Vector2(0.5f,0.5f),5 ) - sdConeExact(new Vector3(x - d, y, z),new Vector2(0.5f,0.5f),5) / 2 / d,
                           sdConeExact(new Vector3(x, y + d, z),new Vector2(0.5f,0.5f),5 ) - sdConeExact(new Vector3(x, y - d, z),new Vector2(0.5f,0.5f),5) / 2 / d,
                           sdConeExact(new Vector3(x, y, z + d),new Vector2(0.5f,0.5f),5 ) - sdConeExact(new Vector3(x, y, z - d),new Vector2(0.5f,0.5f),5) / 2 / d).normalized;
    }
  private Vector3 normal_from_Cone(float x, float y, float z,Vector2 angle, float height=5, float d = 0.01f)
    {
        return new Vector3(sdConeExact(new Vector3(x + d, y, z),angle,height ) , 
                           sdConeExact(new Vector3(x, y + d, z),angle,height ) , 
                           sdConeExact(new Vector3(x, y, z + d),angle,height )   ).normalized;
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
            return sdfgrid[x,y,z] <= 0;
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
            sdfgrid[Math.Abs((int)coord.x), Math.Abs((int)coord.y) , Math.Abs((int)coord.z)]=-1;
            generateVertices(defAngle, defHeight);
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
                    Vector3 position = new Vector3(x-coord.x,y-coord.y ,z-coord.z);
                    position += offset;
                    float cache = sdConeExact(position, angle, height) ;
                    // flag=2;
                    //  if(cache  < 5 ) {//to test 0.5
                        // Debug.Log("Cache cone:"+cache);
                        sdfgrid[x,y,z] = cache;
                        // flag=1;
                    // }
                }
            }
        }
        generateVertices(angle,height);
        // Debug.Log("Is flag "+flag);
        // flag=0;
        return success;
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
        // gravity();
     }

    //optimize by dynamically adapting areasize to be changed by position and dimension of new cone
    public void gravity()
    {
          for (int x = 0; x <= areaSize; x++) 
        {
            for (int y = 1; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++){
                    if(sdfgrid[x,y,z] < 1){
                         if(!(sdfgrid[x,y-1,z] < 1)){
                            //no ground
                            sdfgrid[x,y,z] = 2;
                            sdfgrid[x,y-1,z] = sdfgrid[x,y,z];
                        }else if(!(sdfgrid[x+1,y-1,z] < 1)){

                            sdfgrid[x,y,z] = 2;
                            sdfgrid[x+1,y-1,z] = sdfgrid[x,y,z];;
                        }else if(x!=0 && !(sdfgrid[x-1,y-1,z] < 1)){

                            sdfgrid[x,y,z] = 2;
                            sdfgrid[x-1,y-1,z] = sdfgrid[x,y,z];;
                        }else if(!(sdfgrid[x,y-1,z+1] < 1)){

                            sdfgrid[x,y,z] = 2;
                            sdfgrid[x,y-1,z+1] = sdfgrid[x,y,z];;
                        }else if(x!=0 && z !=0 && !(sdfgrid[x-1,y-1,z-1] < 1)){

                            sdfgrid[x,y,z] = 2;
                            sdfgrid[x-1,y-1,z-1] = sdfgrid[x,y,z];;
                        }else if( y==0 || sdfgrid[x,y-1,z] < 1 ){
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
        //it seems there is a maximum to the vertices you can add, no error is displayed when it happens!
        vertices.Clear();
        indicies.Clear();

        // If one point is inside and not all points are inside place vertex -
        // another way to phrase it is just show the edges where a sign switch happens
        for (int x = 0; x < areaSize; x++) 
        {
            for (int y = 0; y < areaSize; y++)
            {
                for (int z = 0; z < areaSize; z++)
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
                                vertices.Add(vertexGrid[x - 1, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 0, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {
                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 0, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 1, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

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
                                vertices.Add(vertexGrid[x - 1, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 1, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
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
                                vertices.Add(vertexGrid[x, y - 1, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {

                                vertices.Add(vertexGrid[x, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y - 1, z - 1]);
                                indicies.Add(vertices.Count - 1);
                            }
                        }
                    }
                }
            }
        }
        

        // Vector3[] vertices = mesh.vertices;
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Quads, 0);
        mesh.vertices = vertices.ToArray();
		// mesh.vertices = vertices;
        filter.mesh = mesh;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}
