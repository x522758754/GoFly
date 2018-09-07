
using UnityEngine;
using UnityEngine.Events;
using System;

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Cinematic/Color Grading")]
#if UNITY_5_4_OR_NEWER
[ImageEffectAllowedInSceneView]
#endif


public class TonemappingColorGrading : MonoBehaviour
{
#if UNITY_EDITOR
    // EDITOR ONLY call for allowing the editor to update the histogram
    public UnityAction<RenderTexture> onFrameEndEditorOnly;

    [SerializeField]
    private ComputeShader m_HistogramComputeShader;
    public ComputeShader histogramComputeShader
    {
        get
        {
            if (m_HistogramComputeShader == null)
                m_HistogramComputeShader = Resources.Load<ComputeShader>("HistogramCompute");

            return m_HistogramComputeShader;
        }
    }

    [SerializeField]
    private Shader m_HistogramShader;
    public Shader histogramShader
    {
        get
        {
            if (m_HistogramShader == null)
                m_HistogramShader = Shader.Find("Hidden/TonemappingColorGradingHistogram");

            return m_HistogramShader;
        }
    }

    [SerializeField]
    public bool histogramRefreshOnPlay = false;
#endif

    #region Attributes
    [AttributeUsage(AttributeTargets.Field)]
    public class SettingsGroup : Attribute
    { }

    public class IndentedGroup : PropertyAttribute
    { }

    public class ChannelMixer : PropertyAttribute
    { }

    public class ColorWheelGroup : PropertyAttribute
    {
        public int minSizePerWheel = 60;
        public int maxSizePerWheel = 300;

        public ColorWheelGroup()
        { }

        public ColorWheelGroup(int minSizePerWheel, int maxSizePerWheel)
        {
            this.minSizePerWheel = minSizePerWheel;
            this.maxSizePerWheel = maxSizePerWheel;
        }
    }

    public class Curve : PropertyAttribute
    {
        public Color color = Color.white;

        public Curve()
        { }

        public Curve(float r, float g, float b, float a) // Can't pass a struct in an attribute
        {
            color = new Color(r, g, b, a);
        }
    }
    #endregion

    #region Settings
    [Serializable]
    public struct LUTSettings
    {
        public bool enabled;

        [Tooltip("Custom lookup texture (strip format, e.g. 256x16).")]
        public Texture texture;

        [Range(0f, 1f), Tooltip("Blending factor.")]
        public float contribution;

        public static LUTSettings defaultSettings
        {
            get
            {
                return new LUTSettings
                {
                    enabled = true,
                    texture = null,
                    contribution = 1f
                };
            }
        }
    }

    //[Serializable]
    //public struct ColorWheelsSettings
    //{
    //[ColorUsage(true)]
    //public Color shadows;

    //[ColorUsage(true)]
    //public Color midtones;

    //[ColorUsage(true)]
    //public Color highlights;

    //public static ColorWheelsSettings defaultSettings
    //{
    //get
    //{
    //return new ColorWheelsSettings
    //{
    //shadows = Color.white,
    //midtones = Color.white,
    //highlights = Color.white
    //};
    //}
    //}
    //}

    [Serializable]
    public struct BasicsSettings
    {
        public Color shadows;
        public Color midtones;
        public Color highlights;
        [Range(0f, 1f), Tooltip("shadowspower")]
        public float shadowspower;
        [Range(0f, 1f), Tooltip("midtonespower")]
        public float midtonespower;
        [Range(0f, 1f), Tooltip("highlightspower")]
        public float highlightspower;

        [Range(-2f, 2f), Tooltip("调节白平衡色温")]
        public float temperatureShift;

        [Range(-2f, 2f), Tooltip("调节白平衡补色")]
        public float tint;

        [Space, Range(-0.5f, 0.5f), Tooltip("调节屏幕色彩")]
        public float hue;

        [Range(0f, 2f), Tooltip("调节屏幕饱和度")]
        public float saturation;

        [Range(-1f, 1f), Tooltip("自然饱和度校正，将场景饱和度低于这个值的像素饱和度拉高")]
        public float vibrance;

        [Range(0f, 10f), Tooltip("对屏幕的亮度进行调节，整体变亮或者变暗")]
        public float value;

