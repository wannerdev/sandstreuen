
using System.Collections.Generic;
using UnityEngine;

class debugGrid  : MonoBehaviour{

    //public int[][][] Space = new int[20][][];
    public int[,] Space = new int[20,20];
    public float width = 10;
    public float height = 10;
    public int GridSize;

    public void Start()
    {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
         meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter filter = gameObject.AddComponent<MeshFilter>();    
        var mesh = new Mesh();
        var verticies = new List<Vector3>();

        
        var mesh2 = new Mesh();
        var verticies2 = new List<Vector3>();

        var indicies = new List<int>();
        var indicies2 = new List<int>();
        int i=0;
        for (int y = 0;  y <= GridSize; y++)
        {
            for (int x = 0; x <= GridSize; x++)
            {
                if(isInside(x+1,y)){
                    
                verticies.Add(new Vector3(1+x, 0+y, 0));
                    indicies.Add( i);
                    i++;
                }else{
                    // indicies.Add( i );
                }

                if(isInside(x+1,y+1)){             
                verticies.Add(new Vector3(1+x, 1+y, 0));      
                    indicies.Add( i );
                i++;
                }else{
                    // indicies.Add(i);
                }
                
                if(isInside(x,y+1)){
                verticies.Add(new Vector3(0+x, 1+y, 0));
                    
                     indicies.Add( i );
                    i++;
                }else{
                    //  indicies.Add( i );
                }
                
                if(isInside(x,y)){
                verticies.Add(new Vector3(0+x, 0+y, 0));
                    indicies.Add( i );
                i++;
                }else{
                    // indicies.Add( i );
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