using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using System.Collections.Specialized;
using System;
using System.Security.Cryptography;

static class Extensions
{
    public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }
}
public class Face
{
    
    public Vector3[] vert;
    public int[] tri;
    bool valid = false;
    public bool isValid()
    {
        return valid;
    }

    public Face(Vector3[] vertices)
    {
       if (vertices.Length >= 3)
        {
            vert = new Vector3[vertices.Length];
            vertices.CopyTo(vert, 0);
            tri = new int[3 * (vertices.Length - 2)];
            for (int i = 1; i < vertices.Length - 1; i++)//1-3
            {
                tri[i * 3 - 3] = 0;
                tri[i * 3 - 2] = i;
                tri[i * 3 - 1] = i + 1;
            }
            valid = true;
        }
        else 
            valid = false;
    }
}
public class Cut : MonoBehaviour
{
    static int ct = 0;
    public float maxDist = 999999;
    List<Face> Faces;
    Mesh mesh;
    public bool isSphere;
    public String prphabName;
    public float massPerCubicMeter;
    Vector3[] m;
    Vector3 newcenter;
    int[] tri;
    bool facesSet = false;
    // Start is called before the first frame update
    void BuildFaces()
    {
        Faces = new List<Face>();
        Vector3 oldCenter = new Vector3(0.5f, 0.5f, 0.5f);
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 a = new Vector3(mesh.vertices[mesh.triangles[i]].x, mesh.vertices[mesh.triangles[i]].y, mesh.vertices[mesh.triangles[i]].z);
            Vector3 b = new Vector3(mesh.vertices[mesh.triangles[i+1]].x, mesh.vertices[mesh.triangles[i+1]].y, mesh.vertices[mesh.triangles[i+1]].z);
            Vector3 c = new Vector3(mesh.vertices[mesh.triangles[i+2]].x, mesh.vertices[mesh.triangles[i+2]].y, mesh.vertices[mesh.triangles[i+2]].z);
            Faces.Add(new Face(new Vector3[] {a,b,c }));
        }
    }
    void build(List<Face> newFaces, Vector3 newCenter)
    {
        newcenter = newCenter;
        Faces = newFaces;
        rebuild();
    } 
   /* void setFaces()
    {
        float a = -0.5f;
        float b = 0.5f;
        Faces = new List<Face>();
        Faces.Add(new Face(new Vector3[] { new Vector3(a, a, a), new Vector3(a, a, b), new Vector3(a, b, b), new Vector3(a, b, a) }));
        Faces.Add(new Face(new Vector3[] { new Vector3(a, a, b), new Vector3(b, a, b), new Vector3(b, b, b), new Vector3(a, b, b) }));
        Faces.Add(new Face(new Vector3[] { new Vector3(b, a, b), new Vector3(b, a, a), new Vector3(b, b, a), new Vector3(b, b, b) }));
        Faces.Add(new Face(new Vector3[] { new Vector3(b, a, a), new Vector3(a, a, a), new Vector3(a, b, a), new Vector3(b, b, a) }));
        Faces.Add(new Face(new Vector3[] { new Vector3(a, a, a), new Vector3(b, a, a), new Vector3(b, a, b), new Vector3(a, a, b) }));
        Faces.Add(new Face(new Vector3[] { new Vector3(b, b, a), new Vector3(a, b, a), new Vector3(a, b, b), new Vector3(b, b, b) }));
        newcenter = new Vector3(0, 0, 0);
        facesSet = true;
    }*/
    void rebuild()
    {
        
        int vertCount = 0;
        int triIndCount = 0;
        List<Face> Faces2 = new List<Face>();
        foreach (Face face in Faces)
        {
            if (face.isValid())
            {
                vertCount += face.vert.Length;
                triIndCount += face.tri.Length;
                Faces2.Add(face);
            }
        }
        Faces = Faces2;
        if (Faces2.Count < 4)
            Destroy(this.gameObject);
        int currentVert = 0;
        int currentTri = 0;
        m = new Vector3[vertCount];
        tri = new int[triIndCount];
        foreach (Face face in Faces)
        {
            face.vert.CopyTo(m, currentVert);
            for (int i = 0; i < face.tri.Length;i++)
            {
                tri[currentTri + i] = face.tri[i] + currentVert;
            }
            currentVert += face.vert.Length;
            currentTri += face.tri.Length;

        }
        mesh.Clear();
        mesh.vertices = m;
        mesh.triangles = tri;

 
        Vector2[] uvs = new Vector2[m.Length];
        float volume =0;
        for (int i = 0; i < tri.Length; i += 3)
        {
            if (!isSphere)
            {
                if ((m[tri[i]].x == 0.5f && m[tri[i + 1]].x == 0.5f && m[tri[i + 2]].x == 0.5f) || (m[tri[i]].x == -0.5f && m[tri[i + 1]].x == -0.5f && m[tri[i + 2]].x == -0.5f))
                {
                    uvs[tri[i]] = new Vector2((m[tri[i]].z + 1.5f) / 2f, (m[tri[i]].y + 1.5f) / 2f);
                    uvs[tri[i + 1]] = new Vector2((m[tri[i + 1]].z + 1.5f) / 2f, (m[tri[i + 1]].y + 1.5f) / 2f);
                    uvs[tri[i + 2]] = new Vector2((m[tri[i + 2]].z + 1.5f) / 2f, (m[tri[i + 2]].y + 1.5f) / 2f);
                }
                else if ((m[tri[i]].z == 0.5f && m[tri[i + 1]].z == 0.5f && m[tri[i + 2]].z == 0.5f) || (m[tri[i]].z == -0.5f && m[tri[i + 1]].z == -0.5f && m[tri[i + 2]].z == -0.5f))
                {
                    uvs[tri[i]] = new Vector2((m[tri[i]].x + 1.5f) / 2f, (m[tri[i]].y + 1.5f) / 2f);
                    uvs[tri[i + 1]] = new Vector2((m[tri[i + 1]].x + 1.5f) / 2f, (m[tri[i + 1]].y + 1.5f) / 2f);
                    uvs[tri[i + 2]] = new Vector2((m[tri[i + 2]].x + 1.5f) / 2f, (m[tri[i + 2]].y + 1.5f) / 2f);
                }
                else
                {
                    uvs[tri[i]] = new Vector2((m[tri[i]].x + 0.5f) / 2f, (m[tri[i]].z + 1.5f) / 2f);
                    uvs[tri[i + 1]] = new Vector2((m[tri[i + 1]].x + 0.5f) / 2f, (m[tri[i + 1]].z + 1.5f) / 2f);
                    uvs[tri[i + 2]] = new Vector2((m[tri[i + 2]].x + 0.5f) / 2f, (m[tri[i + 2]].z + 1.5f) / 2f);
                }
            }
            else
            {
                float mult = 0.95f;
              float radius = 0.49f;
                Vector3 center = (m[tri[i]]+ m[tri[i + 1]]+ m[tri[i + 2]])/3;
                float a = Mathf.Atan(m[tri[i]].x / m[tri[i]].z) / Mathf.PI * mult;
                float b = Mathf.Atan(m[tri[i + 1]].x / m[tri[i + 1]].z) / Mathf.PI * mult;
                float c = Mathf.Atan(m[tri[i + 2]].x / m[tri[i + 2]].z) / Mathf.PI * mult;
                if (center.magnitude > radius)
                   {

                    uvs[tri[i]] = new Vector2((a + 1.5f) / 2f, (m[tri[i]].y + 1.5f) / 2f);
                    uvs[tri[i + 1]] = new Vector2((b + 1.5f) / 2f, (m[tri[i + 1]].y + 1.5f) / 2f);
                    uvs[tri[i + 2]] = new Vector2((c + 1.5f) / 2f, (m[tri[i + 2]].y + 1.5f) / 2f);
                }
                else
                {
                    uvs[tri[i]] = new Vector2(( + 0.5f) / 2f, (1.5f+ m[tri[i]].magnitude* mult) / 2f);
                    uvs[tri[i + 1]] = new Vector2(( +0.4f) / 2f, (1.5f - m[tri[i+1]].magnitude* mult) / 2f);
                    uvs[tri[i + 2]] = new Vector2((1-m[tri[i + 2]].magnitude* mult) / 2f, (1.5f) / 2f);
                }
            }
            volume+=getTriVolume(m[tri[i]], m[tri[i + 1]], m[tri[i + 2]]);
        }
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        float tmpmass = massPerCubicMeter * volume * transform.localScale.x * transform.localScale.y * transform.localScale.z;
        GetComponent<Rigidbody>().mass = tmpmass > 0.001f ? tmpmass : 0.001f;


        this.gameObject.GetComponent<MeshCollider>().sharedMesh = null;
        this.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;


    }
    float getTriVolume(Vector3 a, Vector3 b, Vector3 c)
    {
        float sideA = (a - b).magnitude;
        float sideB = (b - c).magnitude;
        float sideC = (c - a).magnitude;
        float p = (sideA + sideB + sideC) / 2;
        float triSurf = Mathf.Sqrt(p*(p- sideA) * (p - sideB) * (p - sideC));
            Plane plane = new Plane(GetNormal(a,b,c), a);
        float h = Mathf.Abs(plane.GetDistanceToPoint(newcenter));
        return triSurf * h / 3f;
    }
    public void process(Vector3 planePoint, Vector3 planeNormal)
    {
        
        cut(planePoint, planeNormal);
        rebuild();
    }
    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (!facesSet)
        {
            BuildFaces();
            rebuild();
        }   
    }
     Vector3 lineIntersection(Vector3 planePoint, Vector3 planeNormal, Vector3 linePoint, Vector3 lineDirection)
    {
        if (Vector3.Dot(planeNormal.normalized,lineDirection.normalized)==0)
        {
            return new Vector3(float.MaxValue,float.MaxValue,float.MaxValue);
        }

        float t = (Vector3.Dot(planeNormal, planePoint) - Vector3.Dot(planeNormal, linePoint)) / Vector3.Dot(planeNormal, lineDirection.normalized);
        return linePoint+lineDirection.normalized*t;
    }

    Vector3 findDotInside(Vector3 planePoint, Vector3 planeNormal, float multy)
    {
        Plane plane = new Plane(planeNormal, planePoint);
        float dist = plane.GetDistanceToPoint(new Vector3(0,0,0));
        Vector3 dotOnPlane = -planeNormal * dist;
            return dotOnPlane * multy;
    }
    void cut(Vector3 planePoint, Vector3 planeNormal)
    {
        ct++; 
        m = mesh.vertices;
        tri = mesh.triangles;
        Vector3[] m2 = mesh.vertices;
        List<Face> Faces2 = new List<Face>();
        List<Face> Faces3 = new List<Face>();
        List<Vector3> cutFace = new List<Vector3>();

        foreach (Face f in Faces)
        {
            List<Vector3> keep = new List<Vector3>();
            List<Vector3> cutted = new List<Vector3>();
            bool requireInsert = true;
            bool requireInsertVoid = false;

            Vector3 intersection1 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 intersection2 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            bool inside = false;

            for (int i = 0; i < f.vert.Length; i++)
            {
                if (i < f.vert.Length - 1)
                {
                    Vector3 interA = lineIntersection(planePoint, planeNormal, f.vert[i], f.vert[i + 1] - f.vert[i]);
                    if (interA.x < maxDist)
                    {
                        if ((f.vert[i] - interA).magnitude + (f.vert[i + 1] - interA).magnitude <= (f.vert[i] - f.vert[i + 1]).magnitude * 1.001f)
                        {

                            if (intersection1.x > maxDist)
                                intersection1 = interA;
                            else
                                intersection2 = interA;
                        }
                    }
                }
                else
                {
                    Vector3 interA = lineIntersection(planePoint, planeNormal, f.vert[i], f.vert[0] - f.vert[i]);
                    if (interA.x < maxDist)
                        if ((f.vert[i] - interA).magnitude + (f.vert[0] - interA).magnitude <= (f.vert[i] - f.vert[0]).magnitude * 1.001f)
                        {
                            if (intersection1.x > maxDist)
                                intersection1 = interA;
                            else
                                intersection2 = interA;
                        }
                }
            }

            for (int i = 0; i < f.vert.Length; i++)
            {
                if (Vector3.Dot(f.vert[i] - planePoint, planeNormal) > 0)
                {
                    if (requireInsertVoid)
                    {
                        if (intersection1.x < maxDist && intersection2.x < maxDist)
                        {
                            cutted.Add(intersection1);
                            cutted.Add(intersection2);

                        }
                        requireInsertVoid = false;
                    }
                    keep.Add(f.vert[i]);
                    if (i == 0)
                        inside = true;
                }
                else
                {
                    if (requireInsert)
                    {
                        if (intersection1.x < maxDist && intersection2.x < maxDist)
                        {
                            if (inside)
                            {
                                keep.Add(intersection1);
                                keep.Add(intersection2);
                            }
                            else
                            {
                                keep.Add(intersection2);
                                keep.Add(intersection1);
                            }
                        }
                        //insert both intersections here to keep;
                        requireInsert = false;
                    }
                    cutted.Add(f.vert[i]);
                    if (i == 0)
                        requireInsertVoid = true;
                }
            }
            if (intersection1.x < maxDist && intersection2.x < maxDist)
            {
                if (inside)
                {
                    cutted.Add(intersection2);
                    cutted.Add(intersection1);
                }

                cutFace.Add(intersection1);
                cutFace.Add(intersection2);
            }

            for (int i = keep.Count - 1; i >= 1; i--)
            {
                bool marked = false;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (Mathf.Abs((keep[i] - keep[j]).magnitude) < 0.001f)
                        marked = true;
                }
                if (marked)
                    keep.RemoveAt(i);
            }
            if (keep.Count > 2)
            {
                Face keepFace = new Face(keep.ToArray());
                if (keepFace.isValid())
                    Faces2.Add(keepFace);
            }

            for (int i = cutted.Count - 1; i >= 1; i--)
            {
                bool marked = false;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (Mathf.Abs((cutted[i] - cutted[j]).magnitude) < 0.001f)
                        marked = true;
                }
                if (marked)
                    cutted.RemoveAt(i);
            }
            if (cutted.Count > 2)
            {

                Face cuttedFace = new Face(cutted.ToArray());
                if (cuttedFace.isValid())
                    Faces3.Add(cuttedFace);
            }
        }

        for (int i = cutFace.Count - 1; i >= 1; i--)
        {
            bool marked = false;
            for (int j = i - 1; j >= 0; j--)
            {
                if (Mathf.Abs((cutFace[i] - cutFace[j]).magnitude) < 0.0003f)
                {
                    marked = true;
                }
            }
            if (marked)
                cutFace.RemoveAt(i);
        }
     
        if (cutFace != null && cutFace.Count > 2)
        {
            Face cuttedFace = new Face(cutFace.ToArray());
            Face newFace = new Face(cutFace.ToArray());           
            Faces2.Add(newFace);
            Faces3.Add(cuttedFace);
        }
        Faces = Faces2;

        Vector3 keepCenter = new Vector3(0, 0, 0);
        int count = 0;
        foreach (Face f in Faces)
        {
            foreach (Vector3 v in f.vert)
                keepCenter += v;
            count += f.vert.Count();
        }
        keepCenter = keepCenter / count;
        newcenter = keepCenter;
        count = 0;
        List<Face> keepFaces = new List<Face>();
        foreach (Face f in Faces)
        {
            Vector3 tmpNorm = new Vector3(0, 0, 0);
            for (int i = 0; i < f.vert.Count(); i++)
                for (int j = i + 1; j < f.vert.Count(); j++)
                    for (int k = j + 1; k < f.vert.Count(); k++)
                    {
                        tmpNorm = GetNormal(f.vert[i], f.vert[j], f.vert[k]);

                        if (tmpNorm.magnitude > 0.99f)
                        {
                            i = f.vert.Count();
                            j = f.vert.Count();
                            k = f.vert.Count();
                        }
                    }
            Vector3 center = new Vector3(0, 0, 0);
            foreach (Vector3 v in f.vert)
                center += v;
            center = center / f.vert.Count();
            if (Vector3.Dot(tmpNorm, (center - keepCenter)) < 0)
                 keepFaces.Add( new Face(sortVertices(f.vert.ToList<Vector3>(), -tmpNorm).ToArray()));
            else
                keepFaces.Add(new Face(sortVertices(f.vert.ToList<Vector3>(), tmpNorm).ToArray()));

        }
        Faces = keepFaces;
        if (Faces.Count < 4)
            Destroy(this.gameObject);

        Vector3 cutCenter = new Vector3(0, 0, 0);
        foreach (Face f in Faces3)
        {
            foreach (Vector3 v in f.vert)
                cutCenter += v;
            count += f.vert.Count();          
        }
        cutCenter = cutCenter / count;;
        List<Face> cutFaces = new List<Face>();
        foreach (Face f in Faces3)
        {
            Vector3 tmpNorm = new Vector3(0, 0, 0);
            for (int i = 0; i < f.vert.Count(); i++)
                for (int j = i + 1; j < f.vert.Count(); j++)
                    for (int k = j + 1; k < f.vert.Count(); k++)
                    {
                        tmpNorm = GetNormal(f.vert[i], f.vert[j], f.vert[k]);

                        if (tmpNorm.magnitude > 0.99f)
                        {
                            i = f.vert.Count();
                            j = f.vert.Count();
                            k = f.vert.Count();
                        }
                    }
            Vector3 center = new Vector3(0, 0, 0);
            foreach (Vector3 v in f.vert)
                center += v;
            center = center / f.vert.Count();
            if (Vector3.Dot(tmpNorm, (center - cutCenter)) < 0)
                cutFaces.Add(new Face(sortVertices(f.vert.ToList<Vector3>(), -tmpNorm).ToArray()));
            else
                cutFaces.Add(new Face(sortVertices(f.vert.ToList<Vector3>(), tmpNorm).ToArray()));
        }
        

        if (cutFaces.Count > 3)
        {
            GameObject newPart = Instantiate(Resources.Load<GameObject>(prphabName), this.gameObject.transform.position, this.gameObject.transform.rotation);
            newPart.transform.localScale = this.gameObject.transform.localScale;
            newPart.GetComponent<Cut>().build(cutFaces, cutCenter);
        }


    }
    Vector3 GetNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 a = v1 - v2;
        Vector3 b = v1 - v3;
        return Vector3.Cross(a.normalized, b.normalized).normalized;
    }

    List<Vector3> sortVertices(List<Vector3> input, Vector3 norm)
    {
        List<Vector3> ret = new List<Vector3>();
        Vector3 center = new Vector3(0, 0, 0);
        int count = 0;
        foreach (Vector3 v in input)
        {
            count++;
            center+=v;
        }
        center = center / count;

        Vector3 first = (input.First()-center).normalized;
        List<float> cosplus = new List<float>();
        List<float> cosminus = new List<float>();
        List<Vector3> retplus = new List<Vector3>();
        List<Vector3> retminus = new List<Vector3>();
        Vector3 orientation = Vector3.Cross(norm, first);

        foreach (Vector3 v in input)
        {
            if (input.IndexOf(v) != 0)
            {
                Vector3 arrow = (v - center).normalized;
                if (Vector3.Dot(arrow, orientation) > 0)
                {
                    cosplus.Add(Vector3.Dot(arrow, first));
                    retplus.Add(v);
                }
                else
                {
                    cosminus.Add(Vector3.Dot(arrow, first));
                    retminus.Add(v);
                }
            }
        }
        ret.Add(input.First());
        for (int i = 0; i < cosplus.Count; i++)
        {
            int  index = cosplus.IndexOf(cosplus.Max());
            if(cosplus[index]>-999)
                ret.Add(retplus[index]);
            cosplus[index]=-99999;
        }
        for (int i = 0; i < cosminus.Count; i++)
        {
            int index = cosminus.IndexOf(cosminus.Min());
            if (cosminus[index] <999)
                ret.Add(retminus[index]);
            cosminus[index] = 99999;
        }

        return ret;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
