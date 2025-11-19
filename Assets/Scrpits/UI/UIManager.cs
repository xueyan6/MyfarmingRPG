using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonobehaviour<UIManager>
{
    private bool _pauseMenuOn = false;// 标记暂停菜单是否打开
    [SerializeField] private UIInventoryBar uiInventoryBar = null;
    [SerializeField] private PauseMenuInventoryManagement pauseMenuInventoryManagement = null;//暂停菜单库存管理
    [SerializeField] private GameObject pauseMenu = null;// 暂停菜单游戏对象
    [SerializeField] private GameObject[] menuTabs = null;// 菜单标签页数组
    [SerializeField] private Button[] menuButtons = null;// 菜单按钮数组

    public bool PauseMenuOn { get => _pauseMenuOn; set => _pauseMenuOn = value; }// 声明暂停菜单状态属性

    private void Awake()
    {
        base.Awake();

        pauseMenu.SetActive(false);
        pauseMenu.SetActive(false);// 初始化时关闭暂停菜单
    }

    private void Update()
    {
        PauseMenu();// 每帧检测暂停菜单输入
    }

    private void PauseMenu()
    {
        // Toggle pause menu if escape is pressed按下Escape键时切换暂停菜单
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseMenuOn)
            {
                DisablePauseMenu();// 如果菜单已打开，则关闭
            }
            else
            {
                EnablePauseMenu();// 如果菜单未打开，则打开
            }
        }
    }

    private void EnablePauseMenu()
    {
        // Destroy any currently dragged items 清除当前拖拽的任何项目 
        uiInventoryBar.DestroyCurrentlyDraggedItems();

        // Clear currently selected items 清除当前选中的项目 
        uiInventoryBar.ClearCurrentlySelectedItems();

        PauseMenuOn = true;// 设置暂停菜单状态为打开
        Player.Instance.PlayerInputIsDisabled = true;// 禁用玩家输入
        Time.timeScale = 0;// 暂停游戏时间
        pauseMenu.SetActive(true);// 显示暂停菜单

        // Trigger garbage collector触发垃圾回收器
        System.GC.Collect(); // 在暂停时进行垃圾回收（提高性能）

        // Highlight selected button高亮选定按钮
        HighlightButtonForSelectedTab();// 更新按钮高亮状态
    }

    public void DisablePauseMenu()
    {
        // Destroy any currently dragged items 清除当前拖拽的任何项目 
        pauseMenuInventoryManagement.DestroyCurrentlyDraggedItems();

        PauseMenuOn = false;// 设置暂停菜单状态为关闭
        Player.Instance.PlayerInputIsDisabled = false;// 启用玩家输入
        Time.timeScale = 1;// 恢复游戏时间
        pauseMenu.SetActive(false);// 隐藏暂停菜单
    }

    private void HighlightButtonForSelectedTab()
    {
        for (int i = 0; i < menuTabs.Length; i++)// 遍历所有菜单标签页
        {
            if (menuTabs[i].activeSelf)// 如果标签页处于活动状态
            {
                SetButtonColorToActive(menuButtons[i]);// 设置对应按钮为活动颜色
            }
            else
            {
                SetButtonColorToInactive(menuButtons[i]);// 设置对应按钮为非活动颜色
            }
        }
    }

    private void SetButtonColorToActive(Button button)
    {
        ColorBlock colors = button.colors;// 获取按钮的颜色配置
        colors.normalColor = colors.pressedColor;// 将正常状态颜色设为按下状态颜色
        button.colors = colors; // 应用新的颜色配置
    }

    private void SetButtonColorToInactive(Button button)
    {
        ColorBlock colors = button.colors;// 获取按钮的颜色配置
        colors.normalColor = colors.disabledColor;// 将正常状态颜色设为禁用状态颜色
        button.colors = colors;// 应用新的颜色配置
    }

    // 该函数放在SelectionButton的onClick事件中
    public void SwitchPauseMenuTab(int tabNum)// 切换暂停菜单标签页
    {
        for (int i = 0; i < menuTabs.Length; i++)// 遍历所有菜单标签页
        {
            //判断是否是用户想要打开的标签页
            if (i != tabNum)
            {
                menuTabs[i].SetActive(false);// 关闭非目标标签页
            }
            else
            {
                menuTabs[i].SetActive(true);// 打开目标标签页
            }
        }

        HighlightButtonForSelectedTab();// 更新按钮高亮状态
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
