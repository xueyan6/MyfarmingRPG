
using UnityEngine;


[ExecuteAlways]//使脚本在编辑模式和运行模式都能执行
public class GenerateGUID : MonoBehaviour
{
    [SerializeField]
    private string _gUID = "";

    public string GUID { get => _gUID; set => _gUID = value; }//属性封装器：提供对私有字段的安全访问，支持外部读写操作


    private void Awake()
    {
        // Only populate in the editor仅在编辑器中填充
        if (!Application.IsPlaying(gameObject))
        {

            // Ensure the object has a guaranteed unique id确保对象具有唯一标识符
            if (_gUID == "")
            {
                // Assign GUID分配全局唯一标识符
                _gUID = System.Guid.NewGuid().ToString();//核心功能：调用.NET GUID生成器，转换为字符串存储
            }
        }
    }
}
