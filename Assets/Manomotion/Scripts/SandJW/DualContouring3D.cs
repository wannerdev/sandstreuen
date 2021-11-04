using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Bodies;

public class DualContouring3D : MonoBehaviour
{
    //settings
    public Material material;
    public int areaSize; //80
    public bool isAdaptive; //true
    public float floor; //-1
    public float scale; //1
    public float contour; //0
    public bool isGravity; //true
    public bool isConeOptimized; //true
    public bool isPretty; //false very expensive, with some optimization should be runnable
    public bool isCheapPretty; //false very expensive, with some optimization should be runnable
    private bool addedFlag; //when to recalculate vertices 
    
    
    // isWithCone start with Cone
    // someNoise  not really functional but interesting to look at with gravity enabled
    // Default just a floor
    public enum settings{SOMENOISE, ISWITHCONE,DEFAULT};
    public settings startwith;

    //Default cone  
    static Vector2 defAngle = new Vector2(0.5f,0.5f);
    const float  defHeight = 10;

    //Grid to generate quads by
    internal Vector3[,,] vertexGrid;

    //Quads
    internal List<Vector3> vertices;
    internal List<int> indicies;

    //Offset to World coordinates - no y component
    private Vector3 offset;

    // public bool[,,] grid;

    //sdf = new float[areaSize*x*areaSize*y*areaSize*z]; //??
    //In the future maybe use onedimensional array - compiler heavily optimized for one dimension
    internal float[,,] sdfgrid;

    internal Mesh mesh;
    internal MeshFilter filter;
    internal MeshRenderer meshRenderer;

    //Debug
    private int flag=0;

    //Regarding Schmitz adaptivity
    private readonly int MaxParticleIterations=50;
    float threshold = 0.001f;

