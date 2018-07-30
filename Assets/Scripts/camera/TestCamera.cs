using System;
using System.Collections.Generic;
using UnityEngine;

public class TestCamera : MonoBehaviour
{
    ///public float _roll;     //翻滚角不考虑使用
    ///public float _fovx;     //视锥体的水平方向上的视野角度,取决于视口的宽高比
    ///
    ///相机操作基础数据(欧拉角、视角、跟随设置)
    public float _pitch;    //俯仰角[-89,89],避免造成万向锁
    public float _yaw;      //偏航角(Unity左手坐标系
                            //默认情况，不做任何旋转的情况相机的朝向就是vec3(0,0,1),不用做任何调整）
    public float _fovy;     //视锥体的垂直方向上的视野角度
    public float _followDistance;       //跟随距离
    public float _followZoomSpeed;      //跟随推拉速度
    public float _followRotateSpeed;    //跟随旋转速度

    ///目标与跟随
    public Transform transFollow;
    public Transform transTarget;

    /// 相机操作扩展数据
    public int _raycastLayers;      //射线检测LayerMask
    public float _zoomSpeed;        //相机推拉速度,单位（m/s）
    public float _rotateSpeed;      //相机旋转速度,单位（°/s）
    public bool _lockPitch;         //相机运动是否锁定俯仰角
    public bool _lockYaw;           //相机运动是否锁定偏航角
    public bool _lockZoom;          //相机运动是否锁定跟随位置
    public Vector3 _targetOffset;   //相机目标点偏移
    public float _targetPitch;      //相机目标俯仰角
    public float _targetYaw;        //相机目标偏航角

    private void InitData()
    {
        _pitch = 0;
        _yaw = 0;

        _zoomSpeed = 6;
        _rotateSpeed = 10;

        _followDistance = 6;
        _followZoomSpeed = 6;
        _followRotateSpeed = 10;

        _targetOffset = new Vector3(0, 1.5f, 0);
        _targetPitch = 0;
        _targetYaw = 0;
    }

    public void OnSetTouchDeltaX(float deltax)
    {
        _targetYaw -= deltax;
    }
    public void OnSetTouchDeltaY(float deltaY)
    {
        _targetPitch -= deltaY;

        _targetPitch = _targetPitch < -89 ? -89 : _targetPitch;
        _targetPitch = _targetPitch > 89 ? 89 : _targetPitch;
    }

    private void Start()
    {
        InitData();
    }

    public void Update()
    {
        
    }

    Vector3 _prePos; //缓存相机上一帧的位置
    Vector3 _currentTargetPos; //当前帧移动目标点
    Vector3 _curTargetOffset;
    float _currFollowDis;

    public void LateUpdate()
    {
        float deltaTime = Time.deltaTime;

        _currentTargetPos = Vector3.Lerp(_currentTargetPos, transTarget.position, deltaTime * _followZoomSpeed);
        _curTargetOffset = Vector3.Lerp(_curTargetOffset, _targetOffset, deltaTime * _followZoomSpeed * 0.5f);
        
        //碰撞检测
        float colliderDis = _followDistance;
        Vector3 raycastOrign = _currentTargetPos + _curTargetOffset;
        Vector3 raycastDir = (transFollow.position - raycastOrign).normalized;
        RaycastHit hit;
        if(Physics.Raycast(raycastOrign, raycastDir, out hit, colliderDis, 256))
        {
            colliderDis = hit.distance;
        }
        _currFollowDis = Mathf.Lerp(_currFollowDis, colliderDis, deltaTime * _followZoomSpeed);

        //俯仰角、偏航角插值计算
        _pitch = Mathf.Lerp(_pitch, _targetPitch, deltaTime * _followRotateSpeed);
        _yaw = Mathf.Lerp(_yaw, _targetYaw, deltaTime * _followRotateSpeed);

        //俯仰角、偏航角
        float y = Mathf.Sin(_pitch * Mathf.Deg2Rad);
        float x = Mathf.Cos(_yaw * Mathf.Deg2Rad);
        float z = Mathf.Sin(_yaw * Mathf.Deg2Rad);

        //相机的朝向根据相机的欧拉角计算得到
        Vector3 front = new Vector3(x * Mathf.Cos(_pitch * Mathf.Deg2Rad), y, z * Mathf.Cos(_pitch * Mathf.Deg2Rad));
        transFollow.forward = -front;
        
        //设置相机的位置
        transFollow.position = _currentTargetPos + _curTargetOffset + front * _currFollowDis;

        //计算相机震动
        Vector3 shakeCamPos = Vector3.zero;//用annimation做
        Vector3 shakeOffset = transFollow.localToWorldMatrix.MultiplyPoint(shakeCamPos);
        Vector3 worldOffset = shakeOffset - transFollow.position;
        transFollow.position += worldOffset;

        //相机在每个坐标轴的相对位置
        float distanceProjXZ = _currFollowDis * Mathf.Cos(_pitch * Mathf.Deg2Rad);
        float distanceProjX = distanceProjXZ * x;
        float distanceProjZ = distanceProjXZ * z;
        float distanceProjY = _currFollowDis * y;
        Vector3 ralationPos = new Vector3(distanceProjX, distanceProjY, distanceProjZ);
    }
}