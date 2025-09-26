using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneControllerManager : SingletonMonobehaviour<SceneControllerManager>
{
    private bool isFading;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup faderCanvasGroup = null;
    [SerializeField] private Image faderImage = null;
    public SceneName startingSceneName;

    private IEnumerator Fade(float finalAlpha)
    {
        // Set the fading flag to true so the FadeAndSwitchScenes coroutine won't be called again.将淡出标志设置为true，这样FadeAndSwitchScenes协程就不会再次被调用。
        isFading = true;

        // Make sure the CanvasGroup blocks raycasts into the scene so no more input can be accepted.确保CanvasGroup阻止射线投射进入场景，从而不再接受任何输入。
        faderCanvasGroup.blocksRaycasts = true;

        // Calculate how fast the CanvasGroup should fade based on it's current alpha,
        // it's final alpha and how long it has to change between the two.
        //根据CanvasGroup的当前透明度、最终透明度以及两者之间需要变化的时间，计算其淡出速度。
        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;

        // while the CanvasGroup hasn't reached the final alpha yet...虽然CanvasGroup尚未达到透明度...
        while (!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
        {
            // ... move the alpha towards it's target alpha.将透明度向其目标透明度移动。
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha,
                fadeSpeed * Time.deltaTime);

            // Wait for a frame then continue.等待一个帧后继续。
            yield return null;
        }

        // Set the flag to false since the fade has finished.由于淡出效果已完成，将标志设置为false。
        isFading = false;

        // Stop the CanvasGroup from blocking raycasts so input is no longer ignored.停止阻止CanvasGroup阻挡射线投射，使输入不再被忽略。
        faderCanvasGroup.blocksRaycasts = false;
    }

    // This is the coroutine where the 'building blocks' of the script are put together.这是将脚本“构建模块”整合在一起的协程。
    private IEnumerator FadeAndSwitchScenes(string sceneName, Vector3 spawnPosition)
    {
        // Call before scene unload fade out event在场景卸载淡出事件前调用
        EventHandler.CallBeforeSceneUnloadFadeOutEvent();

        // Start fading to block and wait for it to finish before continuing.开始淡出至阻挡层，并在淡出完成后再继续。
        yield return StartCoroutine(Fade(1f));  // 变黑色

        //Store scene data存储场景数据
        SaveLoadManager.Instance.StoreCurrentSceneData();

        // Set player position设置玩家位置
        Player.Instance.gameObject.transform.position = spawnPosition;

        // Call before scene unload event.Call before scene unload event.在场景卸载事件之前调用。在场景卸载事件之前调用。
        EventHandler.CallBeforeSceneUnloadEvent();

        // Unload the current active scene.卸载当前活动场景。
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        // Start loading the given scene and wait for it to finish.开始加载指定场景，并等待其加载完成。
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        // Call after scene load event场景加载事件后的调用
        EventHandler.CallAfterSceneLoadEvent();

        //Restore new scene data恢复新场景数据
        SaveLoadManager.Instance.RestoreCurrentSceneData();

        // Start fading back in and wait for it to finish before exiting the function.开始淡入效果，并在淡入完成后退出函数。
        yield return StartCoroutine(Fade(0f)); // 变白色

        // Call after scene load fade in event场景载入淡入事件后呼叫
        EventHandler.CallAfterSceneLoadFadeInEvent();

    }

    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        // Allow the given scene to load over serval frames and add it to the already
        // loaded scenes (just the Persistent scene at this point).
        //允许指定场景在多个帧内加载完毕，并将其添加至已加载的场景中（此时仅指持久场景）。
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Find the scene that was most recently loaded (the one at the last index of the loaded scenes).
        //查找最近加载的场景（即加载场景列表中索引最末的那个场景）。
        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        // Set the newly loaded scene as the active scene(this marks it as the one to be unloaded next).
        // 将新加载的场景设为活动场景（这标志着它将成为下一个要卸载的场景）
        SceneManager.SetActiveScene(newlyLoadedScene);

    }

    private IEnumerator Start()
    {
        // Set the initial alpha to start off with a block screen.设置初始透明度，以全屏遮罩效果开始。
        faderImage.color = new Color(0f, 0f, 0f, 1f);
        faderCanvasGroup.alpha = 1f;

        // Start the first scene loading and wait for it to finish开始加载第一个场景并等待其完成
        yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName.ToString()));

        // If this event has any subscribers, call it如果此事件有任何订阅者，则调用它
        EventHandler.CallAfterSceneLoadEvent();

        //每次进入到新场景，都恢复一下场景下的数据信息。
        SaveLoadManager.Instance.RestoreCurrentSceneData(); 

        // Once the scene is finished loading, start fading in场景加载完成后，开始淡入效果
        StartCoroutine(Fade(0f));
    }

    // This is the main external point of contact and influence from the rest of the project.
    // This will be called when the player wants to switch scenes.
    //这是项目其他部分对外的主要联络点和影响点。玩家需要切换场景时会调用此接口。
    // sceneName:目标场景名称
    // spawnPosition: 主角出现的位置
    public void FadeAndLoadScene(string sceneName, Vector3 spawnPosition)
    {
        // If a fade isn't happening then start fading and switching scenes.若淡出效果未实现，则开始淡出并切换场景。
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneName, spawnPosition));
        }
    }



}
    /// <summary>总结：
    /// ‌1. 核心功能
    /// 该代码实现了一个‌带过渡效果的场景切换系统‌，通过淡入淡出动画（黑屏/透明过渡）和异步加载，确保场景切换流畅且无卡顿。
    /// 支持‌基础场景（Persistent Scene）‌与‌动态场景（如Farmyard/Field Scene）‌的叠加加载。
    /// ‌2. 关键流程‌
    /// ‌触发阶段‌
    /// 调用 FadeAndLoadScene (sceneName,spawnPosition)时，检查是否正在过渡（isFading）。
    /// 若未过渡，启动协程FadeAndSwitchScenes，执行以下步骤：
    /// 过渡与卸载
    /// 淡出黑屏‌：调用 CallBeforeSceneUnloadFadeOutEvent，通过 Fade(1f)将屏幕变黑（CanvasGroup透明度从0→1）。
    /// ‌重置玩家位置‌：将玩家对象移动到目标场景的起点（spawnPosition）。
    /// 卸载场景‌：异步卸载当前场景（UnloadSceneAsync），保留基础场景。
    /// ‌加载新场景‌
    /// 异步加载‌：以Additive模式加载新场景（LoadSceneAsync），避免基础场景重复加载。
    /// ‌激活场景‌：通过 SetActiveScene 标记新场景为“待卸载目标”。
    /// 淡入与事件‌
    /// 淡入透明‌：调用 CallAfterSceneLoadFadeInEvent，通过Fade(0f)将屏幕变透明（CanvasGroup透明度从1→0）。
    /// 事件回调‌：加载完成后触发CallAfterSceneLoadEvent，通知其他系统（如UI更新）。
    /// </summary>


