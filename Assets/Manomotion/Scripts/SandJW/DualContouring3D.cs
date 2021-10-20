using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Qef;
using static Bodies;

public class DualContouring3D : MonoBehaviour
{
    public int areaSize = 20;
    public bool notAdaptive = true;

    internal List<Vector3> verticies;
    internal Vector3[,,] vertexe;
    internal List<int> indicies;

    internal float[] area;
    internal Mesh mesh;
    internal MeshFilter filter;
    public List<Cone> cones;
    public bool[,,] grid;
    public Material material;
    private int flag=0;
    public float floor=1;
    internal MeshRenderer meshRenderer;

    /* pseudocode to create authentic sand

        start when hand gesture fist detected && angle correct 
        get coord from hand
        add coords to list of vertices inside / or add cone object to list?
        increase height of cone
        */

    // Start is called before the first frame update

    void Start()
    {

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        //Shader sh = new Shader();
        meshRenderer.sharedMaterial = material;
        //meshRenderer.material.SetColor("_Color", Color.red);

        //inits
        mesh = new Mesh();
        this.cones = new List<Cone>();

        vertexe = new Vector3[areaSize + 1, areaSize + 1, areaSize + 1];
        grid = new bool [areaSize +2, areaSize+2 , areaSize+2 ];
        indicies = new List<int>();
        verticies = new List<Vector3>();
        // todo save mesh array
        //area = new float[areaSize*areaSize*areaSize*3];
        //in the future maybe use onedimensional array with the float values instead of boolean


        filter = gameObject.AddComponent<MeshFilter>();
        var mesh2 = new Mesh();
        var adaptedV = new List<Vector3>();

        var faces = new List<Vector3>();


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

        filter.mesh = mesh;
        //Move  0,0,0 in the middle
        this.transform.position -= new Vector3(areaSize/2,0,areaSize/2);
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
                    edges[j, k, l] = ball_function(x + j, y + k, +l);
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
            return Vector3.negativeInfinity; //?? tochange!



        var normals = new List<float>();
        foreach (Vector3 v in changes ){
            Vector3 n = normal_from_Ball(v[0], v[1],v[2]);
            normals.Add(n.x);
            normals.Add(n.y);
            normals.Add(n.z);
        }

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
        return new Vector3(x, y, z);
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

    //approximate of normal
    private Vector3 normal_from_F(Func<float, float, float, float> function, float x, float y, float z, float d = 0.01f)
    {
        return new Vector3(function(x + d, y, z) - function(x - d, y, z) / 2 / d,
                           function(x, y + d, z) - function(x, y - d, z) / 2 / d,
                           function(x, y, z + d) - function(x, y, z - d) / 2 / d).normalized;
    }

    bool isInside(int x, int y, int z)
    {
        if (grid[x,y,z] || y < floor)return true;
        return false;
        
        // Vector3 offset = gameObject.transform.position;        
        // //check cones
        // foreach (Cone cone in cones)
        // {
        //     // if(flag %40==0){
        //     //     Debug.Log(sdConeShort(new Vector3(x,y,z), new Vector2(0.5f, 0.5f), 10));
        //     // }
        //     //working ( 10 height pretty big)

        //     Vector3 position = new Vector3(x-cone.position[0],y-cone.position[1] ,z-cone.position[2]);
        //     position += offset;
        //     Vector2 angle = new Vector2(0.5f, 0.5f);#
        //     //< 0??
        //     //if( sdConeShort(position, angle, cone.height) < 1) return true;
            
        //     //if( sdConeExact(new Vdwctor3(x-cone.tip[0],y-cone.tip[1],z-cone.tip[2]), new Vector2(0.5f, 0.5f), cone.height) < 0 ) return true;
        //     //if( sdConeExact(new Vector3(x,y,z),  new Vector2(cone.bot[0], cone.bot[1]) ) < 0 ) return true;
        //     //}
        // } 
        // return y < floor; 
        // ball_function(x, y, z) > 0 ||
        
    }


    public void add_cone(Vector3 coord, float height, float aperture=0.5f)
    {
        Debug.Log("executed at " + coord.ToString());
        Vector3 offset = gameObject.transform.position;
        Vector2 angle = new Vector2(aperture, aperture);
        for (int x = 0; x <= areaSize; x++) 
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++){
                    //Vector3 position = new Vector3(x-coord.x,y-coord.y ,z-coord.z);
                    Vector3 position = new Vector3(x-coord.x,y-coord.y ,z-coord.z);
                    position += offset;
                    if( sdConeExact(position, angle, height) < 1) {
                        grid[x,y,z]=true;
                        // flag=1;
                    }
                }
            }
        }
        // Debug.Log("Is flag "+flag);
        // flag=0;
    }




    // Update is called once per frame
    void Update()
    {
        // regenerateMesh();
    }

    //regenerate only a specific area? depending on cones dimension

    public void regenerateMesh()
    {
        //Without this you can only place a certain amount of cones
        //it seems there is a maximum to the verticies you can add, no error is displayed when it happens!
        verticies.Clear();
        indicies.Clear();
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

        filter.mesh = mesh;
    }
}
