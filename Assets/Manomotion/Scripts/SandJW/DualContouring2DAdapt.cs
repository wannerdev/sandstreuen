using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Q = Qef;

public class DualContouring2DAdapt : MonoBehaviour
{
    public int areaSize = 10;
    public bool isAdaptive = false;
    // Start is called before the first frame update
    void Start()
    {
        
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        meshRenderer.material.SetColor("_Color", Color.red);
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
            for (int x = 0;  x <= areaSize; x++)
            {
                for (int y = 0; y <= areaSize; y++)
                {                
                    //normal to define - for now 2D
                    vertexe[x,y] = find_vertex(x,y, new Vector3(0,0,0) );
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

                    bool y0_inside = isInside(x,y0);
                    bool y1_inside = isInside(x,y1);
                    if(y0_inside ^y1_inside){
                        //prob not good
                        
                        verticies.Add(vertexe[x-1,y]);
                        indicies.Add(verticies.Count-1);

                        verticies.Add(vertexe[x,y]);
                        indicies.Add(verticies.Count-1);
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

                    bool x0_inside = isInside(x0,y);
                    bool x1_inside = isInside(x1,y);
                    if(x0_inside ^ x1_inside){
                        verticies.Add(vertexe[x,y-1]);
                        indicies.Add(verticies.Count-1);
                        verticies.Add(vertexe[x,y]);
                        indicies.Add(verticies.Count-1);
                    }
                }   
            }

        //make mesh
        mesh.vertices = verticies.ToArray();
        Vector3[] vertices = mesh.vertices;

        // assign the array of colors to the Mesh.
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Lines, 0);
        
        filter.mesh = mesh;
    }

    private Vector3 find_vertex(float x, float y, Vector2 normal)
    {
        if (!isAdaptive) return new Vector3(x+0.5f,y+0.5f,0);
        float x0y0 = circle_function(x + 0.0f, y + 0.0f);
        float x0y1 = circle_function(x + 0.0f, y + 1.0f);
        float x1y0 = circle_function(x + 1.0f, y + 0.0f);
        float x1y1 = circle_function(x + 1.0f, y + 1.0f);

        
        var changes = new List<Vector2>();
        if ((x0y0 > 0) ^ (x0y1 > 0))
            changes.Add(new Vector2(x + 0, y + adapt(x0y0, x0y1) ));
        if ((x1y0 > 0) ^ (x1y1 > 0))
            changes.Add(new Vector2(x + 1, y + adapt(x1y0, x1y1)));
        if ((x0y0 > 0) != (x1y0 > 0))
            changes.Add(new Vector2(x + adapt(x0y0, x1y0), y + 0));
        if ((x0y1 > 0) != (x1y1 > 0))
            changes.Add(new Vector2(x + adapt(x0y1, x1y1), y + 1));

        if (changes.Count <= 1)
            return new Vector2 (0,0); //?? tochange!

        // For each sign change location v[i], we find the normal n[i].

        var normals = new List<float>();
        foreach (Vector2 v in changes ){
            Vector2 n = f_normal(v[0], v[1]);
            normals.Add(n.x);
            normals.Add(n.y);
        }

        return new Vector3(0,0,0);//solve_qef_2d(x, y, changes, normals);
        //return solve_qef_2d(x, y, changes, normals);
    }

    Vector2 f_normal(float x, float y ){ //d =0.01 f=circlefunction;
        float d = 0.01f;
    //Given a sufficiently smooth 2d function, f, returns a function approximating of the gradient of f.
    // d controls the scale, smaller values are a more accurate approximation."""
        return  new Vector2(
            (circle_function(x + d, y) - circle_function(x - d, y)) / 2 / d,
            (circle_function(x, y + d) - circle_function(x, y - d)) / 2 / d
        ).normalized;
            //return new Vector2(0,0); //implement
    }

//  public static Vector<float> CalculateCubeQEF(Vector3[] normals, Vector3[] positions, Vector3 meanPoint)
//     {
//         var A = DenseMatrix.OfRowArrays(normals.Select(e => new[] { e.X, e.Y, e.Z }).ToArray());
//         var b = DenseVector.OfArray(normals.Zip(positions.Select(p => p - meanPoint), Vector3.Dot).ToArray());

//         var pseudo = PseudoInverse(A);
//         var leastsquares = pseudo.Multiply(b);

//         return leastsquares + DenseVector.OfArray(new[] { meanPoint.X, meanPoint.Y, meanPoint.Z });
//     }
    // Vector2 solve_qef_2d(double  v0, double v1, List<Vector2> changes, List<float> l){
    //     // Mat3 ata, Vec3 atb, Vec4 pointaccum,out Vec3 x)
    //     qef = QEF.make_2d(positions, normals);

    //     residual, v = qef.solve();
    //     Q.Solve( ata,  atb, pointaccum,out x));
    //     //object p = Qef.Solve();
    //     return new Vector2(0,0); //implement
    // }

    float adapt(double  v0, double v1){
        //v0 and v1 are numbers of opposite sign. 
        //This returns how far you need to interpolate from v0 to v1 to get to 0
        //assert((v1 > 0) != (v0 > 0));
        if (isAdaptive)
            return (float) ((0 - v0) / (v1 - v0));
        else
            return 0.5f;
    }
    void swap(ref List<int> indicies){
        int cache = indicies[indicies.Count-2];
        indicies[indicies.Count-2] = indicies[indicies.Count-1];
        indicies[indicies.Count-1] = cache;
                    
    }

    bool isInside(float x, float y){
        //check cones?
        // x=x-5;
        // y=y-5;
        return circle_function(x, y) > 0;
    }

    float circle_function(float x,float  y){
        //move circle to the right
        x=x-5;
        y= y-5;
        return (float) (2.5 - Mathf.Sqrt(x*x + y*y)); 
    }

}
