using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unity调试Recast工具
public class NavTools : MonoBehaviour
{
    public float m_pathSpace = 5f;
    bool m_ret = false;
    DbgRenderMesh m_dbgRenderMesh = new DbgRenderMesh();
    GameObject m_goDbg;
    private List<Vector3> targetList = new List<Vector3>();
    private List<Vector3> bornList = new List<Vector3>();
    private List<RecastDetour> rdList = new List<RecastDetour>();

    void Start()
    {
        LoadMap("E:\\Work\\FairyTail\\client\\Output\\data\\mapbin\\1000_zhucheng_ground.bin");
    }

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
        int vertCount = RecastHelper.get_mesh_vert_count();
        float[] vertPos = new float[vertCount*3];
        RecastHelper.get_mesh_vert_pos(vertPos);
        Color col = new Color();

        for (int i=0; i < vertCount*3; i += 9)
        {
            Vector3 a = new Vector3(vertPos[i+0], vertPos[i + 1], vertPos[i + 2]);
            Vector3 b = new Vector3(vertPos[i + 3], vertPos[i + 4], vertPos[i + 5]);
            Vector3 c = new Vector3(vertPos[i + 6], vertPos[i + 7], vertPos[i + 8]);

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

        for (int i=0; i != 1; ++i)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.transform.position = bornList[i];
            go.transform.localScale = Vector3.one * 3;
            go.name = "robot_" + i.ToString();
            if (false && m_goDbg)
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
        m_goDbg.layer = LayerMask.NameToLayer("Ground");
        m_dbgRenderMesh.Clear();
    }

    private void BuildPathMap()
    {
        targetList.Clear();
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

    public static float GetHeight(float x, float z)
    {
        Vector3 vPos = new Vector3(x, 10000f, z);

        RaycastHit hitInfo;
        bool bRet = Physics.Raycast(vPos, Vector3.down, out hitInfo, 20000f, LayerMask.GetMask("Ground"));
        return bRet ? hitInfo.point.y : 0;
    }
}
