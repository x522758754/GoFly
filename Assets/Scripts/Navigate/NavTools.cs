using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unity调试Recast工具
public class NavTools : MonoBehaviour
{
    
    DbgRenderMesh m_dbgRenderMesh = new DbgRenderMesh();

    public void LoadMap(string path)
    {
        bool ret = RecastHelper.load_map_bin(path);
        if (!ret)
        {
            Debug.LogWarning("load map bin failed");
            return;
        }


        int triCount = RecastHelper.get_tri_vert_count();
        ///1 tri => 3 verts, 1 verts = 1 pos(vec3) => 3 float
        float[] vertPos = new float[triCount * 9];
        RecastHelper.get_tri_vert_pos(vertPos);
        Color col = new Color();

        CreateDbgRenderMesh();
        for (int i=0; i != triCount; ++i)
        {
            Vector3 a = new Vector3(vertPos[i * 9 + 0], vertPos[i * 9 + 1], vertPos[i * 9 + 2]);
            Vector3 b = new Vector3(vertPos[i * 9 + 3], vertPos[i * 9 + 4], vertPos[i * 9 + 5]);
            Vector3 c = new Vector3(vertPos[i * 9 + 6], vertPos[i * 9 + 7], vertPos[i * 9 + 8]);

            m_dbgRenderMesh.AddTriangle(new DbgRenderTriangle(a, b, c, col));
        }
        m_dbgRenderMesh.Rebuild();
    }

    public void LoadOb(string path)
    {
        bool ret = RecastHelper.load_ob_bin(path);
        if (!ret)
        {
            Debug.LogWarning("load map bin failed");
            return;
        }
        int obCount = RecastHelper.get_ob_count();
    }

    private void CreateDbgRenderMesh()
    {
        Shader shader = Shader.Find("Diffuse");
        Material m = new Material(shader);
        m_dbgRenderMesh.CreateGameObject("DbgRenderMesh", m);
        m_dbgRenderMesh.Clear();
    }
}
