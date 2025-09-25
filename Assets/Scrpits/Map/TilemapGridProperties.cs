using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


[ExecuteAlways]
public class TilemapGridProperties : MonoBehaviour
{
    private Tilemap tilemap;
    [SerializeField] private SO_GridProperties gridProperties = null;
    [SerializeField] private GridBoolProperty gridBoolProperty = GridBoolProperty.diggable;

    private void OnEnable()
    {
        // Only populate in the editor���ڱ༭��ģʽ��ִ�У�������ʱ��
        if (!Application.IsPlaying(gameObject))
        {
            tilemap = GetComponent<Tilemap>();

            if (gridProperties != null)// ����������������Ѹ�ֵ
            {
                // �༭ģʽ�¿����ű���������е�gridPropertyList����
                gridProperties.gridPropertyList.Clear();// ������������б��е���������
            }
        }
    }

    private void OnDisable()
    {
        // Only populate in the editor���ڱ༭��ģʽ��ִ�У�������ʱ��
        if (!Application.IsPlaying(gameObject))
        {
            // ���������������ݡ����رսű����������ݣ�
            UpdateGridProperties();

            if (gridProperties != null)
            {
                // This is required to ensure that the updated gridproperties gameobject gets saved when the game is saved - otherwise they are not saved.
                //�˲���ּ��ȷ�����º������������Ϸ�����ڱ�����Ϸʱ�����桪��������Щ���Խ����ᱻ���档   
                EditorUtility.SetDirty(gridProperties);// ����������Զ���Ϊ���޸�״̬��ȷ������ʱ��Ч
            }
        }
    }


    private void UpdateGridProperties()
    {
        // Compress tilemap boundsѹ����Ƭ��ͼ�߽磨ȥ����Ч������ı߽����ݣ�ʹ��ͼ��ʾ���ӽ��ո�Ч��
        tilemap.CompressBounds();

        // Only populate in the editor���ڱ༭��ģʽ��ִ�У�������ʱ��
        if (!Application.IsPlaying(gameObject))
        {
            if (gridProperties != null)
            {
                //// ��ȡ��Ƭ��ͼ����ʼ�ͽ�����Ԫ������
                Vector3Int startCell = tilemap.cellBounds.min;
                Vector3Int endCell = tilemap.cellBounds.max;

                //������Ƭ��ͼ�ϵ�ÿ����Ԫ��
                for (int x = startCell.x; x < endCell.x; x++)
                {
                    for (int y = startCell.y; y < endCell.y; y++)
                    {
                        // ��ȡָ��λ�õ���Ƭ
                        TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));

                        // �����λ������Ƭ
                        if (tile != null)
                        {
                            // ����Ƭ�����������ӵ����������б���
                            gridProperties.gridPropertyList.Add(new GridProperty(new GridCoordinate(x, y), gridBoolProperty, true));
                        }
                    }
                }
            }
        }

    }

    private void Update()
    {
        // Only populate in the editor���ڱ༭��ģʽ��ִ�У�������ʱ��
        if (!Application.IsPlaying(gameObject))
        {
            Debug.Log("DISABLE PROPERTY TILEMAPS");
        }
    }

    //���Ĺ��ܣ�
    //�ýű���Unity�༭����չ���ߣ�ר�����ڹ���Tilemap��������������
    //��Ҫʵ��Tilemap��Ԫ�����Ե��Զ��ɼ��ʹ洢����
    //���л��ƣ�
    //���ڱ༭��ģʽ�¹�����ͨ��Application.IsPlaying�жϣ�

    //����ʱ��������������ݣ�OnEnable����
    //���ű����������/����ʱ������Inspector��ѡ����򳡾�����ʱ��
    //�������gridProperties.gridPropertyList�д洢������������������
    //Ŀ����ȷ��ÿ�α༭���Ӹɾ�״̬��ʼ����������ݸ���
    //�������ڱ༭��ģʽ������Ϸ����ʱ��

    //����ʱ������º���������ݣ�OnDisable����
    //���ű����������/���������༭ʱ
    //����UpdateGridProperties()ɨ�赱ǰTilemap������Ч��Ƭ
    //���µ��������������ֵ����gridProperties.gridPropertyList
    //ͨ��EditorUtility.SetDirty���������Ҫ����
    //ȷ���༭����ܳ־û�����Ŀ�ļ���

    //�����ǽ���"�༭ʱ���á��޸ġ��ر�ʱ����"�Ĺ�����������Tilemap����������ϵͳ������ͬ����

    //���ݴ������̣�
    //ͨ��CompressBounds�Ż���Ƭ��ͼ�߽�
    //�������а�����Ƭ�ĵ�Ԫ������
    //����Ч��Ԫ�����������ֵ����SO_GridProperties�ű�������
    //���ݳ־û���
    //ʹ��EditorUtility.SetDirtyȷ���޸��ܱ�����
    //ͨ�����л��ֶα�¶���ò�����gridProperties/gridBoolProperty��
}
