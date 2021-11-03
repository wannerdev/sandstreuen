using System;
using UnityEngine;


public static class Bodies
{
    //Angles
    //public const int sandDry= 34, sandWaStart=15, sandWaEnd=30,sandWet=45;
    public enum types{SANDDRY, SANDWATERSTART, SANDWATEREND,SANDWET};
    public static string[] sandname= {"sandDry", "sandWaterStart", "sandWaterEnd","sandWet"};
    public static float[] sands= { 0.56f, 0.26f, 0.5f,0.7f};

    //Credit  iquilezles
    //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
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

    //Credit  iquilezles
    //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
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

    // Adapted from
    // https://answers.unity.com/questions/938178/3d-perlin-noise.html
    public static float perlinNoise3D(float x, float y, float z)
    {
     float xy = Mathf.PerlinNoise(x, y);
     float xz = Mathf.PerlinNoise(x, z);
     float yz = Mathf.PerlinNoise(y, z);
     float yx = Mathf.PerlinNoise(y, x);
     float zx = Mathf.PerlinNoise(z, x);
     float zy = Mathf.PerlinNoise(z, y);
 
     return (xy + xz + yz + yx + zx + zy) / 6;
    }
    static float perlin3DFixed(float a, float b)
    {
        return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a, b));
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
