using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class SceneTeleport : MonoBehaviour
{
    [SerializeField] private SceneName sceneNameGoto = SceneName.Scene1_Farm;  // 要去的场景的名称
    [SerializeField] private Vector3 scenePositionGoto = new Vector3(); // 新场景的player的位置信息

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 尝试从碰撞对象获取Player组件，如果碰撞对象不是Player类型，则player为null
        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            // Calculate player's new position计算玩家的新位置

            // 如果目标场景的x位置接近0，则保持玩家当前位置x坐标，否则使用预设的x位置坐标
            float xPosition = Mathf.Approximately(scenePositionGoto.x, 0f) ? player.transform.position.x : scenePositionGoto.x;
            // 如果目标场景的x位置接近0，则保持玩家当前位置y坐标，否则使用预设的y位置坐标
            float yPosition = Mathf.Approximately(scenePositionGoto.y, 0f) ? player.transform.position.y : scenePositionGoto.y;
            //解析：
            //1.坐标接近0的判断：
            //如果预设值为0或接近0（如0.0001），系统认为开发者没有明确指定目标x坐标。
            //此时会继承玩家当前x坐标（player.transform.position.x），保持玩家在x轴方向的位置不变。
            //2.预设值非0的情况：
            //如果预设的x坐标明显不为0（如10.5），则使用该预设值作为玩家在新场景的x坐标。
            //这适用于开发者明确需要玩家出现在新场景的特定位置时（如传送到某个固定点）。


            //固定z坐标为0（2D场景中通常不需要z轴坐标）
            float zPosition = 0f;

            // Teleport to new scene传送至新场景
            SceneControllerManager.Instance.FadeAndLoadScene(sceneNameGoto.ToString(), new Vector3(xPosition, yPosition, zPosition));
            // 调用场景管理器的淡入淡出加载方法
            // 参数1：目标场景名称
            // 参数2：玩家在新场景中的初始位置
        }
    }



}
