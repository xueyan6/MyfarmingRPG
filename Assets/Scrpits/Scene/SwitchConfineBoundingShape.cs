using UnityEngine.SceneManagement;
using UnityEngine;
using Cinemachine;
using System;

public class SwitchConfineBoundingShape : MonoBehaviour
{
    //元素获取基于同样的理由，需要在场景确认加载完毕后才能去获取
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SwitchBoundingShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SwitchBoundingShape;
    }


    /// <summary>
    /// Switch the collider that cinemachine uses to define the edges of the screen切换Cinemachine用于定义屏幕边缘的碰撞体
    /// </summary>

    private void SwitchBoundingShape()
    {
        //Get the polygon collider on the "boundsconfiner" gameobject which is used by cinemachine to prevent the camera going beyond the screen edges
        //获取用于防止摄像机超出屏幕边缘的“boundsconfiner”游戏对象上的多边形碰撞体（该对象由Cinemachine使用）
        PolygonCollider2D polygonCollider2D = GameObject.FindGameObjectWithTag(Tags.BoundsConfiner).GetComponent<PolygonCollider2D>();

        CinemachineConfiner cinemachineconfiner=GetComponent<CinemachineConfiner>();

        cinemachineconfiner.m_BoundingShape2D = polygonCollider2D;

        //since the confiner bounds have changed need to call this to clear the cache
        //由于限制器边界已变更，需调用此方法清除缓存

        cinemachineconfiner.InvalidatePathCache();
    }
}
 