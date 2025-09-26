
using UnityEngine;


[ExecuteAlways]//ʹ�ű��ڱ༭ģʽ������ģʽ����ִ��
public class GenerateGUID : MonoBehaviour
{
    [SerializeField]
    private string _gUID = "";

    public string GUID { get => _gUID; set => _gUID = value; }//���Է�װ�����ṩ��˽���ֶεİ�ȫ���ʣ�֧���ⲿ��д����


    private void Awake()
    {
        // Only populate in the editor���ڱ༭�������
        if (!Application.IsPlaying(gameObject))
        {

            // Ensure the object has a guaranteed unique idȷ���������Ψһ��ʶ��
            if (_gUID == "")
            {
                // Assign GUID����ȫ��Ψһ��ʶ��
                _gUID = System.Guid.NewGuid().ToString();//���Ĺ��ܣ�����.NET GUID��������ת��Ϊ�ַ����洢
            }
        }
    }
}
