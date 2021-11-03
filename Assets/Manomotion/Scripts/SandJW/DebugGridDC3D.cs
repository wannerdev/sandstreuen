
using System.Collections.Generic;
using UnityEngine;

//Sanity grid
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
                    if(flag % 100 == 0){
                    }
                    flag++;
                    // if(d3D.sdfgrid[x,y,z] > 1 ){
                        
                    //     verticies.Add(new Vector3(x,y,z));
                    //     indicies.Add(verticies.Count - 1);
                         if(flag % 1000==0){
                             flag++;
                             GameObject text = new GameObject();
                             TextMesh t = text.AddComponent<TextMesh>();
                            //  t.text = ""+d3D.sdfgrid[x,y,z];
                             t.fontSize = 30;
                             t.transform.position = new Vector3(x, y, z);
                         }
                    // }
                }
            }
        }

        mesh.vertices = verticies.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);

        filter.mesh = mesh;
    }




    void Update()
    {
        
        //updateMesh();
    }

    void updateMesh(){
        for (int x = 0; x <= areaSize; x++)
                {
                    for (int y = 0; y <= areaSize; y++)
                    {
                        for (int z = 0; z <= areaSize; z++)
                        {
                           if(flag % 100 == 0){
                            }
                            flag++;
                            // if(d3D.sdfgrid[x,y,z] < 1){
                                
                            verticies.Add(new Vector3(x,y,z));
                            indicies.Add(verticies.Count - 1);
                                if(flag % 1000==0){
                                    flag++;
                                    GameObject text = new GameObject();
                                    TextMesh t = text.AddComponent<TextMesh>();
                                    t.text = ""+d3D.sdfgrid[x,y,z];
                                    t.fontSize = 30;
                                    t.transform.position = new Vector3(x, y, z);
                                }
                            }
                        // }
                    }
                }

        mesh.vertices = verticies.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);

        filter.mesh = mesh;
    }
    }