using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Cinemachine.CinemachineBlendDefinition;

[System.Serializable]
public class colorSwap
{
    public Color fromColor;// 原始颜色值
    public Color toColor;// 目标颜色值


    // 构造函数：创建颜色替换规则
    public colorSwap(Color fromColor, Color toColor)
    {
        this.fromColor = fromColor;
        this.toColor = toColor;
    }
}


public class ApplyCharacterCustomisation : MonoBehaviour
{
    // Input Textures
    [Header("Base Textures")]
    [SerializeField] private Texture2D maleFarmerBaseTexture = null;// 男性角色基础纹理
    [SerializeField] private Texture2D femaleFarmerBaseTexture = null;// 女性角色基础纹理
    [SerializeField] private Texture2D shirtsBaseTexture = null;// 衬衫样式合集纹理
    [SerializeField] private Texture2D hairBaseTexture = null;//发型基础纹理
    [SerializeField] private Texture2D hatsBaseTexture = null;//发型基础纹理
    [SerializeField] private Texture2D adornmentsBaseTexture = null;//装饰物基础纹理
    private Texture2D farmerBaseTexture;// 当前使用的角色基础纹理

    // 输出纹理配置
    [Header("OutputBase Texture To Be Used For Animation")]
    [SerializeField] private Texture2D farmerBaseCustomised = null;  // 最终自定义角色纹理
    [SerializeField] private Texture2D hairCustomised = null;//发型更新后纹理
    [SerializeField] private Texture2D hatsCustomised = null;//发型更新后纹理
    private Texture2D farmerBaseShirtsUpdated;  // 衬衫更新后的纹理
    private Texture2D farmerBaseAdornmentsUpdated;  // 装饰物更新后的纹理
    private Texture2D selectedShirt;       // 当前选中的衬衫纹理
    private Texture2D selectedAdornments;       // 当前选中的装饰物纹理

    // 衬衫样式选择
    [Header("Select Shirt Style")]
    [Range(0, 1)]   // 取值范围：0=红色衬衫，1=绿色衬衫
    [SerializeField] private int inputShirtStyleNo = 0;

    // Select Hair Style 发型样式选择
    [Header("Select Hair Style")]
    [Range(0, 2)]
    [SerializeField] private int inputHairStyleNo = 0;

    // Select Hat Style 
    [Header("Select Hat Style")] 
    [Range(0, 1)]
    [SerializeField] private int inputHatStyleNo = 0;

    // Select Adornments Style 
    [Header("Select Adornments Style")]
    [Range(0, 2)]
    [SerializeField] private int inputAdornmentsStyleNo = 0;

    // Select Skin Type 皮肤样式选择
    [Header("Select Skin Type")]
    [Range(0, 3)] 
    [SerializeField] private int inputSkinType = 0;

    // 性别选择
    [Header("Select Sex:0=Male, 1=female")]
    [Range(0, 1)]
    [SerializeField] private int inputSex = 0;

    // Select Hair Color 选择发型颜色
    [SerializeField] private Color inputHairColor = Color.black;

    // Select Trouser Color 选择裤子颜色 
    [SerializeField] private Color inputTrouserColor = Color.blue;

    private Facing[,] bodyFacingArray;        // 角色各部位朝向数组
    private Vector2Int[,] bodyShirtOffsetArray;  // 衬衫偏移位置数组
    private Vector2Int[,] bodyAdornmentsOffsetArray;  // 装饰品偏移位置数组

    // 精灵表维度定义
    private int bodyRows = 21;              // 身体精灵行数
    private int bodyColumns = 6;            // 身体精灵列数
    private int farmerSpriteWidth = 16;     // 单个角色精灵宽度
    private int farmerSpriteHeight = 32;    // 单个角色精灵高度

    private int shirtTextureWidth = 9;       // 衬衫纹理宽度
    private int shirtTextureHeight = 36;     // 衬衫纹理高度
    private int shirtSpriteWidth = 9;        // 单个衬衫精灵宽度
    private int shirtSpriteHeight = 9;       // 单个衬衫精灵高度
    private int shirtStylesInSpriteWidth = 16;  // 衬衫纹理图集可容纳的样式数量

    private int hairTextureWidth = 16;//发型纹理宽度
    private int hairTextureHeight = 96;//发型纹理高度
    private int hairStylesInSpriteWidth = 8;// 发型纹理图集可容纳的样式数量

    private int hatTextureWidth = 20;
    private int hatTextureHeight = 80;
    private int hatStylesInSpriteWidth = 12;

    private int adornmentsTextureWidth = 16;
    private int adornmentsTextureHeight = 32;
    private int adornmentsStylesInSpriteWidth = 8;
    private int adornmentsSpriteWidth = 16;
    private int adornmentsSpriteHeight = 16;

    // 颜色替换列表
    private List<colorSwap> colorSwapList;

    // 手臂颜色替换目标值
    private Color32 armTargetColor1 = new Color32(77, 13, 13, 255);  // 最深色
    private Color32 armTargetColor2 = new Color32(138, 41, 41, 255);  // 次深色
    private Color32 armTargetColor3 = new Color32(172, 50, 50, 255);  // 最浅色