    // Start is called before the first frame update
    void Start()
    {
        if(scale != 1){
            isAdaptive=false;
        }
        //Move  0,0,0 in the middle
        this.transform.position -= new Vector3(areaSize/2,0,areaSize/2);
        offset = gameObject.transform.position;

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        //inits
        mesh = new Mesh();
        
        //Generated boolean space
        // grid = new bool [areaSize +2, areaSize+2 , areaSize+2 ];

        vertexGrid = new Vector3[areaSize + 1, areaSize + 1, areaSize + 1];

        //Generated SDF space
        sdfgrid = new float [areaSize +2, areaSize+2 , areaSize+2 ];

        switch(startwith) {

            case settings.SOMENOISE:
                for (int x = 0; x <= areaSize+1; x++)
                {
                    for (int y = 0; y <= areaSize+1; y++)
                    {
                        for (int z = 0; z <= areaSize+1; z++){
                            sdfgrid[x,y,z] = perlinNoise3D(y+areaSize+0.9672f ,x+areaSize+0.342352f,z+areaSize+0.1352f) -0.5f;
                            if (y<floor){ 
                                sdfgrid[x,y,z] = -1;
                            }
                        }
                    }
                }
                break;
            case settings.ISWITHCONE:
                Vector3 pos;
                for ( int i = 0; i < (areaSize +2);i++ ) {
                    for ( int j = 0; j < (areaSize +2);j++ ) {
                        for ( int k = 0; k <(areaSize +2);k++ ) {
                            pos = new Vector3(i,j,k);
                            pos.y +=-8;
                            pos += offset;
                            sdfgrid[i,j,k] = sdConeExact(pos,defAngle,defHeight);
                            
                            if (j<floor){ 
                                sdfgrid[i,j,k] = -1;
                            }
                            // Debug.Log(sdfgrid[i,j,k]);
                            // Mathf.PerlinNoise(i/10+0.32342f+areaSize,j+0.568f+areaSize);//float.MaxValue;
                        }
                    }
                }
                break;
            default: //Just a floor
                for ( int i = 0; i < (areaSize +2);i++ ) {
                    for ( int j = 0; j < (areaSize +2);j++ ) {
                        for ( int k = 0; k <(areaSize +2);k++ ) {
                            if (j<floor){ 
                                sdfgrid[i,j,k] = -1+Math.Abs(j)*-1;
                            }else{
                                sdfgrid[i,j,k] = 1+Math.Abs(j);
                            }
                        }
                    }
                }      
                break; 
        }
        //Surface Quads
        indicies = new List<int>();
        this.vertices = new List<Vector3>();



        filter = gameObject.AddComponent<MeshFilter>();

        generateVertices(defAngle,defHeight);

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
                                vertices.Add(vertexGrid[x - 1, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 0, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {
                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 0, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 1, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

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
                                vertices.Add(vertexGrid[x - 1, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 1, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
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
                                vertices.Add(vertexGrid[x, y - 1, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {

                                vertices.Add(vertexGrid[x, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y - 1, z - 1]);
                                indicies.Add(vertices.Count - 1);
                            }
                        }
                    }
                }
            }
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Quads, 0);


        filter.mesh = mesh;
        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    private void generateVertices(Vector2 angle,float height)
    {
         for (int x = 0; x <= areaSize; x++)
        {
            for (int y = 0; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++)
                {
                    Vector3 vertex = find_vertex(x, y, z, angle, height);
                    if (vertex == Vector3.negativeInfinity)
                    {
                        continue;
                    }
                    vertexGrid[x, y, z] = vertex;
                    //add indicies?
                }
            }
        }
    }

    //Not in use
    private void generateVertices(Vector3 coord, Vector2 angle,float height)
    {

        Vector3 index = new Vector3(areaSize/2,0,areaSize/2);
        index += coord;

        // Optimization attempt
        int xStart =(int) (index.x-height-10);
        int yStart =(int) (index.y-height-5);
        int zStart =(int) (index.z-height-10);
        int xEnd = (int)  (index.x+height+10);
        int yEnd = (int)  (index.y+height+5);
        int zEnd = (int)  (index.z+height+10);

        for (int x = xStart; x <= xEnd; x++)
        {
            for (int y = yStart; y <= yEnd; y++)
            {
                for (int z = zStart; z <= zEnd; z++)
                {
                    Vector3 vertex = find_vertex(x, y, z, angle, height);
                    if (vertex == Vector3.negativeInfinity)
                    {
                        continue;
                    }
                    vertexGrid[x, y, z] = vertex;
                    //add indicies?
                }
            }
        }
    }


    private Vector3 find_vertex(float x, float y, float z, Vector2 angle, float height)
    {
        if (!isAdaptive)
        {
            Vector3 vert =  new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
            vert.Scale(new Vector3(scale,scale,scale));

            //Make vertex coordinates world coordinates
            if(scale < 1){
                vert -= offset; //add Offset since its negative
            }else if(scale > 1){
                vert += offset; 
            }
            //if scale 1 do nothing
            return vert;
        }
        //else{
        //    return new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
        //}
        threshold*= threshold;
        Vector3 vec = new Vector3(x, y, z);
        float[,,] edges = new float[2, 2, 2];

        // Vector3 pos;
        for (int j = 0; j < 2; j++)
        {
            for (int k = 0; k < 2; k++)
            {
                for (int l = 0; l < 2; l++)
                {
                    // pos = new Vector3(x + j, y + k, +l);
                    // pos.y +=-10;
                    // pos += offset;
                    edges[j, k, l] = sdfgrid[(int)x + j,(int) y + k,(int) z+l];//sdConeExact(pos, new Vector2(0.5f,0.5f),10);
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
            return new Vector3(x + 0.5f, y + 0.5f, z + 0.5f); 


        var normals = new List<Vector3>();
        foreach (Vector3 v in changes ){
            normals.Add(normal_from_Cone(v[0], v[1],v[2],angle,height));
            // Vector3 n = normal_from_Ball(v[0], v[1],v[2]);
            // normals.Add(n.x);
            // normals.Add(n.y);
            // normals.Add(n.z);
        }

        // changes.Count
        float count = changes.Count;
        //Schmitz adaptivity integrated of opensource repo
        //
        // start mass point
        // calculated by mean of intersection points
        Vector3 c = new Vector3();
        for (int i = 0; i < count; i++)
        {
            c += changes[i];
        }
        c /= count;

        for (int i = 0; i < MaxParticleIterations; i++)
        {
            // force that acts on mass
            Vector3 force = new Vector3();

             for (int j = 0; j < count; j++)
             {
                 Vector3 xPoint = changes[j];
                 Vector3 xNormal = normals[j];

                 force += xNormal * -1 * Vector3.Dot(xNormal, c - xPoint);
             }

             // dampen force
             float damping = 1 - (float)i / MaxParticleIterations;
             c += force * damping / count;

             if (force.sqrMagnitude < threshold)
             {
                 break;
             }
        }
        // flag++;
        // if(flag >= 5){
        //     Debug.Log("Vert."+vec.ToString()+" C:" +c.ToString());
        //     flag=int.MinValue;
        // }
        return c;
    }

    private float adapt(float v0, float v1)
    {
        //v0 and v1 are numbers of opposite sign. This returns how far you need to interpolate from v0 to v1 to get to 0
        //assert((v1 > 0) != (v0 > 0));
        if (!isAdaptive)
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

    //Approximate of normal
    private Vector3 normal_from_Ball(float x, float y, float z, float d = 0.01f)
    {
        return new Vector3(ball_function(x + d, y, z) - ball_function(x - d, y, z) / 2 / d,
                           ball_function(x, y + d, z) - ball_function(x, y - d, z) / 2 / d,
                           ball_function(x, y, z + d) - ball_function(x, y, z - d) / 2 / d).normalized;
    }

    private Vector3 normal_from_Cone6(float x, float y, float z, float d = 0.01f)
    {
        return new Vector3(sdConeExact(new Vector3(x + d, y, z),new Vector2(0.5f,0.5f),5 ) - sdConeExact(new Vector3(x - d, y, z),new Vector2(0.5f,0.5f),5) / 2 / d,
                           sdConeExact(new Vector3(x, y + d, z),new Vector2(0.5f,0.5f),5 ) - sdConeExact(new Vector3(x, y - d, z),new Vector2(0.5f,0.5f),5) / 2 / d,
                           sdConeExact(new Vector3(x, y, z + d),new Vector2(0.5f,0.5f),5 ) - sdConeExact(new Vector3(x, y, z - d),new Vector2(0.5f,0.5f),5) / 2 / d).normalized;
    }
    private Vector3 normal_from_Cone(float x, float y, float z,Vector2 angle, float height=5, float d = 0.01f)
    {
        return new Vector3(sdConeExact(new Vector3(x + d, y, z),angle,height ) , 
                           sdConeExact(new Vector3(x, y + d, z),angle,height ) , 
                           sdConeExact(new Vector3(x, y, z + d),angle,height )   ).normalized;
    }
    
    //Delegate 
    private Vector3 normal_from_F(Func<float, float, float, float> function, float x, float y, float z, float d = 0.01f)
    {
        return new Vector3(function(x + d, y, z) - function(x - d, y, z) / 2 / d,
                           function(x, y + d, z) - function(x, y - d, z) / 2 / d,
                           function(x, y, z + d) - function(x, y, z - d) / 2 / d).normalized;
    }

    bool isInside(int x, int y, int z)
    {
        return sdfgrid[x,y,z] < contour;        
    }


    public bool add_single(Vector3 coord, int material,bool remove=false){
       
        sbyte change =-1;
        if(remove)change=1;
        
        addedFlag=true;
        if(checkPosBounds(coord)){
            if(scale!= 1){
                coord=scaleCoordToIndex(coord);
            }else{
                coord.x +=Math.Abs(offset.x);
                coord.y +=Math.Abs(offset.y);
                coord.z +=Math.Abs(offset.z);
            }
            //Star
            sdfgrid[Math.Abs((int)coord.x), Math.Abs((int)coord.y) , Math.Abs((int)coord.z)]= change;
            sdfgrid[Math.Abs((int)coord.x), Math.Abs((int)coord.y) , Math.Abs((int)coord.z)+1]= change;
            sdfgrid[Math.Abs((int)coord.x), Math.Abs((int)coord.y) , Math.Abs((int)coord.z-1)]=change;
            sdfgrid[Math.Abs((int)coord.x-1), Math.Abs((int)coord.y) , Math.Abs((int)coord.z)]=change;
            sdfgrid[Math.Abs((int)coord.x), Math.Abs((int)coord.y)+1 , Math.Abs((int)coord.z)]=change;
            sdfgrid[Math.Abs((int)coord.x), Math.Abs((int)coord.y-1) , Math.Abs((int)coord.z)]=change;
            sdfgrid[Math.Abs((int)coord.x)+1, Math.Abs((int)coord.y) , Math.Abs((int)coord.z)]=change;
            //if( isAdaptive)generateVertices(coord,defAngle, defHeight);
            if( isAdaptive)generateVertices(defAngle, defHeight);
            return true;
        }
        return false;
    }

    private Vector3 scaleCoordToIndex(Vector3 coord)
    { 
        Camera cam = GameObject.FindObjectOfType<Camera>();
        coord -= cam.transform.forward;
        //Adjust to scale
        coord.x /=scale;
        coord.y /=scale;
        coord.z /=scale;
        coord += cam.transform.forward/scale;

        coord.x -=offset.x*scale;
        coord.y -=offset.y*scale;
        coord.z -=offset.z*scale;
        return coord;
    }

    public bool add_cone(Vector3 coord, float height, int material)
    {
        float aperture = Bodies.sands[material];
        addedFlag=true;
        // Debug.Log("executed at " + coord.ToString());
        if(!checkPosBounds(coord)) return false;
        Vector2 angle = new Vector2((float)Math.Sin(aperture),(float) Math.Cos(aperture));
        bool success = true;
        
        //index of  0,0,0
        // 40,0,40
        Vector3 index = new Vector3(areaSize/2,0,areaSize/2);
        index += coord;
        
        float xStart=0;
        float yStart=0;
        float zStart=0;
        float xEnd=areaSize;
        float yEnd=areaSize;
        float zEnd=areaSize;
        
        if(isConeOptimized){
            //Simplification(wrong): no cone has a bigger circumference than its height
        
            //Optimization attempt
            xStart = (index.x-height-10-1/aperture);//*scale;
            yStart = (index.y-height-5-1/aperture) ;//*scale; 
            zStart = (index.z-height-10-1/aperture);//*scale;
            xEnd =   (index.x+height+10+1/aperture);///scale;
            yEnd =   (index.y+height+5+1/aperture) ;///scale;
            zEnd =   (index.z+height+10+1/aperture);///scale;
            //optimize by limiting for loops by cones more exact dimension
            //http://mathcentral.uregina.ca/QQ/database/QQ.09.07/s/marija1.html
            // if(coord.y+height < areaSize) yLimit= coord.y+height;
            // double diameter = 2*height *Math.Tan(aperture);
            // if(coord.x+diameter < areaSize) xLimit=coord.x+ diameter;
            // if(coord.z+diameter < areaSize) zLimit= coord.z+diameter;

            if(xStart <0)xStart=0;
            if(yStart <0)yStart=0;
            if(zStart <0)zStart=0;

            if(xEnd >areaSize)xEnd=areaSize;
            if(yEnd >areaSize)yEnd=areaSize;
            if(zEnd >areaSize)zEnd=areaSize;
        

            // Debug.Log("Optimizationlogs: ");        
            // Debug.Log(xStart );
            // Debug.Log(yStart );
            // Debug.Log(zStart );
            // Debug.Log(xEnd );
            // Debug.Log(yEnd );
            // Debug.Log(zEnd );        
        }

        for (float x =xStart; x <=  xEnd; x=x+scale) 
        {
            for (float y = yStart; y <= yEnd; y=y+scale)
            {
                for (float z =zStart; z <=  zEnd; z=z+scale){
                    //reduces to near 0,0,0
                    //Vector3 position = new Vector3((x-coord.x/scale,(y-coord.y)/scale ,(z-coord.z)/scale);
                    Vector3 position = new Vector3(x-coord.x,y-coord.y ,z-coord.z);
                    
                    // float sx = (x*scale);
                    // float sy = (y*scale);
                    // float sz = (z*scale);
                    // Vector3 position = new Vector3(sx-coord.x,sy-coord.y ,sz-coord.z);

                    
                    //Cone Exact is always at zero so remove added offset
                    position += offset;
                    float cache = sdConeExact(position, angle, height);
                    if( cache < 1) {//to test 0.5
                        index = new Vector3(x,y,z);
                        // Debug.Log("in Cone at " + index.ToString());
                        // i don't want to scale offset
                        
                        if(scale!= 1){
                            index += offset;
                            index.x /=scale;
                            index.y /=scale;
                            index.z /=scale;
                            index -= offset;
                        
                            // index=scaleCoordToIndex(index);
                        }
                        // Debug.Log("Index:"+(int)(index.x)+" "+(int)(index.y)+" "+ (int)(index.z));
                        sdfgrid[(int)(index.x),(int)(index.y), (int)(index.z)] = cache;
                        
                        // index.x +=Math.Abs(offset.x*scale);
                        // index.y +=Math.Abs(offset.y*scale);
                        // index.z +=Math.Abs(offset.z*scale);

                        //Boolean option
                        // if(checkIndexBounds(index) ) {
                        //     grid[(int)(index.x),(int)(index.y), (int)(index.z)] =true;
                        // }
                    }
                }
            }
        }
        //if( isAdaptive)generateVertices(coord,angle, height);
        if( isAdaptive)generateVertices(angle, height);
        // Debug.Log("Is flag "+flag);
        // flag=0;
        return success;
    }

    private bool checkPosBounds(Vector3 coord){
        if(coord.x >(areaSize/2) || coord.y > (areaSize/2) || coord.z >(areaSize/2) ||
           coord.x <(-areaSize/2) || coord.y < (-areaSize/2) || coord.z <(-areaSize/2) ){
            Debug.Log("Pos out of bounds");
            return false; //out of bounds
        }
        return true;
    }

    private bool checkIndexBounds(Vector3 coord){
        if(coord.x >(areaSize) || coord.y > (areaSize) || coord.z >(areaSize) ||
          (coord.x <0 || coord.y < 0) || coord.z <0 ) {
            Debug.Log("Index out of bounds");
            return false; //out of bounds
        }
        return true;
    }


    // Update is called once per frame
     void Update()
     {
        regenerateMesh();
        if(isGravity){
            gravity(); 
        }
        if(isPretty){
            generateVertices(defAngle,defHeight);
        }else if( isCheapPretty && Time.frameCount%30 ==0 && addedFlag){
            //use a general cone to render the sdfgrid prettier
            generateVertices(defAngle,defHeight);
            addedFlag=false;
        }
     }

    //Optimize by dynamically adapting areasize to be changed by position and dimension of new cone
    //Todo think about vertexgrid aswell
    public void gravity()
    {
          for (int x = 0; x <= areaSize; x++) 
        {
            for (int y = 1; y <= areaSize; y++)
            {
                for (int z = 0; z <= areaSize; z++){
                    if(sdfgrid[x,y,z] < 0){ //isSand / Matter
                        if(!(sdfgrid[x,y-1,z] < 0)){
                            //No ground
                            sdfgrid[x,y-1,z] = sdfgrid[x,y,z];
                            sdfgrid[x,y,z] = 2;

                            addedFlag=true;
                            //vertexGrid
                            // vertexGrid[x,y-1,z] = vertexGrid[x,y,z];
                            // vertexGrid[x,y,z] = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                        }else if(sdfgrid[x+1,y-1,z] > 0 && (sdfgrid[x,y+1,z] > 0) ){ 

                            sdfgrid[x+1,y-1,z] = sdfgrid[x,y,z];;
                            sdfgrid[x,y,z] = 2;
                            addedFlag=true;

                            // vertexGrid[x+1,y-1,z] = vertexGrid[x,y,z];
                            // vertexGrid[x,y,z] = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                        }else if(x!=0 && !(sdfgrid[x-1,y-1,z] < 0) && (sdfgrid[x,y+1,z] > 0)){

                            sdfgrid[x-1,y-1,z] = sdfgrid[x,y,z];;
                            sdfgrid[x,y,z] = 2;

                            addedFlag=true;
                            // vertexGrid[x-1,y-1,z] = vertexGrid[x,y,z];
                            // vertexGrid[x,y,z] = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                        }else if(!(sdfgrid[x,y-1,z+1] < 0) && (sdfgrid[x,y+1,z] > 0)){

                            sdfgrid[x,y-1,z+1] = sdfgrid[x,y,z];;
                            sdfgrid[x,y,z] = 2;

                            addedFlag=true;
                            // vertexGrid[x,y-1,z+1] = vertexGrid[x,y,z];
                            // vertexGrid[x,y,z] = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                        }else if(x!=0 && z !=0 && !(sdfgrid[x-1,y-1,z-1] < 0) && (sdfgrid[x,y+1,z] > 0)){
                            sdfgrid[x-1,y-1,z-1] = sdfgrid[x,y,z];;
                            sdfgrid[x,y,z] = 2;

                            addedFlag=true;
                            // vertexGrid[x-1,y-1,z-1] = vertexGrid[x,y,z];
                            // vertexGrid[x,y,z] = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                            //maybe change < 0 to < contour?
                        }
                        /* else if( y==0 || sdfgrid[x,y-1,z] < 0 ){
                            //ground below do nothing
                        } */
                        ////
                    }
                        // flag=1;
                }
            }
        }
        
    }
    
    //To test regenerate only a specific area, depending on input location and dimension
    public void regenerateMesh()
    {
        // If one point is inside and not all points are inside place add vertices for Quad -
        // another way to phrase it is just show the edges where a sign switch happens
        for (int x = 0; x < areaSize; x++) 
        {
            for (int y = 0; y < areaSize; y++)
            {
                for (int z = 0; z < areaSize; z++)
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
                                vertices.Add(vertexGrid[x - 1, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 0, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {
                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 0, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 1, y - 1, z]);
                                indicies.Add(vertices.Count - 1);

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
                                vertices.Add(vertexGrid[x - 1, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {

                                vertices.Add(vertexGrid[x - 1, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x - 1, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
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
                                vertices.Add(vertexGrid[x, y - 1, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);

                                vertices.Add(vertexGrid[x, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                            }
                            else
                            {
                                vertices.Add(vertexGrid[x, y - 1, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y, z - 1]);
                                indicies.Add(vertices.Count - 1);
                                vertices.Add(vertexGrid[x, y - 1, z - 1]);
                                indicies.Add(vertices.Count - 1);
                            }
                        }
                    }
                }
            }
        }
        

        mesh.SetIndices(indicies.ToArray(), MeshTopology.Quads, 0);
        mesh.vertices = vertices.ToArray();
        filter.mesh = mesh;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        
        //Without Clearing you can only place a certain amount of cones
        //it seems there is a maximum to the vertices you can add, no error is displayed when it happens!
        vertices.Clear();
        indicies.Clear();
    }
}
