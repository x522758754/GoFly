using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//导航测试器
public class RecastDetour : MonoBehaviour
{
    public List<Vector3> targetList;

    List<Vector3> pathlist = new List<Vector3>();
    List<GameObject> pathObjects = new List<GameObject>();
    float m_speed = 16f;
    float m_distace = 0f;

    void calcTargetPos()
    {
        if (targetList.Count < 1) return;

        int index = Random.Range(0, targetList.Count);
        
        float[] sPos = new float[3];
        sPos[0] = this.transform.position.x;
        sPos[1] = this.transform.position.y;
        sPos[2] = this.transform.position.z;
        float[] ePos = new float[3];
        ePos[0] = targetList[index].x;
        ePos[1] = targetList[index].y;
        ePos[2] = targetList[index].z;


        int pathCount;
        int max_path_count = 256 * 3;
        float[] path = new float[max_path_count];
        RecastHelper.find_path(sPos, ePos, path, out pathCount, out m_distace);

        recycle();
        for (int i=0; i < pathCount*3; i+=3)
        {
            Vector3 point = new Vector3(path[i], path[i + 1], path[i + 2]);
            pathlist.Add(point);
            getObject(pathObjects.Count, point);
        }

        getObject(pathObjects.Count, transform.position, 1);
        getObject(pathObjects.Count, targetList[index], 2);
    }

    void recycle()
    {
        for(int i=0; i != pathObjects.Count; ++i)
        {
            GameObject.Destroy(pathObjects[i]);
        }
        pathObjects.Clear();
    }

    GameObject getObject(int index, Vector3 point, int type = 0)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        if(type == 1) ///标志起点
        {
            go.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else if(type == 2)
        {
            go.GetComponent<MeshRenderer>().material.color = Color.green;
        }

        go.transform.position = point;
        go.transform.localScale = Vector3.one;
        go.name = "path_" + index.ToString();
        pathObjects.Add(go);

        return go;
    }

    void move(float dt)
    {
        float moveDist = dt * m_speed;

        Vector2 vCurr = new Vector2(transform.position.x, transform.position.z);
        Vector2 vDest = new Vector2(pathlist[0].x, pathlist[0].z);
        Vector2 v = vDest - vCurr;
        Vector2 vDir = v.normalized;

        if (moveDist >= m_distace)
        {
            ///移动距离大于剩余路程
            transform.position = pathlist[pathlist.Count - 1];
            pathlist.Clear();
        }
        else
        {
            ///移动距离小于剩余路程
            Vector2 pos;

            ///移动距离小于当前位置至下一个路点的路程
            if (moveDist <= v.magnitude)
            {
                pos = vCurr + moveDist * vDir;
            }
            else
            {
                ///移动距离大于当前位置至下一个路点的路程
                vCurr = vDest;
                moveDist -= v.magnitude;
                pathlist.RemoveAt(0);
                if (pathlist.Count > 0)
                    pos = moveToNext(vDest, moveDist);
                else
                    pos = vCurr;
            }

            float h = 0;
            h = NavTools.GetHeight(pos.x, pos.y);
            transform.position = new Vector3(pos.x, h, pos.y);
        }

    }

    Vector2 moveToNext(Vector2 vCurr, float moveDist)
    {
        Vector2 pos;
        Vector2 vDest = new Vector2(pathlist[0].x, pathlist[0].z);
        Vector2 v = vDest - vCurr;
        Vector2 vDir = v.normalized;

        if (moveDist <= v.magnitude)
        {
            ///移动距离小于当前路点至下一个路点的路程
            pos = vCurr + moveDist * vDir;
            return pos;
        }
        else
        {
            ///移动距离大于当前路点至下一个路点的路程
            float[] start = new float[3] { vCurr.x, 0, vCurr.y };
            float[] end = new float[3] { vDest.x, 0, vDest.y };
            float[] hit = new float[3];
            float[] hitNormal = new float[3];

            bool bHit = RecastHelper.raycast(start, end, hit, hitNormal);
            if (bHit)
                ///有阻挡
                return new Vector2(hit[0], hit[2]);
            else
            {
                ///无阻挡
                ///以当前目标路点为起始点开始
                moveDist -= v.magnitude;
                vCurr = vDest;
                pathlist.RemoveAt(0);
                if (pathlist.Count > 0)
                    return moveToNext(vDest, moveDist);
                else
                    return vCurr;
            }

        }
    }

    void Update()
    {
        if (pathlist.Count > 0)
            move(Time.deltaTime);
        else
            calcTargetPos();
    }
}