    //Target skin colours for color replacement 目标肤色用于颜色替换 
    private Color32 skinTargetColor1 = new Color32(145, 117, 90, 255); // darkest 
    private Color32 skinTargetColor2 = new Color32(204, 155, 108, 255); // next darkest 
    private Color32 skinTargetColor3 = new Color32(207, 166, 128, 255); // next darkest 
    private Color32 skinTargetColor4 = new Color32(238, 195, 154, 255); // lightest]

    private void Awake()
    {
        // 初始化颜色替换列表
        colorSwapList = new List<colorSwap>();

        // 执行自定义处理流程
        ProcessCustomisation();
    }

    private void ProcessCustomisation()
    {
        ProcessGender();    // 处理性别选择
        ProcessShirt();    // 处理衬衫样式
        ProcessArms();     // 处理手臂颜色
        ProcessTrousers();//处理裤子颜色
        ProcessHair();// 处理发型样式
        ProcessSkin();//处理皮肤样式
        ProcessHat();//处理帽子样式
        ProcessAdornments();//处理装饰物样式
        MergeCustomisations();  // 合并所有自定义项
    }

    private void ProcessGender()
    {
        // 根据性别设置基础纹理
        if (inputSex == 0)
        {
            farmerBaseTexture = maleFarmerBaseTexture;  // 选择男性纹理
        }
        else if (inputSex == 1)
        {
            farmerBaseTexture = femaleFarmerBaseTexture;  // 选择女性纹理
        }

        // 获取基础像素数据
        Color[] farmerBasePixels = farmerBaseTexture.GetPixels();

        // 设置自定义纹理的像素数据
        farmerBaseCustomised.SetPixels(farmerBasePixels);
        farmerBaseCustomised.Apply();  // 应用纹理更改
    }

    private void ProcessShirt()
    {
        // 初始化身体朝向数组
        bodyFacingArray = new Facing[bodyColumns, bodyRows];

        // 填充身体朝向数组数据
        PopulateBodyFacingArray();

        // 初始化衬衫偏移数组
        bodyShirtOffsetArray = new Vector2Int[bodyColumns, bodyRows];

        // 填充衬衫偏移数组数据
        PopulateBodyShirtOffsetArray();

        // 创建选定的衬衫纹理
        AddShirtToTexture(inputShirtStyleNo);

        // 将衬衫纹理应用到基础纹理上
        ApplyShirtTextureToBase();
    }

    private void ProcessArms()
    {
        // 获取需要重新着色的手臂像素区域（前288像素宽度）
        Color[] farmerPixelsToRecolour = farmerBaseTexture.GetPixels(0, 0, 288, farmerBaseTexture.height);

        // 填充手臂颜色替换列表
        PopulateArmColorSwapList();

        // 更改手臂像素颜色
        ChangePixelColors(farmerPixelsToRecolour, colorSwapList);

        // 设置重新着色后的像素数据
        farmerBaseCustomised.SetPixels(0, 0, 288, farmerBaseTexture.height, farmerPixelsToRecolour);

        // 应用纹理更改
        farmerBaseCustomised.Apply();
    }

    private void ProcessTrousers()
    {
        // Get trouser pixels to recolor获取裤子像素点以重新着色
        Color[] farmerTrouserPixels = farmerBaseTexture.GetPixels(288, 0, 96, farmerBaseTexture.height);

        // Change trouser colors更换裤子颜色
        TintPixelColors(farmerTrouserPixels, inputTrouserColor);

        // Set changed trouser pixels设置更改裤装像素
        farmerBaseCustomised.SetPixels(288, 0, 96, farmerBaseTexture.height, farmerTrouserPixels);

        // Apply texture changes应用纹理更改
        farmerBaseCustomised.Apply();
    }

    private void ProcessHair()
    {
        // Create Selected Hair Texture创建选定发质纹理
        AddHairToTexture(inputHairStyleNo);

        // Get hair pixels to recolor获取头发像素以重新着色
        Color[] farmerSelectedHairPixels = hairCustomised.GetPixels();

        // Tint hair pixels为头发像素上色
        TintPixelColors(farmerSelectedHairPixels, inputHairColor);

        hairCustomised.SetPixels(farmerSelectedHairPixels);
        hairCustomised.Apply();
    }

    private void ProcessSkin()
    {
        // Get skin pixels to recolor获取皮肤像素以重新着色
        Color[] farmerPixelsToRecolor = farmerBaseCustomised.GetPixels(0, 0, 288, farmerBaseTexture.height);

        // Populate Skin Color Swap List填充肤色替换列表
        PopulateSkinColorSwapList(inputSkinType);

        // Change skin colors更改皮肤颜色
        ChangePixelColors(farmerPixelsToRecolor, colorSwapList);

        // Set recoloured pixels设置重新着色的像素
        farmerBaseCustomised.SetPixels(0, 0, 288, farmerBaseTexture.height, farmerPixelsToRecolor);

        // Apply texture changes应用纹理更改
        farmerBaseCustomised.Apply();
    }

    private void ProcessHat()
    {
        // Create Selected Hat Texture
        AddHatToTexture(inputHatStyleNo);
    }

    private void TintPixelColors(Color[] basePixelArray, Color tintColor)
    {
        // Loop through pixels to tint 遍历像素进行着色
        for (int i = 0; i < basePixelArray.Length; i++)
        {
            basePixelArray[i].r = basePixelArray[i].r * tintColor.r;
            basePixelArray[i].g = basePixelArray[i].g * tintColor.g;
            basePixelArray[i].b = basePixelArray[i].b * tintColor.b;
        }
    }