        [Space, Range(0f, 5f), Tooltip("对比度调节")]
        public float contrast;

        [Range(0.01f, 5f), Tooltip("对比度曲线调节器. 用于对比度过大之后的微调")]
        public float gain;

        [Range(0.01f, 5f), Tooltip("放大色域")]
        public float gamma;

        public static BasicsSettings defaultSettings
        {
            get
            {
                return new BasicsSettings
                {
                    shadowspower = 0.0f,
                    midtonespower = 0.5f,
                    highlightspower = 0.5f,
                    temperatureShift = 0f,
                    tint = 0f,
                    contrast = 1f,
                    hue = 0f,
                    saturation = 1f,
                    value = 1f,
                    vibrance = 0f,
                    gain = 1f,
                    gamma = 1f,
                    shadows = Color.white,
                    midtones = Color.white,
                    highlights = Color.white
                };
            }
        }
    }

    [Serializable]
    public struct ChannelMixerSettings
    {
        public int currentChannel;
        public Vector3[] channels;

        public static ChannelMixerSettings defaultSettings
        {
            get
            {
                return new ChannelMixerSettings
                {
                    currentChannel = 0,
                    channels = new[]
                    {
                            new Vector3(1f, 0f, 0f),
                            new Vector3(0f, 1f, 0f),
                            new Vector3(0f, 0f, 1f)
                        }
                };
            }
        }
    }

    [Serializable]
    public struct CurvesSettings
    {
        [Curve]
        public AnimationCurve master;

        [Curve(1f, 0f, 0f, 1f)]
        public AnimationCurve red;

        [Curve(0f, 1f, 0f, 1f)]
        public AnimationCurve green;

        [Curve(0f, 1f, 1f, 1f)]
        public AnimationCurve blue;

        public static CurvesSettings defaultSettings
        {
            get
            {
                return new CurvesSettings
                {
                    master = defaultCurve,
                    red = defaultCurve,
                    green = defaultCurve,
                    blue = defaultCurve
                };
            }
        }

