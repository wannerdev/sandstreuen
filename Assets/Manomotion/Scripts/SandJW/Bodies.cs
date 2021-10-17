using System;
using UnityEngine;


public static class Bodies
{

    static public float sdConeShort(Vector3 p, Vector2 c, float h)
    {
        float q = (float)Math.Sqrt(p.x * p.x + p.z * p.z);
        return Math.Max(Vector2.Dot(c, new Vector2(q, p.y)), -h - p.y);
    }

    //Credit  iquilezles
    //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
    static public float sdConeExact(Vector3 p, Vector2 c, float h)
    {
        // c is the sin/cos of the angle, h is height
        // Alternatively pass q instead of (c,h),
        // which is the point at the base in 2D
        Vector2 q = h * new Vector2(c.x / c.y, -1.0f);

        Vector2 w = new Vector2((float)Math.Sqrt(p.x * p.x + p.z * p.z), p.y);
        Vector2 a = w - q * Mathf.Clamp(Vector2.Dot(w, q) / Vector2.Dot(q, q), 0.0f, 1.0f);
        float clam = Mathf.Clamp((w.x / q.x), 0.0f, 1.0f);
        Vector2 cache = new Vector2(clam, 1.0f);
        Vector2 b = w - q * cache;

        float k = Math.Sign(q.y);
        float d = Math.Min(Vector2.Dot(a, a), Vector2.Dot(b, b));
        float s = Math.Max(k * (w.x * q.y - w.y * q.x), k * (w.y - q.y));
        return (float)(Math.Sqrt(d) * Math.Sign(s));
    }

static public float sdConeExact(Vector3 p, Vector2 q)
    {
        // c is the sin/cos of the angle, h is height
        // Alternatively pass q instead of (c,h),
        // which is the point at the base in 2D

        Vector2 w = new Vector2((float)Math.Sqrt(p.x * p.x + p.z * p.z), p.y);
        Vector2 a = w - q * Mathf.Clamp(Vector2.Dot(w, q) / Vector2.Dot(q, q), 0.0f, 1.0f);
        float clam = Mathf.Clamp((w.x / q.x), 0.0f, 1.0f);
        Vector2 cache = new Vector2(clam, 1.0f);
        Vector2 b = w - q * cache;

        float k = Math.Sign(q.y);
        float d = Math.Min(Vector2.Dot(a, a), Vector2.Dot(b, b));
        float s = Math.Max(k * (w.x * q.y - w.y * q.x), k * (w.y - q.y));
        return (float)(Math.Sqrt(d) * Math.Sign(s));
    }



    /** Credit @https://stackoverflow.com/questions/10768142/verify-if-point-is-inside-a-cone-in-3d-space
    * @param x coordinates of point to be tested 
    * @param t coordinates of apex point of cone
    * @param b coordinates of center of basement circle
    * @param aperture in radians
        Maybe move into cone
    */
    static public bool isLyingInCone(float[] x, float[] t, float[] b,
                                         float aperture)
    {
        // This is for our convenience
        float halfAperture = aperture / 2.0f;
        // Vector pointing to X point from apex
        float[] apexToXVect = dif(t, x);
        // Vector pointing from apex to circle-center point.
        float[] axisVect = dif(t, b);
        // X is lying in cone only if it's lying in 
        // infinite version of its cone -- that is, 
        // not limited by "round basement".
        // We'll use dotProd() to 
        // determine angle between apexToXVect and axis.
        bool isInInfiniteCone = dotProd(apexToXVect, axisVect)
                                / magn(apexToXVect) / magn(axisVect)
                                    >
                                // We can safely compare cos() of angles 
                                // between vectors instead of bare angles.
                                Math.Cos(halfAperture);
        if (!isInInfiniteCone) return false;

        // X is contained in cone only if projection of apexToXVect to axis
        // is shorter than axis. 
        // We'll use dotProd() to figure projection length.
        bool isUnderRoundCap = dotProd(apexToXVect, axisVect)
                                / magn(axisVect)
                                    <
                                magn(axisVect);
        return isUnderRoundCap;
    }

    static public float dotProd(float[] a, float[] b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
    }

    static public float[] dif(float[] a, float[] b)
    {
        return (new float[]{
                a[0]-b[0],
                a[1]-b[1],
                a[2]-b[2]
        });
    }

    static public float magn(float[] a)
    {
        return (float)(Math.Sqrt(a[0] * a[0] + a[1] * a[1] + a[2] * a[2]));
    }

    

    static public float ball_function(float x, float y, float z)
    {
        //move 
        x -= 5;
        y -= 5;
        z -= 5;
        return 2.5f - Mathf.Sqrt(x * x + y * y + z * z);
    }

    static public double circle_function(float x, float y, float z)
    {
        //move circle to the right
        x = x - 5;
        y = y - 5;
        return 2.5 - Mathf.Sqrt(x * x + y * y);
    }
    
 public struct Cone{
        public float[] tip;

        public float[] position; //bottom
        public float height;

        public float aperture;

        public Cone(float[] tip,float[] position, float height = 4f, float aperture = 100)
        {
            this.position = position;
            this.height = height;
            this.tip = tip;
            this.aperture = aperture;
        }
 }
}
