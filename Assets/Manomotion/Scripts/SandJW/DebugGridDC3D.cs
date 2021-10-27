
using System.Collections.Generic;
using UnityEngine;

class DebugGridDC3D : MonoBehaviour
{

    public int areaSize=80;
    public DualContouring3D d3D;

    MeshRenderer meshRenderer ;
    MeshFilter filter ;
    List<Vector3> verticies;
    List<int> indicies ;
    int flag;
    Mesh mesh;
    public void Start()
    {
        flag=0;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        filter = gameObject.AddComponent<MeshFilter>();
         mesh = new Mesh();
        verticies = new List<Vector3>();


        var mesh2 = new Mesh();
        var adaptedV = new List<Vector3>();

        indicies = new List<int>();
        var indicies2 = new List<int>();

        Vector3[,,] vertexe = new Vector3[areaSize + 1, areaSize + 1, areaSize + 1];
        for (int x = 0; x <= areaSize; x++)
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++)
                {
                    //normal to define - for now 2D
                    // vertexe[x, y, z] = find_vertex(x, y, z, new Vector3(0, 0, 0));
                    // //Add centered/adapted vertex
                    if(flag % 100 == 0){
                    verticies.Add(new Vector3(x,y,z));
                    indicies.Add(verticies.Count - 1);
                    }
                    flag++;
                    if(d3D.sdfgrid[x,y,z] > 1){
                        if(flag % 1000==0){
                            flag++;
                            GameObject text = new GameObject();
                            TextMesh t = text.AddComponent<TextMesh>();
                            t.text = ""+d3D.sdfgrid[x,y,z];
                            t.fontSize = 30;
                            t.transform.position = new Vector3(x, y, z);
                        }
                    }
                }
            }
        }

        mesh.vertices = verticies.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);

        filter.mesh = mesh;
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
for (int x = 0; x <= areaSize; x++)
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++)
                {
                    //normal to define - for now 2D
                    // vertexe[x, y, z] = find_vertex(x, y, z, new Vector3(0, 0, 0));
                    // //Add centered/adapted vertex
                    if(flag % 100 == 0){
                        verticies.Add(new Vector3(x,y,z));
                        indicies.Add(verticies.Count - 1);
                    }
                    flag++;
                    if(d3D.sdfgrid[x,y,z] > 1){
                        if(flag % 1000==0){
                            flag++;
                            GameObject text = new GameObject();
                            TextMesh t = text.AddComponent<TextMesh>();
                            t.text = ""+d3D.sdfgrid[x,y,z];
                            t.fontSize = 30;
                            t.transform.position = new Vector3(x, y, z);
                        }
                    }
                }
            }
        }

        mesh.vertices = verticies.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);

        filter.mesh = mesh;
    }

}