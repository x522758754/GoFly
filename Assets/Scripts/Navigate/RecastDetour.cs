using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//导航测试
public class RecastDetour : MonoBehaviour
{
    List<Vector3> pathlist = new List<Vector3>();

    void calcTargetPos()
    {
        //随机设置NavMesh上目标点

        //计算到目标点的路径点列表

        //
    }

    void move(Vector3 firstPos, Vector3 secondPos)
    {
        ///参照游戏进行移动测试
        ///不符合则打log记录 掉出网格外、寻路目标不到终点
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