        public static AnimationCurve defaultCurve
        {
            get { return new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f)); }
        }
    }

    public enum ColorGradingPrecision
    {
        Normal = 16,
        //High = 32
    }

    [Serializable]
    public struct ColorGradingSettings
    {
        public bool enabled;
        [Tooltip(" LUT纹理精度. \"Normal\"生成后的LUT贴图是256x16格式. Prefer \"Normal\" 在移动平台上使用normal精度就合适了.")]
        public ColorGradingPrecision precision;

        //[Space, ColorWheelGroup]
        //public ColorWheelsSettings colorWheels;

        [Space, IndentedGroup]
        public BasicsSettings basics;

        [Space, ChannelMixer]
        public ChannelMixerSettings channelMixer;

        [Space, IndentedGroup]
        public CurvesSettings curves;

        //[Space, Tooltip("Use dithering to try and minimize color banding in dark areas.")]
        //public bool useDithering;

        [Tooltip("Displays the generated LUT in the top left corner of the GameView.")]
        public bool showDebug;

        public static ColorGradingSettings defaultSettings
        {
            get
            {
                return new ColorGradingSettings
                {
                    enabled = false,
                    showDebug = false,
                    precision = ColorGradingPrecision.Normal,
                    //colorWheels = ColorWheelsSettings.defaultSettings,
                    basics = BasicsSettings.defaultSettings,
                    channelMixer = ChannelMixerSettings.defaultSettings,
                    curves = CurvesSettings.defaultSettings
                };
            }
        }

        internal void Reset()
        {
            curves = CurvesSettings.defaultSettings;
        }
    }

    [SerializeField, SettingsGroup]
    private ColorGradingSettings m_ColorGrading = ColorGradingSettings.defaultSettings;
    public ColorGradingSettings colorGrading
    {
        get { return m_ColorGrading; }
        set
        {
            m_ColorGrading = value;
            SetDirty();
        }
    }

    [SerializeField, SettingsGroup]
    public LUTSettings m_Lut = LUTSettings.defaultSettings;
    public LUTSettings lut
    {
        get { return m_Lut; }
        set { m_Lut = value; }
    }

    public void SetLUTEnabel(bool _enable)
    {
        m_Lut.enabled = _enable;
        return;
    }

    public void SetLUTContribution(float _contribution)
    {
        m_Lut.contribution = _contribution;
        return;
    }

    public void SetLUTTexture(Texture _texture)
    {
        m_Lut.texture = _texture;
        return;
    }


    #endregion

    private Texture2D m_IdentityLut;
    private RenderTexture m_InternalLut;
    private Texture2D m_CurveTexture;

    private Texture2D identityLut
    {
        get
        {
            if (m_IdentityLut == null || m_IdentityLut.height != lutSize)
            {
                DestroyImmediate(m_IdentityLut);
                m_IdentityLut = GenerateIdentityLut(lutSize);
            }

            return m_IdentityLut;
        }
    }

    private RenderTexture internalLutRt
    {
        get
        {
            if (m_InternalLut == null || !m_InternalLut.IsCreated() || m_InternalLut.height != lutSize)
            {
                DestroyImmediate(m_InternalLut);
                m_InternalLut = new RenderTexture(lutSize * lutSize, lutSize, 0, RenderTextureFormat.ARGB32)
                {
                    name = "Internal LUT",
                    filterMode = FilterMode.Bilinear,
                    anisoLevel = 0,
                    hideFlags = HideFlags.DontSave
                };
            }

            return m_InternalLut;
        }
    }

    private Texture2D curveTexture
    {
        get
        {
            if (m_CurveTexture == null)
            {
                m_CurveTexture = new Texture2D(256, 1, TextureFormat.ARGB32, false, true)
                {
                    name = "Curve texture",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    anisoLevel = 0,
                    hideFlags = HideFlags.DontSave
                };
            }

            return m_CurveTexture;
        }
    }

    [SerializeField]
    private Shader m_Shader;
    public Shader shader
    {
        get
        {
            if (m_Shader == null)
                m_Shader = Shader.Find("Fairy Tails/ImageEffect_ColorGrading");

            return m_Shader;
        }
    }

    private Material m_Material;
    public Material material
    {
        get
        {
            if (m_Material == null)
                m_Material = ImageEffectHelper.CheckShaderAndCreateMaterial(shader);

            return m_Material;
        }
    }

    public bool isGammaColorSpace
    {
        get { return QualitySettings.activeColorSpace == ColorSpace.Gamma; }
    }

    public int lutSize
    {
        get { return (int)colorGrading.precision; }
    }

    private enum Pass
    {
        LutGen,
        TonemappingOff
    }

    public bool validRenderTextureFormat { get; private set; }
    public bool validUserLutSize { get; private set; }

    private bool m_Dirty = true;

    private RenderTexture m_SmallAdaptiveRt;
    private RenderTextureFormat m_AdaptiveRtFormat;

    private int m_WhiteBalance;
    private int m_Lift;
    private int m_Gamma;
    private int m_Gain;
    private int m_ContrastGainGamma;
    private int m_Vibrance;
    private int m_HSV;
    private int m_ChannelMixerRed;
    private int m_ChannelMixerGreen;
    private int m_ChannelMixerBlue;
    private int m_CurveTex;
    private int m_InternalLutTex;
    private int m_InternalLutParams;
    private int m_UserLutTex;
    private int m_UserLutParams;

    public void SetDirty()
    {
        m_Dirty = true;
    }

    private void Awake()
    {

        m_WhiteBalance = Shader.PropertyToID("_WhiteBalance");
        m_Lift = Shader.PropertyToID("_Lift");
        m_Gamma = Shader.PropertyToID("_Gamma");
        m_Gain = Shader.PropertyToID("_Gain");
        m_ContrastGainGamma = Shader.PropertyToID("_ContrastGainGamma");
        m_Vibrance = Shader.PropertyToID("_Vibrance");
        m_HSV = Shader.PropertyToID("_HSV");
        m_ChannelMixerRed = Shader.PropertyToID("_ChannelMixerRed");
        m_ChannelMixerGreen = Shader.PropertyToID("_ChannelMixerGreen");
        m_ChannelMixerBlue = Shader.PropertyToID("_ChannelMixerBlue");
        m_CurveTex = Shader.PropertyToID("_CurveTex");
        m_InternalLutTex = Shader.PropertyToID("_InternalLutTex");
        m_InternalLutParams = Shader.PropertyToID("_InternalLutParams");
        m_UserLutTex = Shader.PropertyToID("_UserLutTex");
        m_UserLutParams = Shader.PropertyToID("_UserLutParams");
    }

    private void OnEnable()
    {
        if (!ImageEffectHelper.IsSupported(shader, false, false, this))
        {
            enabled = false;
            return;
        }

        SetDirty();
    }

    //private void OnDisable()
    private void OnApplicationQuit()
    {
        if (m_Material != null)
            DestroyImmediate(m_Material);

        if (m_IdentityLut != null)
            DestroyImmediate(m_IdentityLut);

        if (m_InternalLut != null)
            DestroyImmediate(internalLutRt);

        if (m_SmallAdaptiveRt != null)
            DestroyImmediate(m_SmallAdaptiveRt);

        if (m_CurveTexture != null)
            DestroyImmediate(m_CurveTexture);

        m_Material = null;
        m_IdentityLut = null;
        m_InternalLut = null;
        m_SmallAdaptiveRt = null;
        m_CurveTexture = null;
    }

    private void OnValidate()
    {
        SetDirty();
    }

    private static Texture2D GenerateIdentityLut(int dim)
    {
        Color[] newC = new Color[dim * dim * dim];
        float oneOverDim = 1f / ((float)dim - 1f);

        for (int i = 0; i < dim; i++)
            for (int j = 0; j < dim; j++)
                for (int k = 0; k < dim; k++)
                    newC[i + (j * dim) + (k * dim * dim)] = new Color((float)i * oneOverDim, Mathf.Abs((float)k * oneOverDim), (float)j * oneOverDim, 1f);

        Texture2D tex2D = new Texture2D(dim * dim, dim, TextureFormat.RGB24, false, true)
        {
            name = "Identity LUT",
            filterMode = FilterMode.Bilinear,
            anisoLevel = 0,
            hideFlags = HideFlags.DontSave
        };
        tex2D.SetPixels(newC);
        tex2D.Apply();

        return tex2D;
    }

    // An analytical model of chromaticity of the standard illuminant, by Judd et al.
    // http://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D
    // Slightly modifed to adjust it with the D65 white point (x=0.31271, y=0.32902).
    private float StandardIlluminantY(float x)
    {
        return 2.87f * x - 3f * x * x - 0.27509507f;
    }

    // CIE xy chromaticity to CAT02 LMS.
    // http://en.wikipedia.org/wiki/LMS_color_space#CAT02
    private Vector3 CIExyToLMS(float x, float y)
    {
        float Y = 1f;
        float X = Y * x / y;
        float Z = Y * (1f - x - y) / y;

        float L = 0.7328f * X + 0.4296f * Y - 0.1624f * Z;
        float M = -0.7036f * X + 1.6975f * Y + 0.0061f * Z;
        float S = 0.0030f * X + 0.0136f * Y + 0.9834f * Z;

        return new Vector3(L, M, S);
    }

    private Vector3 GetWhiteBalance()
    {
        float t1 = colorGrading.basics.temperatureShift;
        float t2 = colorGrading.basics.tint;

        // Get the CIE xy chromaticity of the reference white point.
        // Note: 0.31271 = x value on the D65 white point
        float x = 0.31271f - t1 * (t1 < 0f ? 0.1f : 0.05f);
        float y = StandardIlluminantY(x) + t2 * 0.05f;

        // Calculate the coefficients in the LMS space.
        Vector3 w1 = new Vector3(0.949237f, 1.03542f, 1.08728f); // D65 white point
        Vector3 w2 = CIExyToLMS(x, y);
        return new Vector3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);
    }

    private static Color NormalizeColor(Color c)
    {
        float sum = (c.r + c.g + c.b) / 3f;

        if (Mathf.Approximately(sum, 0f))
            return new Color(1f, 1f, 1f, 1f);

        return new Color
        {
            r = c.r / sum,
            g = c.g / sum,
            b = c.b / sum,
            a = 1f
        };
    }

    private void GenerateLiftGammaGain(out Color lift, out Color gamma, out Color gain)
    {

        Color nLift = (colorGrading.basics.shadows * colorGrading.basics.shadowspower * 2);
        float multiplier;
        multiplier = 1f + (1f - (colorGrading.basics.midtones.r * 0.299f + colorGrading.basics.midtones.g * 0.587f + colorGrading.basics.midtones.b * 0.114f));
        Color nGamma = (colorGrading.basics.midtones * multiplier * colorGrading.basics.midtonespower * 2);
        multiplier = 1f + (1f - (colorGrading.basics.highlights.r * 0.299f + colorGrading.basics.highlights.g * 0.587f + colorGrading.basics.highlights.b * 0.114f));
        Color nGain = (colorGrading.basics.highlights * multiplier * colorGrading.basics.highlightspower * 2);

        lift = new Color(nLift.r, nLift.g, nLift.b);
        gamma = new Color(nGamma.r, nGamma.g, nGamma.b);
        gain = new Color(nGain.r, nGain.g, nGain.b);
        //
        ///

        //Color nLift = (colorGrading.colorWheels.shadows * colorGrading.basics.shadowspower);
        //Color nGamma = (colorGrading.colorWheels.midtones * colorGrading.basics.midtonespower);
        //Color nGain = (colorGrading.colorWheels.highlights * colorGrading.basics.highlightspower);
        //
        //float avgLift = (nLift.r + nLift.g + nLift.b) / 3f;
        //float avgGamma = (nGamma.r + nGamma.g + nGamma.b) / 3f;
        //float avgGain = (nGain.r + nGain.g + nGain.b) / 3f;
        //
        //// Magic numbers
        //const float liftScale = 0.1f;
        //const float gammaScale = 0.5f;
        //const float gainScale = 0.5f;
        //
        //float liftR = (nLift.r - avgLift) * liftScale;
        //float liftG = (nLift.g - avgLift) * liftScale;
        //float liftB = (nLift.b - avgLift) * liftScale;
        //
        //float gammaR = Mathf.Pow(2f, (nGamma.r - avgGamma) * gammaScale);
        //float gammaG = Mathf.Pow(2f, (nGamma.g - avgGamma) * gammaScale);
        //float gammaB = Mathf.Pow(2f, (nGamma.b - avgGamma) * gammaScale);
        //
        //float gainR = Mathf.Pow(2f, (nGain.r - avgGain) * gainScale);
        //float gainG = Mathf.Pow(2f, (nGain.g - avgGain) * gainScale);
        //float gainB = Mathf.Pow(2f, (nGain.b - avgGain) * gainScale);
        //
        //const float minGamma = 0.01f;
        //float invGammaR = 1f / Mathf.Max(minGamma, gammaR);
        //float invGammaG = 1f / Mathf.Max(minGamma, gammaG);
        //float invGammaB = 1f / Mathf.Max(minGamma, gammaB);
        //
        //lift = new Color(liftR, liftG, liftB);
        //gamma = new Color(invGammaR, invGammaG, invGammaB);
        //gain = new Color(gainR, gainG, gainB);
    }

    private void GenCurveTexture()
    {
        AnimationCurve master = colorGrading.curves.master;
        AnimationCurve red = colorGrading.curves.red;
        AnimationCurve green = colorGrading.curves.green;
        AnimationCurve blue = colorGrading.curves.blue;

        Color[] pixels = new Color[256];

        for (float i = 0f; i <= 1f; i += 1f / 255f)
        {
            float m = Mathf.Clamp(master.Evaluate(i), 0f, 1f);
            float r = Mathf.Clamp(red.Evaluate(i), 0f, 1f);
            float g = Mathf.Clamp(green.Evaluate(i), 0f, 1f);
            float b = Mathf.Clamp(blue.Evaluate(i), 0f, 1f);
            pixels[(int)Mathf.Floor(i * 255f)] = new Color(r, g, b, m);
        }

        curveTexture.SetPixels(pixels);
        curveTexture.Apply();
    }

    private bool CheckUserLut()
    {
        validUserLutSize = (lut.texture.height == (int)Mathf.Sqrt(lut.texture.width));
        return validUserLutSize;
    }

    private bool CheckSmallAdaptiveRt()
    {
        if (m_SmallAdaptiveRt != null)
            return false;

        m_AdaptiveRtFormat = RenderTextureFormat.ARGBHalf;

        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf))
            m_AdaptiveRtFormat = RenderTextureFormat.RGHalf;

        m_SmallAdaptiveRt = new RenderTexture(1, 1, 0, m_AdaptiveRtFormat);
        m_SmallAdaptiveRt.hideFlags = HideFlags.DontSave;

        return true;
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
            return;

        int yoffset = 0;

        // Color grading debug
        if (m_InternalLut != null && colorGrading.enabled && colorGrading.showDebug)
        {
            Graphics.DrawTexture(new Rect(0f, yoffset, lutSize * lutSize, lutSize), internalLutRt);
            yoffset += lutSize;
        }
    }

    //private RenderTexture[] m_AdaptRts = null;

    [ImageEffectTransformsToLDR]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
