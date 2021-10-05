using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualConturing3D : MonoBehaviour
{
    public int areaSize = 10;
    // Start is called before the first frame update
    void Start()
    {

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        meshRenderer.material.SetColor("_Color", Color.red);
        //meshRenderer.
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        var mesh = new Mesh();
        var verticies = new List<Vector3>();


        var mesh2 = new Mesh();
        var adaptedV = new List<Vector3>();

        var indicies = new List<int>();
        var faces = new List<Vector3>();

        Vector3[,,] vertexe = new Vector3[areaSize + 1, areaSize + 1, areaSize + 1];

        for (int x = 0; x <= areaSize; x++)
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++)
                {
                    //normal to define - for now 2D
                    vertexe[x, y, z] = find_vertex(x, y, z, new Vector3(0, 0, 0));
                    if (vertexe[x, y, z] == null)
                    {
                        continue;
                    }
                    //Add centered/adapted vertex
                    //verticies.Add(vertexe[x, y, z]);
                    // indicies.Add(verticies.Count-1);
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
    }

    private Vector3 find_vertex(float x, float y, float z, Vector3 normal)
    {
        return new Vector3(x + 0.5f, y + 0.5f, z+0.5f);
    }

    void swap(ref List<int> indicies)
    {
        int cache = indicies[indicies.Count - 2];
        indicies[indicies.Count - 2] = indicies[indicies.Count - 1];
        indicies[indicies.Count - 1] = cache;

    }

    //bool isInside(Vector3 vert){
    bool isInside(float x, float y, float z)
    {
        //check cones?
        // x=x-5;
        // y=y-5;
        return ball_function(x, y, z) > 0;
    }


    double ball_function(float x, float y, float z)
    {
        //move circle
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


    // Update is called once per frame
    void Update()
    {

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
}
