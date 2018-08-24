using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//导航测试器
public class RecastDetour : MonoBehaviour
{
    public List<Vector3> targetList;
    List<Vector3> pathlist = new List<Vector3>();

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
        RecastHelper.find_path(sPos, ePos, out path, out pathCount);
        for (int i=0; i < pathCount*3; i+=3)
        {
            Vector3 point = new Vector3(path[i], path[i + 1], path[i + 2]);
            pathlist.Add(point);
        }
    }

    void move(Vector3 sPos, Vector3 ePos)
    {
        ///参照游戏进行移动测试
        ///不符合则打log记录 掉出网格外、寻路目标不到终点
        ///
        Vector3 v = ePos - sPos;
    }

    void Update()
    {
        if (pathlist.Count >= 2)
            move(pathlist[0], pathlist[1]);
        else
        {
            calcTargetPos();
        }
    }
}