    private void ProcessAdornments()
    {
        // Initialise body adornments offset array初始化身体装饰偏移量数组
        bodyAdornmentsOffsetArray = new Vector2Int[bodyColumns, bodyRows];

        // Populate body adornments offset array填充身体装饰偏移数组
        PopulateBodyAdornmentsOffsetArray();

        // Create Selected Adornments Texture创建选定装饰纹理
        AddAdornmentsToTexture(inputAdornmentsStyleNo);

        // Create new adornments base texture创建新的装饰基础纹理
        farmerBaseAdornmentsUpdated = new Texture2D(farmerBaseTexture.width, farmerBaseTexture.height);
        farmerBaseAdornmentsUpdated.filterMode = FilterMode.Point;

        // Set adornments base texture to transparent将装饰元素的基础纹理设置为透明
        SetTextureToTransparent(farmerBaseAdornmentsUpdated);
        ApplyAdornmentsTextureToBase();
    }

    private void MergeCustomisations()
    {
        // 获取衬衫像素数据
        Color[] farmerShirtPixels = farmerBaseShirtsUpdated.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);

        // 获取裤子像素选择区域（从288像素开始，宽度96像素）
        Color[] farmerTrouserPixelsSelection = farmerBaseCustomised.GetPixels(288, 0, 96, farmerBaseTexture.height);