#if UNITY_EDITOR
        validRenderTextureFormat = true;

        if (source.format != RenderTextureFormat.ARGBHalf && source.format != RenderTextureFormat.ARGBFloat)
            validRenderTextureFormat = false;
#endif
        material.shaderKeywords = null;

        int renderPass = (int)Pass.TonemappingOff;

        if (colorGrading.enabled)
        {
            if (m_Dirty || !m_InternalLut.IsCreated())
            {
                Color lift, gamma, gain;
                GenerateLiftGammaGain(out lift, out gamma, out gain);
                GenCurveTexture();

                material.SetVector(m_WhiteBalance, GetWhiteBalance());
                material.SetVector(m_Lift, lift);
                material.SetVector(m_Gamma, gamma);
                material.SetVector(m_Gain, gain);
                material.SetVector(m_ContrastGainGamma, new Vector3(colorGrading.basics.contrast, colorGrading.basics.gain, 1f / colorGrading.basics.gamma));
                material.SetFloat(m_Vibrance, colorGrading.basics.vibrance);
                material.SetVector(m_HSV, new Vector4(colorGrading.basics.hue, colorGrading.basics.saturation, colorGrading.basics.value));
                material.SetVector(m_ChannelMixerRed, colorGrading.channelMixer.channels[0]);
                material.SetVector(m_ChannelMixerGreen, colorGrading.channelMixer.channels[1]);
                material.SetVector(m_ChannelMixerBlue, colorGrading.channelMixer.channels[2]);
                material.SetTexture(m_CurveTex, curveTexture);
                internalLutRt.MarkRestoreExpected();
                Graphics.Blit(identityLut, internalLutRt, material, (int)Pass.LutGen);
                m_Dirty = false;
            }

            material.EnableKeyword("ENABLE_COLOR_GRADING");

            //if (colorGrading.useDithering)
            //    material.EnableKeyword("ENABLE_DITHERING");

            material.SetTexture(m_InternalLutTex, internalLutRt);
            material.SetVector(m_InternalLutParams, new Vector3(1f / internalLutRt.width, 1f / internalLutRt.height, internalLutRt.height - 1f));
        }

        if (lut.enabled && lut.texture != null  /*&& CheckUserLut()*/)
        {
            material.SetTexture(m_UserLutTex, lut.texture);
            material.SetVector(m_UserLutParams, new Vector4(1f / lut.texture.width, 1f / lut.texture.height, lut.texture.height - 1f, lut.contribution));
            material.EnableKeyword("ENABLE_USER_LUT");
        }

        Graphics.Blit(source, destination, material, renderPass);



#if UNITY_EDITOR
        //If we have an on frame end callabck we need to pass a valid result texture
        // if destination is null we wrote to the backbuffer so we need to copy that out.
        // It's slow and not amazing, but editor only
        if (onFrameEndEditorOnly != null)
        {
            if (destination == null)
            {
                RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0);
                Graphics.Blit(source, rt, material, renderPass);
                onFrameEndEditorOnly(rt);
                RenderTexture.ReleaseTemporary(rt);
                RenderTexture.active = null;
            }
            else
            {
                onFrameEndEditorOnly(destination);
            }
        }
#endif
    }

    public Texture2D BakeLUT()
    {
        Texture2D lut = new Texture2D(internalLutRt.width, internalLutRt.height, TextureFormat.RGB24, false, true);
        RenderTexture.active = internalLutRt;
        lut.ReadPixels(new Rect(0f, 0f, lut.width, lut.height), 0, 0);
        RenderTexture.active = null;
        return lut;
    }
}
