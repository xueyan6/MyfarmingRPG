using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour 
{
    [Header("Tiles & Tilemap References")]  // Unity编辑器中的分组标题，用于组织Inspector面板显示
    [Header("Options")]  // 选项分组标题
    [SerializeField] private bool observeMovementPenalties = true;  // 序列化字段，控制是否考虑地形移动惩罚，默认为true
    [Range(0, 20)]  
    [SerializeField] private int pathMovementPenalty = 0;  // 路径移动惩罚值，影响NPC在路径上的移动成本
    [Range(0, 20)]
    [SerializeField] private int defaultMovementPenalty = 0;  // 默认地形移动惩罚值

    private GridNodes gridNodes;  // 网格节点容器，存储所有网格节点的数据结构
    private Node startNode;  // 路径搜索的起始节点
    private Node targetNode;  // 路径搜索的目标节点
    private int gridWidth;  // 网格的宽度（以单元格为单位）
    private int gridHeight;  // 网格的高度（以单元格为单位）
    private int originX;  // 网格原点在世界坐标系中的X坐标
    private int originY;  // 网格原点在世界坐标系中的Y坐标

    private List<Node> openNodeList;  // 开放节点列表，存储待评估的节点，按fCost排序
    private HashSet<Node> closedNodeList;  // 关闭节点集合，存储已评估过的节点，使用HashSet提高查找效率(PS:HashSet用于需要去重或快速查找的数据，不能通过索引来访问元素。
    private bool pathFound = false;  // 布尔标志，记录是否成功找到从起点到终点的路径

    // 构建路径的主要公共方法：为指定场景从起始网格位置到目标网格位置构建路径
    public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int endGridPosition, Stack<NPCMovementStep> npcMovementStepStack)
    {
        // 第一步：从网格属性字典中初始化网格节点
        if (PopulateGridNodesFromGridPropertiesDictionary(sceneName, startGridPosition, endGridPosition))  // 如果网格初始化成功
        {
            // 第二步：执行A*算法寻找最短路径
            if (FindShortestPath())  // 如果成功找到路径
            {
                // 第三步：将找到的路径更新到NPC移动步骤栈中
                UpdatePathOnNPCMovementStepStack(sceneName, npcMovementStepStack);  // 构建NPC可执行的移动序列
                return true;  // 返回true表示路径构建成功
            }
        }
        return false;  // 返回false表示路径构建失败
    }

    // 将A*算法找到的路径转换为NPC移动步骤
    private void UpdatePathOnNPCMovementStepStack(SceneName sceneName, Stack<NPCMovementStep> npcMovementStepStack)
    {
        Node nextNode = targetNode;  // 从目标节点开始回溯路径
        while (nextNode != null)  // 循环直到回溯到起始节点（起始节点的parentNode为null）
        {
            NPCMovementStep nPCMovementStep = new NPCMovementStep();  // 创建新的移动步骤对象
            nPCMovementStep.sceneName = sceneName;  // 设置移动步骤所属的场景名称
            // 将网格坐标转换为世界坐标：gridPosition是相对坐标，加上origin得到绝对坐标
            nPCMovementStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX, nextNode.gridPosition.y + originY);
            npcMovementStepStack.Push(nPCMovementStep);  // 将移动步骤压入栈，由于是回溯，栈顶是起始节点
            nextNode = nextNode.parentNode;  // 移动到当前节点的父节点，继续回溯
        }
    }

    // A*算法的核心实现：寻找从起始节点到目标节点的最短路径
    private bool FindShortestPath()
    {
        openNodeList.Add(startNode);  // 将起始节点加入开放列表，开始路径搜索
        while (openNodeList.Count > 0)  // 当开放列表中还有待评估节点时继续循环
        {
            openNodeList.Sort();  // 按fCost（gCost + hCost）对开放列表进行升序排序
            Node currentNode = openNodeList[0];  // 取出fCost最小的节点作为当前评估节点
            openNodeList.RemoveAt(0);  // 从开放列表中移除该节点
            closedNodeList.Add(currentNode);  // 将当前节点加入关闭集合，标记为已评估
            if (currentNode == targetNode)  // 检查当前节点是否就是目标节点
            {
                pathFound = true;  // 如果找到目标节点，设置路径找到标志
                break;  // 跳出循环，路径搜索完成
            }
            EvaluateCurrentNodeNeighbours(currentNode);  // 评估当前节点的所有邻居节点
        }
        if (pathFound)  // 检查路径是否成功找到
        {
            return true;  // 返回true表示找到路径
        }
        else
        {
            return false;  // 返回false表示未找到路径
        }
    }

    // 评估当前节点的所有邻居节点，这是A*算法扩展搜索范围的关键步骤
    private void EvaluateCurrentNodeNeighbours(Node currentNode)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;  // 获取当前节点在网格中的坐标
        Node validNeighbourNode;  // 用于存储有效的邻居节点
        for (int i = -1; i <= 1; i++)  // 遍历X方向的邻居（左、中、右）
        {
            for (int j = -1; j <= 1; j++)  // 遍历Y方向的邻居（下、中、上）
            {
                if (i == 0 && j == 0)  // 排除自身位置（i=0, j=0）
                    continue;  // 跳过当前循环，不评估自身
                // 获取当前方向上的邻居节点，检查其有效性
                validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j);
                if (validNeighbourNode != null)  // 如果邻居节点有效（非障碍物且在网格范围内）
                {
                    int newCostToNeighbour;  // 计算从起始节点经过当前节点到达邻居节点的总成本
                    if (observeMovementPenalties)  // 如果启用了移动惩罚系统
                    {
                        // gCost：从起始节点到当前节点的实际成本
                        // GetDistance：当前节点到邻居节点的移动成本
                        // movementPenalty：邻居节点所在位置的移动惩罚值
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode) + validNeighbourNode.movementPenalty;
                    }
                    else  // 如果不考虑移动惩罚
                    {
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                    }
                    bool isValidNeighbourNodeInOpenList = openNodeList.Contains(validNeighbourNode);
                    // 如果找到更优路径或邻居节点不在开放列表中
                    if (newCostToNeighbour < validNeighbourNode.gCost || !isValidNeighbourNodeInOpenList)
                    {
                        validNeighbourNode.gCost = newCostToNeighbour;  // 更新邻居节点的实际移动成本
                        validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);  // 更新邻居节点到目标节点的启发式估计成本
                        validNeighbourNode.parentNode = currentNode;  // 设置邻居节点的父节点为当前节点，用于路径回溯
                        if (!isValidNeighbourNodeInOpenList)  // 如果邻居节点不在开放列表中
                        {
                            openNodeList.Add(validNeighbourNode);  // 将邻居节点加入开放列表以待后续评估
                        }
                    }
                }
            }
        }
    }

    // 计算两个节点之间的移动距离，使用对角线距离作为启发式函数
    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);  // X方向的距离绝对值
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);  // Y方向的距离绝对值
        if (dstX > dstY)  // 如果X方向距离大于Y方向距离
           return 14 * dstY + 10 * (dstX - dstY);  // 14和10分别近似对角线和直线移动的成本比例(若x>y,先走min(dstX,dstY)次对角线，再走|dstX-dstY|次直线)
        return 14 * dstX + 10 * (dstY - dstX);  // 返回优化后的距离值，14≈10√2(若y>x,先走min(dstX,dstY)次对角线，再走|dstY-dstX|次直线)
    }

    // 获取有效的邻居节点，排除无效节点（障碍物、超出边界、已评估节点）
    private Node GetValidNodeNeighbour(int neighbourNodeXPosition, int neighbourNodeYPosition)
    {
        // 检查邻居节点坐标是否超出网格边界
        if (neighbourNodeXPosition >= gridWidth || neighbourNodeXPosition < 0 || neighbourNodeYPosition >= gridHeight || neighbourNodeYPosition < 0)
        {
            return null;  // 如果超出边界，返回null表示无效节点
        }
        Node neighbourNode = gridNodes.GetGridNode(neighbourNodeXPosition, neighbourNodeYPosition);  // 从网格中获取对应位置的节点
        if (neighbourNode.isObstacle || closedNodeList.Contains(neighbourNode))  // 检查是否为障碍物或已在关闭列表中
        {
            return null;  // 如果是障碍物或已评估过，返回null
        }
        else
        {
            return neighbourNode;  // 返回有效的邻居节点
        }
    }

    // 从网格属性字典中初始化网格节点，这是路径搜索的准备工作
    private bool PopulateGridNodesFromGridPropertiesDictionary(SceneName sceneName, Vector2Int startGridPosition, Vector2Int endGridPosition)
    {
        SceneSave sceneSave;  // 场景保存数据对象
        // 从GridPropertiesManager单例中获取指定场景的保存数据
        if (GridPropertiesManager.Instance.GameObjectSave.sceneData.TryGetValue(sceneName.ToString(), out sceneSave))
        {
            if (sceneSave.gridPropertyDetailsDictionary != null)  // 检查网格属性字典是否为空
            {
                Vector2Int gridDimensions;  // 网格尺寸（宽度和高度）
                Vector2Int gridOrigin;  // 网格原点坐标
                // 获取网格的尺寸和原点信息
                if (GridPropertiesManager.Instance.GetGridDimensions(sceneName, out gridDimensions, out gridOrigin))
                {
                    gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);  // 根据网格尺寸创建节点网格
                    gridWidth = gridDimensions.x;  // 设置网格宽度
                    gridHeight = gridDimensions.y;  // 设置网格高度
                    originX = gridOrigin.x;  // 设置原点X坐标
                    originY = gridOrigin.y;  // 设置原点Y坐标
                    openNodeList = new List<Node>();  // 初始化开放节点列表
                    closedNodeList = new HashSet<Node>();  // 初始化关闭节点集合
                }
                else
                {
                    return false;  // 获取网格尺寸失败，返回false
                }
                // 将世界坐标转换为网格相对坐标：减去原点坐标得到在网格中的位置
                startNode = gridNodes.GetGridNode(startGridPosition.x - gridOrigin.x, startGridPosition.y - gridOrigin.y);
                targetNode = gridNodes.GetGridNode(endGridPosition.x - gridOrigin.x, endGridPosition.y - gridOrigin.y);
                for (int x = 0; x < gridDimensions.x; x++)  // 遍历网格的所有X坐标
                {
                    for (int y = 0; y < gridDimensions.y; y++)  // 遍历网格的所有Y坐标
                    {
                        // 获取每个网格位置的属性详情
                        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(x + gridOrigin.x, y + gridOrigin.y, sceneSave.gridPropertyDetailsDictionary);
                        if (gridPropertyDetails != null)  // 如果网格属性详情不为空
                        {
                            if (gridPropertyDetails.isNPCObstacle == true)  // 如果该位置是NPC障碍物
                            {
                                Node node = gridNodes.GetGridNode(x, y);  // 获取对应的节点对象
                                node.isObstacle = true;  // 标记该节点为障碍物，NPC无法通过
                            }
                            else if (gridPropertyDetails.isPath == true)  // 如果该位置是路径（有移动惩罚）
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.movementPenalty = pathMovementPenalty;  // 设置路径移动惩罚值
                            }
                            else  // 默认情况（普通地形）
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.movementPenalty = defaultMovementPenalty;  // 设置默认移动惩罚值
                            }
                        }
                    }
                }
            }
            else
            {
                return false;  // 网格属性字典为空，返回false
            }
        }
        else
        {
            return false;  // 获取场景数据失败，返回false
        }
        return true;  // 网格初始化成功，返回true
    }

    //算法执行流程
    //1. 初始化阶段 → PopulateGridNodesFromGridPropertiesDictionary()
    //加载场景网格数据
    //创建节点容器
    //标记障碍物位置
    //配置移动惩罚值

    //2. 路径搜索阶段 → FindShortestPath() + EvaluateCurrentNodeNeighbours()
    //维护开放列表和关闭列表
    //计算gCost、hCost、fCost
    //选择最优节点扩展

    //3. 路径构建阶段 → UpdatePathOnNPCMovementStepStack()
    //从终点回溯到起点
    //构建NPC移动指令序列
}
