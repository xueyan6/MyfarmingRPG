using UnityEngine.SceneManagement;
using UnityEngine;
using Cinemachine;
using System;

public class SwitchConfineBoundingShape : MonoBehaviour
{
    //Ԫ�ػ�ȡ����ͬ�������ɣ���Ҫ�ڳ���ȷ�ϼ�����Ϻ����ȥ��ȡ
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SwitchBoundingShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SwitchBoundingShape;
    }


    /// <summary>
    /// Switch the collider that cinemachine uses to define the edges of the screen�л�Cinemachine���ڶ�����Ļ��Ե����ײ��
    /// </summary>

    private void SwitchBoundingShape()
    {
        //Get the polygon collider on the "boundsconfiner" gameobject which is used by cinemachine to prevent the camera going beyond the screen edges
        //��ȡ���ڷ�ֹ�����������Ļ��Ե�ġ�boundsconfiner����Ϸ�����ϵĶ������ײ�壨�ö�����Cinemachineʹ�ã�
        PolygonCollider2D polygonCollider2D = GameObject.FindGameObjectWithTag(Tags.BoundsConfiner).GetComponent<PolygonCollider2D>();

        CinemachineConfiner cinemachineconfiner=GetComponent<CinemachineConfiner>();

        cinemachineconfiner.m_BoundingShape2D = polygonCollider2D;

        //since the confiner bounds have changed need to call this to clear the cache
        //�����������߽��ѱ��������ô˷����������

        cinemachineconfiner.InvalidatePathCache();
    }
}
 