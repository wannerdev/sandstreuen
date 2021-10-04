using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualConturing : MonoBehaviour
{
    public int areaSize = 10;
    // Start is called before the first frame update
    void Start()
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
                    //adaptedV.Add(vertexe[x,y]);
                }
            }
            // If one point is inside and not all are inside place vertex -
            // another way to phrase it is just show the edges where a sign switch happens                 
            for (int x = 1; x <= areaSize; x++) //start a bit to the right to have neighbors
            {  
                for (int y = 0;  y <= areaSize; y++)
                {
                    //check edges for sign change
                    float y0 = y;
                    float y1 = y+1;

                    bool y0_inside = isInside(x,y0,0);
                    bool y1_inside = isInside(x,y1,0);
                    if(y0_inside ^y1_inside){
                        //prob not good
                        //Vector3 tesvert = adaptedV[x-1+y];
                        //tesvert.x -= 1;
                        
                        verticies.Add(vertexe[x-1,y]);
                        indicies.Add(verticies.Count-1);

                        verticies.Add(vertexe[x,y]);
                        indicies.Add(verticies.Count-1);
                        // if(y0_inside){
                        //     verticies.Add(vertexe[x,y]);
                        //     indicies.Add(verticies.Count-1);                           
                        //     verticies.Add(vertexe[x-1,y]);
                        //     indicies.Add(verticies.Count-1);
                        // }else{
                        //     verticies.Add(vertexe[x-1,y]);
                        //     indicies.Add(verticies.Count-1);
                        //     verticies.Add(vertexe[x,y]);
                        //     indicies.Add(verticies.Count-1);
                        // }
                    }


                }   
            }
          for (int x = 0; x <= areaSize; x++) 
            {  
                for (int y = 1;  y <= areaSize; y++)//start a bit up to have neighbors
                {
                    //check edges for sign change
                    float x0 = x;
                    float x1 = x+1;

                    bool x0_inside = isInside(x0,y,0);
                    bool x1_inside = isInside(x1,y,0);
                    if(x0_inside ^ x1_inside){
                        

                        verticies.Add(vertexe[x,y-1]);
                        indicies.Add(verticies.Count-1);
                        verticies.Add(vertexe[x,y]);
                        indicies.Add(verticies.Count-1);
                        //prob not good
                        // if(x0_inside){
                        //     verticies.Add(vertexe[x,y]);
                        //     indicies.Add(verticies.Count-1);
                        //     verticies.Add(vertexe[x,y-1]);
                        //     indicies.Add(verticies.Count-1);
                        // }else{
                        //     verticies.Add(vertexe[x,y-1]);
                        //     indicies.Add(verticies.Count-1);
                        //     verticies.Add(vertexe[x,y]);
                        //     indicies.Add(verticies.Count-1);
                        // }
                    }


                }   
            }
        // }

        // for (int z = 0;  z <= areaSize; z++)
        // {
        //     for (int y = 0;  y <= areaSize; y++)
        //     {
        //         for (int x = 0; x <= areaSize; x++)
        //         {
        //             isInside(x,y,z);
        //         }
        //     }
        // }
        //make mesh
        mesh.vertices = verticies.ToArray();
        Vector3[] vertices = mesh.vertices;

        // create new colors array where the colors will be created.
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
            colors[i] = Color.red;
        // assign the array of colors to the Mesh.
        mesh.colors = colors;
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Lines, 0);
        // mesh.
        filter.mesh = mesh;
    }

    private Vector3 find_vertex(float x, float y, float z, Vector3 normal)
    {
        //for now 2d
        return new Vector3(x+0.5f,y+0.5f,z); //+0.5f
    }

    void swap(ref List<int> indicies){
        int cache = indicies[indicies.Count-2];
        indicies[indicies.Count-2] = indicies[indicies.Count-1];
        indicies[indicies.Count-1] = cache;
                    
    }

    //bool isInside(Vector3 vert){
    bool isInside(float x, float y, float z){
        //check cones?
        // x=x-5;
        // y=y-5;
        return circle_function(x, y, z) > 0;
    }

    double circle_function(float x,float  y,float z){
        //move circle to the right
        x=x-5;
        y= y-5;
        return 2.5 - Mathf.Sqrt(x*x + y*y); //+z*z
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
