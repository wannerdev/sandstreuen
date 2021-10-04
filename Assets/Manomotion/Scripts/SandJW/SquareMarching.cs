
using System.Collections.Generic;
using UnityEngine;

class SquareMarching  : MonoBehaviour{

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
                
                bool x0y0 = false;
                bool x0y1 = false;
                bool x1y0 = false;
                bool x1y1 = false;

                if(isInside(x,y)){
                     x0y0 = true;
                }
                if(isInside(x,y+1)){
                     x0y1 = true;
                }
                if(isInside(x+1,y)){
                    x1y0 = true;
                }

                if(isInside(x+1,y+1)){
                    x1y1 = true;
                }

                //evaluate squares
                int cases = (   
                    (x0y0 ? 1: 0) +
                    (x0y1 ? 2: 0) +
                    (x1y0 ? 4: 0) +
                    (x1y1 ? 8: 0));
                switch (cases) {
                        case 0:
                        case 15:
                                break ;//null;
                        case 1:                    
                        case 14:
                            verticies.Add(new Vector3(x+0.5f, y, 0));
                            verticies.Add(new Vector3(x+0, y+0.5f,0));
                            indicies.Add( i);
            i++;
                            indicies.Add( i );
            i++;
                            //single
                            if (cases==14) swap(ref indicies);
                            break;
                         case 2:                    
                         case 13:
                             verticies.Add(new Vector3(x, y+0.5f, 0));
                             verticies.Add(new Vector3(x+0.5f, y+1,0));
                             indicies.Add( i );
             i++;
                             indicies.Add( i );
             i++;
                             //single
                             if (cases==13) swap(ref indicies);
                             break  ;
                        case 4:                    
                        case 11:
                            verticies.Add(new Vector3(x+1, y+0.5f, 0));
                            verticies.Add(new Vector3(x+0.5f, y+0,0));
                            indicies.Add( i );
            i++;
                            indicies.Add( i );
            i++;
                            //single
                            if (cases==11) swap(ref indicies);
                            break  ;
                        case 8:                    
                        case 7:
                            verticies.Add(new Vector3(x+0.5f, y+1, 0));
                            verticies.Add(new Vector3(x+1, y+0.5f,0));
                            indicies.Add( i );
            i++;
                            indicies.Add( i );
            i++;
                            //single
                            if (cases==7) swap(ref indicies);
                            break  ;        
                        case 3:                    
                        case 12:
                           verticies.Add(new Vector3(x+0.5f, y+0, 0));
                           verticies.Add(new Vector3(x+0.5f, y+1,0));
                           indicies.Add( i );
            i++;
                           indicies.Add( i );
            i++;
                            //single
                            if (cases==12) swap(ref indicies);
                            break  ;         
                        case 5:                    
                        case 10:
                            verticies.Add(new Vector3(x+0, y+0.5f, 0));
                            verticies.Add(new Vector3(x+1, y+0.5f,0));
                            indicies.Add( i );
            i++;
                            indicies.Add( i );
            i++;
                            //single
                            if (cases == 5) swap(ref indicies);
                            break  ;           
                        case 9:  
                            verticies.Add(new Vector3(x+0, y, 0));
                            verticies.Add(new Vector3(x+0, y+0.5f,0));           
                            indicies.Add( i );
            i++;
                            indicies.Add( i );
            i++;
                            verticies.Add(new Vector3(x+0.5f, y+1, 0));
                            verticies.Add(new Vector3(x+1, y+0.5f,0));            
                            indicies.Add( i );
            i++;
                            indicies.Add( i );
            i++;
                            break;            
                        case 6:  
                            verticies.Add(new Vector3(x+1, y+0.5f, 0));
                            verticies.Add(new Vector3(x+0.5f, y+0,0));
                            indicies.Add( i );
            i++;
                            indicies.Add( i );
            i++;
                            verticies.Add(new Vector3(x+0, y+0.5f, 0));
                            verticies.Add(new Vector3(x+0.5f, y+1,0));
                            indicies.Add( i);
            i++;
                            indicies.Add( i );
            i++;
                            break;
                        default:                            
                            Debug.Log("no cases");
                            break;

                }
            }
        }

        //is vertex in cell
        mesh.vertices = verticies.ToArray(); 
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Lines, 0);
        //mesh.po
        filter.mesh = mesh;
        //mesh.sub
        //filter.
    }

    void swap(ref List<int> indicies){
        int cache = indicies[indicies.Count-2];
        indicies[indicies.Count-2] = indicies[indicies.Count-1];
        indicies[indicies.Count-1] = cache;
                    
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