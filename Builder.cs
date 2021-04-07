using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{
    Mesh mesh;
    public Vector3[] m;
    Vector3[] n;
    int[] tri;
    public int getVertCount()
    {
        return m.Length;
    }
    // Start is called before the first frame update
    void Awake()
    {
       mesh = GetComponent<MeshFilter>().mesh;
    }
    public void clear()
    {
        m = null;
        tri = null;  
        mesh.triangles = tri;
        mesh.vertices = m;
    }
    public void Destr()
    {
        Destroy(mesh);
    }
    public void addGeometry(Vector3[] newVertices, int[] newTris, Vector3 pos)
    {

        //if(mesh =null)
       //     mesh = GetComponent<MeshFilter>().mesh;
        //append vertices
        int mL = m == null ? 0 : m.Length;

        Vector3[] z = new Vector3[mL + newVertices.Length];
        if(m!=null)
            m.CopyTo(z, 0);
        for (int i = 0; i < newVertices.Length; i++)
        {
            z[mL + i] = new Vector3(newVertices[i].x+pos.x, newVertices[i].y+pos.y, newVertices[i].z+pos.z);
        }
        m = new Vector3[z.Length];
        z.CopyTo(m, 0);

        //append tris
        int tL = tri == null ? 0 : tri.Length;
        int[] t= new int[tL + newTris.Length];
        if (tri != null)
            tri.CopyTo(t, 0);
        for(int i=0;i<newTris.Length;i++)
        {
            t[tL + i] = newTris[i] + mL;
        }
        tri = new int[t.Length];
        t.CopyTo(tri, 0);
    }
    public void setNormals(Vector3[] normals)
    {
        n = normals;
    }
    public void finalizeGeometry(Vector3 chunkCoord, int chunksize)
    {
        if(mesh==null)
            mesh = GetComponent<MeshFilter>().mesh;
        // Debug.Log(m.ToString()  + "|||||" + tri[tri.Length - 1].ToString() + "____" + tri.Length.ToString());
        if (m != null)
        {
            //  Debug.Log(m.Length.ToString());
            mesh.Clear();
            fix_tri();
            mesh.vertices = m;
            mesh.triangles = tri;
        }
        else
        {
            //Debug.Log("0");
            mesh.triangles = tri;
            mesh.vertices = m;
        }
        /*Vector2[] uvs = new Vector2[m.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(1f/((float)uvs.Length)*(float)i, 0.1f+0.5f*(float)(i%2));
        }
        mesh.uv = uvs;*/


        //mesh.RecalculateNormals();
        mesh.normals = n;
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        Vector2[] uvs = new Vector2[m.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(m[i].x + m[i].z, m[i].y+chunkCoord.y*chunksize);
        }
        mesh.uv = uvs;
        this.gameObject.GetComponent<MeshCollider>().sharedMesh = null;
        this.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;

    }
	
    void fix_tri()
    {
        if(tri!=null)
        for (int i = 0; i < (tri.Length); i+=3)
        {
            int a = tri[i];//tri - triangle vertex array
            int b = tri[i + 1];
            int c = tri[i + 2];
            Vector3 face = Vector3.Cross(m[a] - m[b], m[a] - m[c]); //m - vertex coord array
            Vector3 norm = (n[a] + n[b] + n[c]); //n - vertex normals array
            if (Vector3.Dot(face.normalized, norm.normalized) < 0)
            {
                tri[i + 1] = c;
                tri[i + 2] = b;
                    
            }
        }
    }
}
