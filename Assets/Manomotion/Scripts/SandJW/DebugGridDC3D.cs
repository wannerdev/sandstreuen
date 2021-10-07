
using System.Collections.Generic;
using UnityEngine;

class DebugGridDC3D : MonoBehaviour
{

    public int areaSize=40;

    public void Start()
    {

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        var mesh = new Mesh();
        var verticies = new List<Vector3>();


        var mesh2 = new Mesh();
        var adaptedV = new List<Vector3>();

        var indicies = new List<int>();
        var indicies2 = new List<int>();

        Vector3[,,] vertexe = new Vector3[areaSize + 1, areaSize + 1, areaSize + 1];

        for (int x = 0; x <= areaSize; x++)
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++)
                {
                    //normal to define - for now 2D
                    vertexe[x, y, z] = find_vertex(x, y, z, new Vector3(0, 0, 0));
                    //Add centered/adapted vertex
                    verticies.Add(vertexe[x, y,z]);
                    indicies.Add(verticies.Count - 1);
                }
            }
        }

        //is vertex in cell
        mesh.vertices = verticies.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
        //mesh.po
        filter.mesh = mesh;
        //mesh.sub
        //filter.
    }



    private Vector3 find_vertex(float x, float y, float z, Vector3 normal)
    {
        //for now 2d
        return new Vector3(x + 0.5f, y + 0.5f, z); //+0.5f
    }
    bool isInside(int x, int y)
    {
        x = x - 5;
        y = y - 5;
        return circle_function(x, y) > 0;//circle_function(x,y);
    }

    double circle_function(int x, int y)
    {
        return 2.5 - Mathf.Sqrt(x * x + y * y);
    }

    void Update()
    {

    }

}