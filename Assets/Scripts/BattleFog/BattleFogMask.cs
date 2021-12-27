using UnityEngine;



[System.Serializable]
public class BattleFogMask : MonoBehaviour
{
    [RangeAttribute(0, 0.5f)]
    public float            keyFrameRandom;

    public int              width;
    public int              height;
    public float            radius;
    public Texture2D        orgTexture;
    
    /// <summary>
    /// 修改后的雾掩码
    /// </summary>
    private Texture2D       maskTexture;
    private Color[]         final_colors;

    public  AnimationCurve  circleAc;

    public Texture2D        MaskTex
    {
        get { return maskTexture; }
    }

    /// <summary>   
    /// 从本地或原始文件创建
    /// </summary>
    public void CreateMaskTexture( int cityID )
    {
        if (maskTexture == null)
            maskTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        final_colors    = new Color[width * height];
        {
            Color[] region_colors = new Color[width * height];
            for (int l = 0; l < region_colors.Length; l++)
            {
                region_colors[l] = orgTexture.GetPixel(l % width, l / width);
            }
        }
    }

    public void ApplyOrgTexture( )
    {
        Color[] region_colors = new Color[width * height];
        for (int l = 0; l < region_colors.Length; l++)
        {
            region_colors[l] = orgTexture.GetPixel(l % width, l / width);
        }
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = final_colors[y * width + x];
                Color regioncolor = region_colors[y * width + x];
                if (color.r > regioncolor.r)
                {
                    final_colors[y * width + x] = regioncolor;
                }
            }
        }

        maskTexture.SetPixels(final_colors);
        maskTexture.Apply();
    }

    public void ModifyMask(float[,] points )
    {
        
    }
}
