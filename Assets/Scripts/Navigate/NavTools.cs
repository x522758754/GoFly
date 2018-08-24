using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unity调试Recast工具
public class NavTools : MonoBehaviour
{
    public float m_pathSpace = 0.1f;
    bool m_ret = false;
    DbgRenderMesh m_dbgRenderMesh = new DbgRenderMesh();
    GameObject m_goDbg;
    private List<Vector3> targetList = new List<Vector3>();
    private List<Vector3> bornList = new List<Vector3>();
    private List<RecastDetour> rdList = new List<RecastDetour>();

    public void LoadMap(string path)
    {
        m_ret = RecastHelper.load_map_bin(path);
        if (!m_ret)
        {
            Debug.LogWarning("load map bin failed");
            return;
        }

        CreateDbgRenderMesh();

        ///1 tri => 3 verts, 1 verts = 1 pos(vec3) => 3 float
        int triCount = RecastHelper.get_mesh_vert_count();
        float[] vertPos = new float[triCount * 9];
        RecastHelper.get_mesh_vert_pos(vertPos);
        Color col = new Color();

        for (int i=0; i != triCount; ++i)
        {
            Vector3 a = new Vector3(vertPos[i * 9 + 0], vertPos[i * 9 + 1], vertPos[i * 9 + 2]);
            Vector3 b = new Vector3(vertPos[i * 9 + 3], vertPos[i * 9 + 4], vertPos[i * 9 + 5]);
            Vector3 c = new Vector3(vertPos[i * 9 + 6], vertPos[i * 9 + 7], vertPos[i * 9 + 8]);

            m_dbgRenderMesh.AddTriangle(new DbgRenderTriangle(a, b, c, col));
            Vector3 triCenter = (a + b + c) / 3;
            float[] pos = new float[3];
            pos[0] = triCenter.x;
            pos[1] = triCenter.y;
            pos[2] = triCenter.z;
            if (RecastHelper.is_valid_pos(pos))
            {
                bornList.Add(triCenter);
            }
        }
        m_dbgRenderMesh.Rebuild();
        BuildPathMap();
    }

    public void LoadOb(string path)
    {
        bool ret = RecastHelper.load_ob_bin(path);
        if (!ret)
        {
            Debug.LogWarning("load map bin failed");
            return;
        }
        int obCount = RecastHelper.get_ob_box_count();
    }

    public void StartNavTest()
    {
        if (!m_ret) return;
        for (int i=0; i != bornList.Count; ++i)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.transform.position = bornList[i];
            go.transform.localScale = Vector3.one;
            go.name = "robot_" + i.ToString();
            if (m_goDbg)
                go.transform.parent = m_goDbg.transform;

            RecastDetour rd = go.AddComponent<RecastDetour>();
            rd.targetList = targetList;
            rdList.Add(rd);
        }

    }

    private void CreateDbgRenderMesh()
    {
        Shader shader = Shader.Find("Diffuse");
        Material m = new Material(shader);
        m_goDbg = m_dbgRenderMesh.CreateGameObject("DbgRenderMesh", m);
        m_dbgRenderMesh.Clear();
    }

    private void BuildPathMap()
    {
        Vector3 min, center, max;
        m_dbgRenderMesh.GetBounds(out min, out center, out max);
        for (float i = min.x; i < max.x;)
        {
            for (float j = min.z; j < max.z;)
            {
                Vector3 _point = new Vector3(i, 10000, j);
                RaycastHit hit;
                if (Physics.Raycast(new Ray(_point, new Vector3(0, -1, 0)), out hit, 20000.0f))
                {
                    targetList.Add(hit.point);
                }
                j += m_pathSpace;
            }
            i += m_pathSpace;
        }
    }


}