        // Farmer Adornments Pixels获取农夫装饰像素
        Color[] farmerAdornmentsPixels = farmerBaseAdornmentsUpdated.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);

        // 获取身体像素数据
        Color[] farmerBodyPixels = farmerBaseCustomised.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);

        // 合并颜色数组
        MergeColourArray(farmerBodyPixels, farmerTrouserPixelsSelection);
        MergeColourArray(farmerBodyPixels, farmerShirtPixels);
        MergeColourArray(farmerBodyPixels, farmerAdornmentsPixels);

        // 粘贴合并后的像素数据
        farmerBaseCustomised.SetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height, farmerBodyPixels);

        // 应用纹理更改
        farmerBaseCustomised.Apply();
    }

    private void ApplyAdornmentsTextureToBase()
    {
        Color[] frontAdornmentsPixels;
        Color[] rightAdornmentsPixels;

        frontAdornmentsPixels = selectedAdornments.GetPixels(0, adornmentsSpriteHeight * 1, adornmentsSpriteWidth, adornmentsSpriteHeight);
        rightAdornmentsPixels = selectedAdornments.GetPixels(0, adornmentsSpriteHeight * 0, adornmentsSpriteWidth, adornmentsSpriteHeight);

        // Loop through base texture and apply adornments pixels遍历基础纹理并应用装饰像素
        for (int x = 0; x < bodyColumns; x++)
        {
            for (int y = 0; y < bodyRows; y++)
            {
                int pixelX = x * farmerSpriteWidth;
                int pixelY = y * farmerSpriteHeight;

                if (bodyAdornmentsOffsetArray[x, y] != null)
                {
                    pixelX += bodyAdornmentsOffsetArray[x, y].x;
                    pixelY += bodyAdornmentsOffsetArray[x, y].y;
                }

                // Switch on facing direction切换朝向
                switch (bodyFacingArray[x, y])
                {
                    case Facing.none:
                        break;

                    case Facing.front:
                        // Populate front adornments pixels填充前饰件像素
                        farmerBaseAdornmentsUpdated.SetPixels(pixelX, pixelY, adornmentsSpriteWidth, adornmentsSpriteHeight, frontAdornmentsPixels);
                        break;

                    case Facing.right:
                        // Populate right adornments pixels填充右侧装饰像素
                        farmerBaseAdornmentsUpdated.SetPixels(pixelX, pixelY, adornmentsSpriteWidth, adornmentsSpriteHeight, rightAdornmentsPixels);
                        break;

                    default:
                        break;
                }
            }
        }

        // Apply adornments texture pixels应用装饰纹理像素
        farmerBaseAdornmentsUpdated.Apply();
    }

    private void PopulateArmColorSwapList()
    {
        // 清空颜色替换列表
        colorSwapList.Clear();

        // 添加手臂颜色替换规则，从衬衫纹理中获取对应颜色
        colorSwapList.Add(new colorSwap(armTargetColor1, selectedShirt.GetPixel(0, 7)));  // 使用衬衫第7行的颜色
        colorSwapList.Add(new colorSwap(armTargetColor2, selectedShirt.GetPixel(0, 6)));  // 使用衬衫第6行的颜色
        colorSwapList.Add(new colorSwap(armTargetColor3, selectedShirt.GetPixel(0, 5)));  // 使用衬衫第5行的颜色
    }

    private void PopulateSkinColorSwapList(int skinType)
    {
        // Clear color swap list清除颜色替换列表
        colorSwapList.Clear();

        // Skin replacement colors皮肤替换颜色
        //Switch on skin type切换肤质类型
        switch (skinType)
        {
            case 0:
                colorSwapList.Add(new colorSwap(skinTargetColor1, skinTargetColor1));
                colorSwapList.Add(new colorSwap(skinTargetColor2, skinTargetColor2));
                colorSwapList.Add(new colorSwap(skinTargetColor3, skinTargetColor3));
                colorSwapList.Add(new colorSwap(skinTargetColor4, skinTargetColor4));
                break;

            case 1:
                colorSwapList.Add(new colorSwap(skinTargetColor1, new Color32(187, 157, 128, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor2, new Color32(231, 187, 144, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor3, new Color32(221, 186, 154, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor4, new Color32(213, 189, 167, 255)));
                break;

            case 2:
                colorSwapList.Add(new colorSwap(skinTargetColor1, new Color32(105, 69, 2, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor2, new Color32(128, 87, 12, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor3, new Color32(145, 103, 26, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor4, new Color32(161, 114, 25, 255)));
                break;

            case 3:
                colorSwapList.Add(new colorSwap(skinTargetColor1, new Color32(151, 132, 0, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor2, new Color32(187, 166, 15, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor3, new Color32(209, 188, 39, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor4, new Color32(211, 199, 112, 255)));
                break;

            default:
                colorSwapList.Add(new colorSwap(skinTargetColor1, skinTargetColor1));
                colorSwapList.Add(new colorSwap(skinTargetColor2, skinTargetColor2));
                colorSwapList.Add(new colorSwap(skinTargetColor3, skinTargetColor3));
                colorSwapList.Add(new colorSwap(skinTargetColor4, skinTargetColor4));
                break;

        }
    }

    private void ChangePixelColors(Color[] baseArray, List<colorSwap> colorSwapList)
    {
        // 遍历基础数组中的所有像素
        for (int i = 0; i < baseArray.Length; i++)
        {
            // 遍历颜色替换列表进行颜色匹配和替换
            if (colorSwapList.Count > 0)
            {
                for (int j = 0; j < colorSwapList.Count; j++)
                {
                    if (isSameColor(baseArray[i], colorSwapList[j].fromColor))  // 如果颜色匹配
                    {
                        baseArray[i] = colorSwapList[j].toColor;  // 执行颜色替换
                    }
                }
            }
        }
    }

    private bool isSameColor(Color color1, Color color2)
    {
        // 精确比较两个颜色的RGBA值是否完全相同
        if (color1 == color2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void MergeColourArray(Color[] baseArray, Color[] mergeArray)
    {
        // 遍历所有像素进行颜色合并
        for (int i = 0; i < baseArray.Length; i++)
        {
            if (mergeArray[i].a > 0)  // 如果合并数组有颜色（透明度>0）
            {
                if (mergeArray[i].a >= 1)  // 完全替换（完全不透明）
                {
                    baseArray[i] = mergeArray[i];
                }
                else  // 插值混合颜色（部分透明）
                {
                    float alpha = mergeArray[i].a;  // 获取透明度值

                    // 使用透明度进行颜色插值计算
                    baseArray[i].r += (mergeArray[i].r - baseArray[i].r) * alpha;
                    baseArray[i].g += (mergeArray[i].g - baseArray[i].g) * alpha;
                    baseArray[i].b += (mergeArray[i].b - baseArray[i].b) * alpha;
                    baseArray[i].a += mergeArray[i].a;
                }
            }
        }
    }

    private void AddHairToTexture(int hairStyleNo)
    {
        // Calculate coordinates for hair pixels计算头发像素的坐标
        int y = (hairStyleNo / hairStylesInSpriteWidth) * hairTextureHeight;
        int x = (hairStyleNo % hairStylesInSpriteWidth) * hairTextureWidth;

        // Get hair pixels获取头发像素
        Color[] hairPixels = hairBaseTexture.GetPixels(x, y, hairTextureWidth, hairTextureHeight);

        // Apply selected hair pixels to texture将选中的头发像素应用到纹理上
        hairCustomised.SetPixels(hairPixels);
        hairCustomised.Apply();
    }

    private void AddHatToTexture(int hatStyleNo)
    {
        // Calculate coordinates for hat pixels计算帽子像素的坐标
        int y = (hatStyleNo / hatStylesInSpriteWidth) * hatTextureHeight;
        int x = (hatStyleNo % hatStylesInSpriteWidth) * hatTextureWidth;

        // Get hat pixels获取帽子像素
        Color[] hatPixels = hatsBaseTexture.GetPixels(x, y, hatTextureWidth, hatTextureHeight);

        // Apply selected hat pixels to texture将选定的帽子像素应用到纹理上
        hatsCustomised.SetPixels(hatPixels);
        hatsCustomised.Apply();
    }

    private void AddAdornmentsToTexture(int adornmentsStyleNo)
    {
        // Create adornment texture创建装饰纹理
        selectedAdornments = new Texture2D(adornmentsTextureWidth, adornmentsTextureHeight);
        selectedAdornments.filterMode = FilterMode.Point;

        // Calculate coordinates for adornments pixels计算装饰像素的坐标
        int y = (adornmentsStyleNo / adornmentsStylesInSpriteWidth) * adornmentsTextureHeight;
        int x = (adornmentsStyleNo % adornmentsStylesInSpriteWidth) * adornmentsTextureWidth;

        // Get adornments pixels获取装饰像素
        Color[] adornmentsPixels = adornmentsBaseTexture.GetPixels(x, y, adornmentsTextureWidth, adornmentsTextureHeight);

        // Apply selected adornments pixels to texture将选定的装饰像素应用于纹理
        selectedAdornments.SetPixels(adornmentsPixels);
        selectedAdornments.Apply();
    }

    private void AddShirtToTexture(int shirtStyleNo)
    {
        // 创建选定衬衫纹理
        selectedShirt = new Texture2D(shirtTextureWidth, shirtTextureHeight);
        selectedShirt.filterMode = FilterMode.Point;  // 设置过滤模式为点过滤（保持像素清晰）

        // 计算衬衫像素在纹理图集中的坐标位置
        int y = (shirtStyleNo / shirtStylesInSpriteWidth) * shirtTextureHeight;
        int x = (shirtStyleNo % shirtStylesInSpriteWidth) * shirtTextureWidth;

        // 获取衬衫像素数据
        Color[] shirtPixels = shirtsBaseTexture.GetPixels(x, y, shirtTextureWidth, shirtTextureHeight);

        // 将选定的衬衫像素应用到纹理上
        selectedShirt.SetPixels(shirtPixels);
        selectedShirt.Apply();
    }

    private void ApplyShirtTextureToBase()
    {
        // 创建新的衬衫基础纹理
        farmerBaseShirtsUpdated = new Texture2D(farmerBaseTexture.width, farmerBaseTexture.height);
        farmerBaseShirtsUpdated.filterMode = FilterMode.Point;

        // 将衬衫基础纹理设置为透明
        SetTextureToTransparent(farmerBaseShirtsUpdated);

        Color[] frontShirtPixels;  // 正面衬衫像素
        Color[] backShirtPixels;   // 背面衬衫像素
        Color[] rightShirtPixels;  // 右侧衬衫像素

        // 从选定衬衫纹理中提取不同朝向的衬衫像素
        frontShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 3, shirtSpriteWidth, shirtSpriteHeight);  // 正面（第3行）
        backShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 0, shirtSpriteWidth, shirtSpriteHeight);   // 背面（第0行）
        rightShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 2, shirtSpriteWidth, shirtSpriteHeight);  // 右侧（第2行）

        // 遍历基础纹理并应用衬衫像素
        for (int x = 0; x < bodyColumns; x++)
        {
            for (int y = 0; y < bodyRows; y++)
            {
                int pixelX = x * farmerSpriteWidth;   // 计算像素X坐标
                int pixelY = y * farmerSpriteHeight;  // 计算像素Y坐标

                if (bodyShirtOffsetArray[x, y] != null)
                {
                    if (bodyShirtOffsetArray[x, y].x == 99 && bodyShirtOffsetArray[x, y].y == 99) // 特殊标记：不填充衬衫
                        continue;

                    // 应用衬衫偏移量
                    pixelX += bodyShirtOffsetArray[x, y].x;
                    pixelY += bodyShirtOffsetArray[x, y].y;
                }

                // 根据朝向方向切换衬衫像素应用
                switch (bodyFacingArray[x, y])
                {
                    case Facing.none:
                        break;

                    case Facing.front:
                        // 填充正面衬衫像素
                        farmerBaseShirtsUpdated.SetPixels(pixelX, pixelY, shirtSpriteWidth, shirtSpriteHeight, frontShirtPixels);
                        break;

                    case Facing.back:
                        // 填充背面衬衫像素
                        farmerBaseShirtsUpdated.SetPixels(pixelX, pixelY, shirtSpriteWidth, shirtSpriteHeight, backShirtPixels);
                        break;

                    case Facing.right:
                        // 填充右侧衬衫像素
                        farmerBaseShirtsUpdated.SetPixels(pixelX, pixelY, shirtSpriteWidth, shirtSpriteHeight, rightShirtPixels);
                        break;

                    default:
                        break;
                }
            }
        }

        // 应用衬衫纹理像素更改
        farmerBaseShirtsUpdated.Apply();
    }

    private void SetTextureToTransparent(Texture2D texture2D)
    {
        // 创建透明填充数组
        Color[] fill = new Color[texture2D.height * texture2D.width];
        for (int i = 0; i < fill.Length; i++)
        {
            fill[i] = Color.clear;  // 设置为完全透明
        }
        texture2D.SetPixels(fill);  // 应用透明像素
    }

    // 以下为硬编码的身体朝向和衬衫偏移数据填充方法
    // 这些方法定义了角色动画帧中每个精灵的朝向和衬衫位置
    private void PopulateBodyFacingArray()
    {
        // 详细的身体朝向数组初始化代码
        // 定义了6列21行网格中每个位置的朝向（正面、背面、右侧或无）
        bodyFacingArray[0, 0] = Facing.none;//第0列第0行：无朝向
        bodyFacingArray[1, 0] = Facing.none;// ... 第0-9行同样全部设置为 Facing.none
        bodyFacingArray[2, 0] = Facing.none;
        bodyFacingArray[3, 0] = Facing.none;
        bodyFacingArray[4, 0] = Facing.none;
        bodyFacingArray[5, 0] = Facing.none;

        bodyFacingArray[0, 1] = Facing.none;
        bodyFacingArray[1, 1] = Facing.none;
        bodyFacingArray[2, 1] = Facing.none;
        bodyFacingArray[3, 1] = Facing.none;
        bodyFacingArray[4, 1] = Facing.none;
        bodyFacingArray[5, 1] = Facing.none;

        bodyFacingArray[0, 2] = Facing.none;
        bodyFacingArray[1, 2] = Facing.none;
        bodyFacingArray[2, 2] = Facing.none;
        bodyFacingArray[3, 2] = Facing.none;
        bodyFacingArray[4, 2] = Facing.none;
        bodyFacingArray[5, 2] = Facing.none;

        bodyFacingArray[0, 3] = Facing.none;
        bodyFacingArray[1, 3] = Facing.none;
        bodyFacingArray[2, 3] = Facing.none;
        bodyFacingArray[3, 3] = Facing.none;
        bodyFacingArray[4, 3] = Facing.none;
        bodyFacingArray[5, 3] = Facing.none;

        bodyFacingArray[0, 4] = Facing.none;
        bodyFacingArray[1, 4] = Facing.none;
        bodyFacingArray[2, 4] = Facing.none;
        bodyFacingArray[3, 4] = Facing.none;
        bodyFacingArray[4, 4] = Facing.none;
        bodyFacingArray[5, 4] = Facing.none;

        bodyFacingArray[0, 5] = Facing.none;
        bodyFacingArray[1, 5] = Facing.none;
        bodyFacingArray[2, 5] = Facing.none;
        bodyFacingArray[3, 5] = Facing.none;
        bodyFacingArray[4, 5] = Facing.none;
        bodyFacingArray[5, 5] = Facing.none;

        bodyFacingArray[0, 6] = Facing.none;
        bodyFacingArray[1, 6] = Facing.none;
        bodyFacingArray[2, 6] = Facing.none;
        bodyFacingArray[3, 6] = Facing.none;
        bodyFacingArray[4, 6] = Facing.none;
        bodyFacingArray[5, 6] = Facing.none;

        bodyFacingArray[0, 7] = Facing.none;
        bodyFacingArray[1, 7] = Facing.none;
        bodyFacingArray[2, 7] = Facing.none;
        bodyFacingArray[3, 7] = Facing.none;
        bodyFacingArray[4, 7] = Facing.none;
        bodyFacingArray[5, 7] = Facing.none;

        bodyFacingArray[0, 8] = Facing.none;
        bodyFacingArray[1, 8] = Facing.none;
        bodyFacingArray[2, 8] = Facing.none;
        bodyFacingArray[3, 8] = Facing.none;
        bodyFacingArray[4, 8] = Facing.none;
        bodyFacingArray[5, 8] = Facing.none;

        bodyFacingArray[0, 9] = Facing.none;
        bodyFacingArray[1, 9] = Facing.none;
        bodyFacingArray[2, 9] = Facing.none;
        bodyFacingArray[3, 9] = Facing.none;
        bodyFacingArray[4, 9] = Facing.none;
        bodyFacingArray[5, 9] = Facing.none;

        //具体朝向设置‌
        bodyFacingArray[0, 10] = Facing.back;// 第0列第10行：背面朝向
        bodyFacingArray[1, 10] = Facing.back;// 第1列第10行：背面朝向
        bodyFacingArray[2, 10] = Facing.right;// 第2列第10行：右侧朝向
        bodyFacingArray[3, 10] = Facing.right;// ...右侧朝向
        bodyFacingArray[4, 10] = Facing.right;
        bodyFacingArray[5, 10] = Facing.right;

        bodyFacingArray[0, 11] = Facing.front;
        bodyFacingArray[1, 11] = Facing.front;
        bodyFacingArray[2, 11] = Facing.front;
        bodyFacingArray[3, 11] = Facing.front;
        bodyFacingArray[4, 11] = Facing.back;
        bodyFacingArray[5, 11] = Facing.back;

        bodyFacingArray[0, 12] = Facing.back;
        bodyFacingArray[1, 12] = Facing.back;
        bodyFacingArray[2, 12] = Facing.right;
        bodyFacingArray[3, 12] = Facing.right;
        bodyFacingArray[4, 12] = Facing.right;
        bodyFacingArray[5, 12] = Facing.right;

        bodyFacingArray[0, 13] = Facing.front;
        bodyFacingArray[1, 13] = Facing.front;
        bodyFacingArray[2, 13] = Facing.front;
        bodyFacingArray[3, 13] = Facing.front;
        bodyFacingArray[4, 13] = Facing.back;
        bodyFacingArray[5, 13] = Facing.back;


        bodyFacingArray[0, 14] = Facing.back;
        bodyFacingArray[1, 14] = Facing.back;
        bodyFacingArray[2, 14] = Facing.right;
        bodyFacingArray[3, 14] = Facing.right;
        bodyFacingArray[4, 14] = Facing.right;
        bodyFacingArray[5, 14] = Facing.right;


        bodyFacingArray[0, 15] = Facing.front;
        bodyFacingArray[1, 15] = Facing.front;
        bodyFacingArray[2, 15] = Facing.front;
        bodyFacingArray[3, 15] = Facing.front;
        bodyFacingArray[4, 15] = Facing.back;
        bodyFacingArray[5, 15] = Facing.back;


        bodyFacingArray[0, 16] = Facing.back;
        bodyFacingArray[1, 16] = Facing.back;
        bodyFacingArray[2, 16] = Facing.right;
        bodyFacingArray[3, 16] = Facing.right;
        bodyFacingArray[4, 16] = Facing.right;
        bodyFacingArray[5, 16] = Facing.right;

        bodyFacingArray[0, 17] = Facing.front;
        bodyFacingArray[1, 17] = Facing.front;
        bodyFacingArray[2, 17] = Facing.front;
        bodyFacingArray[3, 17] = Facing.front;
        bodyFacingArray[4, 17] = Facing.back;
        bodyFacingArray[5, 17] = Facing.back;

        bodyFacingArray[0, 18] = Facing.back;
        bodyFacingArray[1, 18] = Facing.back;
        bodyFacingArray[2, 18] = Facing.back;
        bodyFacingArray[3, 18] = Facing.right;
        bodyFacingArray[4, 18] = Facing.right;
        bodyFacingArray[5, 18] = Facing.right;

        bodyFacingArray[0, 19] = Facing.right;
        bodyFacingArray[1, 19] = Facing.right;
        bodyFacingArray[2, 19] = Facing.right;
        bodyFacingArray[3, 19] = Facing.front;
        bodyFacingArray[4, 19] = Facing.front;
        bodyFacingArray[5, 19] = Facing.front;

        bodyFacingArray[0, 20] = Facing.front;
        bodyFacingArray[1, 20] = Facing.front;
        bodyFacingArray[2, 20] = Facing.front;
        bodyFacingArray[3, 20] = Facing.back;
        bodyFacingArray[4, 20] = Facing.back;
        bodyFacingArray[5, 20] = Facing.back;

    }

    private void PopulateBodyAdornmentsOffsetArray()
    {
        bodyAdornmentsOffsetArray[0, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 1] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 2] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 3] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 4] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 5] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 6] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 7] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 8] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 9] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 10] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 10] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 10] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[3, 10] = new Vector2Int(0, 2 + 16);
        bodyAdornmentsOffsetArray[4, 10] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[5, 10] = new Vector2Int(0, 0 + 16);

        bodyAdornmentsOffsetArray[0, 11] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[1, 11] = new Vector2Int(0, 2 + 16);
        bodyAdornmentsOffsetArray[2, 11] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[3, 11] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 11] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 11] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 12] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 12] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 12] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[3, 12] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[4, 12] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 12] = new Vector2Int(0, -1 + 16);

        bodyAdornmentsOffsetArray[0, 13] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[1, 13] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[2, 13] = new Vector2Int(1, -1 + 16);
        bodyAdornmentsOffsetArray[3, 13] = new Vector2Int(1, -1 + 16);
        bodyAdornmentsOffsetArray[4, 13] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 13] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 14] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 14] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 14] = new Vector2Int(0, -3 + 16);
        bodyAdornmentsOffsetArray[3, 14] = new Vector2Int(0, -5 + 16);
        bodyAdornmentsOffsetArray[4, 14] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 14] = new Vector2Int(0, 1 + 16);

        bodyAdornmentsOffsetArray[0, 15] = new Vector2Int(0, -2 + 16);
        bodyAdornmentsOffsetArray[1, 15] = new Vector2Int(0, -5 + 16);
        bodyAdornmentsOffsetArray[2, 15] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 15] = new Vector2Int(0, 2 + 16);
        bodyAdornmentsOffsetArray[4, 15] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 15] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 16] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 16] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 16] = new Vector2Int(0, -3 + 16);
        bodyAdornmentsOffsetArray[3, 16] = new Vector2Int(0, -2 + 16);
        bodyAdornmentsOffsetArray[4, 16] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 16] = new Vector2Int(0, 0 + 16);

        bodyAdornmentsOffsetArray[0, 17] = new Vector2Int(0, -3 + 16);
        bodyAdornmentsOffsetArray[1, 17] = new Vector2Int(0, -2 + 16);
        bodyAdornmentsOffsetArray[2, 17] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 17] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 17] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 17] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 18] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 18] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 18] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 18] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 18] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 18] = new Vector2Int(0, -1 + 16);

        bodyAdornmentsOffsetArray[0, 19] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[1, 19] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[2, 19] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 19] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 19] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 19] = new Vector2Int(0, -1 + 16);

        bodyAdornmentsOffsetArray[0, 20] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[1, 20] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[2, 20] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 20] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 20] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 20] = new Vector2Int(99, 99);
    }


    private void PopulateBodyShirtOffsetArray()
    {
        // 详细的衬衫偏移数组初始化代码
        // 定义了衬衫在每个精灵上的精确位置偏移量
        // 使用Vector2Int(99, 99)表示该位置不显示衬衫
        bodyShirtOffsetArray[0, 0] = new Vector2Int(99, 99);// 特殊标记：不显示衬衫
        bodyShirtOffsetArray[1, 0] = new Vector2Int(99, 99);// ...0到9行不显示衬衫
        bodyShirtOffsetArray[2, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 0] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 1] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 2] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 3] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 4] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 5] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 6] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 7] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 8] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 9] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 10] = new Vector2Int(4, 11);// 第0列第10行：X偏移4像素，Y偏移11像素
        bodyShirtOffsetArray[1, 10] = new Vector2Int(4, 10);// 第1列第10行：X偏移4像素，Y偏移10像素
        bodyShirtOffsetArray[2, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[3, 10] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[4, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[5, 10] = new Vector2Int(4, 10);

        bodyShirtOffsetArray[0, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[1, 11] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[2, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[3, 11] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[5, 11] = new Vector2Int(4, 12);

        bodyShirtOffsetArray[0, 12] = new Vector2Int(3, 9);
        bodyShirtOffsetArray[1, 12] = new Vector2Int(3, 9);
        bodyShirtOffsetArray[2, 12] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[3, 12] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[4, 12] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 12] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 13] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 13] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 13] = new Vector2Int(5, 9);
        bodyShirtOffsetArray[3, 13] = new Vector2Int(5, 9);
        bodyShirtOffsetArray[4, 13] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[5, 13] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 14] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[1, 14] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[2, 14] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[3, 14] = new Vector2Int(4, 5);
        bodyShirtOffsetArray[4, 14] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 14] = new Vector2Int(4, 12);

        bodyShirtOffsetArray[0, 15] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[1, 15] = new Vector2Int(4, 5);
        bodyShirtOffsetArray[2, 15] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 15] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[4, 15] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[5, 15] = new Vector2Int(4, 5);

        bodyShirtOffsetArray[0, 16] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[1, 16] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[2, 16] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[3, 16] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[4, 16] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 16] = new Vector2Int(4, 10);

        bodyShirtOffsetArray[0, 17] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[1, 17] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[2, 17] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 17] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 17] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[5, 17] = new Vector2Int(4, 8);

        bodyShirtOffsetArray[0, 18] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 18] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 18] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 19] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 19] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 19] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 20] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 20] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 20] = new Vector2Int(4, 9);
    }

    //系统总体架构
    //这是一个基于Unity的2D角色外观定制系统，采用**模块化流水线设计**，通过四个核心处理阶段实现完整的角色自定义功能。
    //核心处理流程：
    //1. 初始化阶段 (Awake方法)
    // 创建颜色替换列表
    // 启动自定义处理流水线

    //2. 主处理流水线 ( ProcessCustomisation 方法)
    //系统按照严格顺序执行以下四个处理步骤：
    //处理顺序：性别 → 衬衫 → 手臂颜色 → 最终合成
    //详细处理逻辑分解
    //### **第一阶段：性别处理 ( ProcessGender )
    //逻辑流程：
    //1. 性别判断：根据`inputSex`参数选择基础纹理
    // - `0` → 男性角色纹理 (`maleFarmerBaseTexture`)
    // - `1` → 女性角色纹理 (`femaleFarmerBaseTexture`)
    //2. **像素数据提取**：获取选定性别的基础纹理所有像素
    //3. **基础纹理设置**：将基础像素应用到最终的自定义纹理上
    //**关键作用**：建立角色外观的基础框架

    //### **第二阶段：衬衫处理 ( ProcessShirt )**
    //这是系统中最复杂的部分，包含多个子步骤：
    //#### **子步骤1：朝向系统初始化**
    //- 创建`bodyFacingArray`数组（6列×21行）
    //- 定义每个动画帧中角色的朝向（正面、背面、右侧）
    //#### **子步骤2：衬衫定位系统**
    //- 创建`bodyShirtOffsetArray`数组
    //- 精确定义衬衫在每个动画帧上的位置偏移量
    //#### **子步骤3：衬衫纹理创建**
    //- 从衬衫合集纹理(`shirtsBaseTexture`)中提取特定样式
    //- 计算坐标：`x = (shirtStyleNo % 16) × 9`
    //- 创建独立的衬衫纹理对象
    //#### **子步骤4：衬衫应用**
    //- 根据朝向和偏移数据，将衬衫纹理精确贴合到角色身上

    //### **第三阶段：手臂颜色处理 (ProcessArms)**
    //**智能颜色匹配逻辑：**
    //1. **手臂区域识别**：提取纹理前288像素宽度区域（专门的手臂区域）
    //2. **颜色替换规则建立**：
    //   - 手臂基础颜色1 → 衬衫第7行颜色
    //   - 手臂基础颜色2 → 衬衫第6行颜色  
    //   - 手臂基础颜色3 → 衬衫第5行颜色
    //3. **像素级颜色替换**：
    //   - 遍历所有手臂像素
    //   - 匹配预设的基础颜色
    //   - 执行精确的颜色替换

    //### **第四阶段：最终合成 (MergeCustomisations)**
    //**多图层融合逻辑：**
    //1. **像素数据提取**：
    //   - 衬衫像素（0-288宽度）
    //   - 裤子像素（288-384宽度）
    //   - 身体基础像素
    //2. **智能融合算法**：
    //   - **完全替换**：当衬衫/裤子像素完全不透明时
    //   - **插值混合**：当存在透明度时，进行颜色插值计算
    //##  **关键技术实现细节**
    //### **颜色替换机制**（ChangePixelColors）
    //### **纹理坐标系统**
    //- **角色精灵**：16×32像素（单个动画帧）
    //- **衬衫精灵**：9×9像素
    //- **网格布局**：6列×21行（共126个动画帧）
    //### **朝向处理逻辑**
    //系统为每个动画帧定义精确的朝向：
    //- **正面(Front)**：显示正面衬衫纹理
    //- **背面(Back)**：显示背面衬衫纹理  
    //- **右侧(Right)**：显示右侧衬衫纹理


}