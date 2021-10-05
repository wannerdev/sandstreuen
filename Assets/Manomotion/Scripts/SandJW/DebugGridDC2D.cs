
using System.Collections.Generic;
using UnityEngine;

class DebugGridDC2D  : MonoBehaviour{

    //public int[][][] Space = new int[20][][];
    public int[,] Space = new int[20,20];
    public float width = 10;
    public float height = 10;
    public int areaSize;

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
        //int i = 0;
        Vector3 [,]vertexe = new Vector3[areaSize+1,areaSize+1];
        //to debug test show adaptedV in different script

        // verticies.Add(new Vector3(0,0,0));
        // indicies.Add(verticies.Count-1);
        // for (int z = 0;  z <= areaSize; z++)
        // {
            for (int x = 0;  x <= areaSize; x++)
            {
                for (int y = 0; y <= areaSize; y++)
                {                
                    //normal to define - for now 2D
                    vertexe[x,y] = find_vertex(x,y,0, new Vector3(0,0,0) );
                    if (vertexe[x,y] == null){
                        continue;
                    }
                    //Add centered/adapted vertex
                    verticies.Add(vertexe[x,y]);
                    indicies.Add(verticies.Count-1);
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
        return new Vector3(x+0.5f,y+0.5f,z); //+0.5f
    }
    bool isInside(int x, int y){
        x=x-5;
        y=y-5;
        return circle_function(x,y) > 0;//circle_function(x,y);
    }

    double circle_function(int x,int  y){
        return 2.5 - Mathf.Sqrt(x*x + y*y);
    }

    void Update()
    {
   
    }

}