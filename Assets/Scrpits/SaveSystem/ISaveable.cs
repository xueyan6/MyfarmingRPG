
public interface ISaveable
{
    string ISaveableUniqueID { get; set; }//ÿ���ɱ�������Ψһ��ʶ��

    GameObjectSave GameObjectSave { get; set; }//����������ݵ�����

    void ISaveableRegister();//�򱣴�ϵͳע�ᵱǰ����

    void ISaveableDeregister();//�ӱ���ϵͳע����ǰ����

    void ISaveableStoreScene(string sceneName);//����ǰ�����״̬�������л���ָ�������Ĵ浵��

    void ISaveableRestoreScene(string sceneName);//��ָ�������浵�ж�ȡ���ָ�����״̬

}
