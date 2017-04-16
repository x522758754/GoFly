using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SpriteEntry = UIAtlasMaker.SpriteEntry;
public class ToggleRowData
{
    public string content;
    public bool isCheck;

    public ToggleRowData(string content, bool isCheck)
    {
        this.content = content;
        this.isCheck = isCheck;
    }
}

public class SplitAtlas : EditorWindow
{
    UIAtlas m_atlasSource;
    UIAtlas m_atlasSourceOld;

    UIAtlas m_altasSplitNew;
    UIAtlas m_atlasTaget;

    private Vector2 scrollPos1;
    List<ToggleRowData> m_listToggleData = new List<ToggleRowData>();
    List<string> m_listAllSpriteRects = new List<string>();

    int m_titleIndex = 0;
    string[] m_titles = new string[] { "拆分图集=>碎图", "拆分图集=>小图集", "拆分图集=>其他图集" };

    private void OnGUI()
    {
        m_titleIndex = GUILayout.Toolbar(m_titleIndex, m_titles);

        CommonGui();

        switch (m_titleIndex)
        {
            case 0:
                GuiSplitTexToSamllTex();
                break;
            case 1:
                GuiSplitAtlasToSamllAtlas();
                break;
            case 2:
                GuiSplitAtlasToOtherAtlas();
                break;
        }
    }

    private void OnEnable()
    {
        GetShowData();
    }

    private void GetShowData()
    {
        if (null != m_atlasSource)
        {
            List<string> listAllSpriteNames = GetAllSprites(m_atlasSource);
            m_listToggleData = GetAllToggleRowData(listAllSpriteNames);
        }
    }

    private void CommonGui()
    {
        GUILayout.Label("Select a Source Atlas", EditorStyles.boldLabel);
        ShowGuiAtlas(ref m_atlasSourceOld);

        if (m_atlasSource != m_atlasSourceOld)
        {
            m_atlasSource = m_atlasSourceOld;

            GetShowData();
        }

        GUILayout.BeginHorizontal();
        {
            if (0 != m_listToggleData.Count)
            {
                if (GUILayout.Button("All"))
                {
                    for (int i = 0; i != m_listToggleData.Count; ++i)
                    {
                        m_listToggleData[i].isCheck = true;
                    }
                }
                if (GUILayout.Button("None"))
                {
                    for (int i = 0; i != m_listToggleData.Count; ++i)
                    {
                        m_listToggleData[i].isCheck = false;
                    }
                }
            }
        }
        GUILayout.EndHorizontal();

        scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1, GUILayout.MaxHeight(300));
        {
            for (int i = 0; i != m_listToggleData.Count; ++i)
            {
                string showString = string.Format("{0} {1}", m_listToggleData[i].content, m_listAllSpriteRects[i]);
                m_listToggleData[i].isCheck = EditorGUILayout.ToggleLeft(showString, m_listToggleData[i].isCheck, GUILayout.MinWidth(200));
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void GuiSplitTexToSamllTex()
    {
        if(GUILayout.Button("Save to Local Folder"))
        {
            string path = EditorUtility.SaveFolderPanel("Save textures to directory", "", "");
            SplitAtlasToLocalTexs(m_atlasSource, m_listToggleData, path);
        }
    }

    private void GuiSplitAtlasToSamllAtlas()
    {
        bool isShowBtn = false;
        bool isClick = false;
        for(int i=0; i != m_listToggleData.Count; ++i)
        {
            if (m_listToggleData[i].isCheck)
            {
                isShowBtn = true;
            }
        }
        if (isShowBtn)
            isClick = GUILayout.Button("Split to smaller atlas ");
        
        if (isClick)
        {
            string name = string.Format("{0}1", m_atlasSource.name);
            string savePath = AssetDatabase.GetAssetPath(m_atlasSource);
            string path = EditorUtility.SaveFilePanelInProject("Save textures to directory", name, "prefab", "Save atlas as...", savePath);

            m_altasSplitNew = CreateAtlas(path);

            SplitAtlasToSamllerAtlas(m_atlasSource, m_altasSplitNew, m_listToggleData);
        }

        if (null != m_altasSplitNew)
        {
            ShowGuiAtlas(ref m_altasSplitNew);
        }
    }

    private void GuiSplitAtlasToOtherAtlas()
    {
        ShowGuiAtlas(ref m_atlasTaget);
        if(GUILayout.Button("Split atlas to target atlas"))
        {
            if (m_atlasSource != m_atlasTaget)
                SplitAtlasToOtherAtlas(m_atlasSource, m_atlasTaget, m_listToggleData);
            else
                EditorUtility.DisplayDialog("Tip", "same Atlas", "OK");
        }
    }

    private void ShowGuiAtlas(ref UIAtlas atlas)
    {
        EditorGUILayout.BeginHorizontal();

        atlas = (UIAtlas)EditorGUILayout.ObjectField(atlas, typeof(UIAtlas), false);

        if(null != atlas && null != atlas.texture)
            GUILayout.Label(string.Format("{0}x{1}", atlas.texture.width, atlas.texture.height));
        else
            GUILayout.Label(" N/A");

        EditorGUILayout.EndHorizontal();
    }

    private List<string> GetAllSprites(UIAtlas atlas)
    {
        List<string> listAllSpriteName = null;
        if (null != atlas)
        {
            listAllSpriteName = new List<string>();
            for (int i = 0; i != atlas.spriteList.Count; ++i)
            {
                listAllSpriteName.Add(atlas.spriteList[i].name);
                m_listAllSpriteRects.Add(string.Format("({0}x{1})", atlas.spriteList[i].width, atlas.spriteList[i].height));
            }
        }

        return listAllSpriteName;
    }

    private List<ToggleRowData> GetAllToggleRowData(List<string> listAllspriteNames)
    {
        List<ToggleRowData> listRowData = new List<ToggleRowData>();
        if (null != listAllspriteNames)
        {
            
            for (int i = 0; i != listAllspriteNames.Count; ++i)
            {
                listRowData.Add(new ToggleRowData(listAllspriteNames[i], false));
            }
        }

        return listRowData;
    }


    #region 拆分图集=>碎图
    public void SplitAtlasToLocalTexs(UIAtlas atlas, List<ToggleRowData> rowData, string path)
    {
        if(null != rowData)
        {
            List<SpriteEntry> sprites = ExtractSprites(atlas);
            List<Texture2D> texs = SpritesToTexs(sprites);

            for(int i=0; i != rowData.Count; ++i)
            {
                if(rowData[i].isCheck)
                {
                    for(int j= 0; j != texs.Count; ++j)
                    {
                        Texture2D tex = texs[j];
                        if (tex.name.Equals(rowData[i].content))
                        {
                            string newPath = string.Format("{0}/{1}.png", path, tex.name);
                            byte[] bytes = tex.EncodeToPNG();
                            System.IO.File.WriteAllBytes(newPath, bytes);

                            break;
                        }
                    }

                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
    }

    private List<Texture2D> SpritesToTexs(List<SpriteEntry> sprites)
    {
        List<Texture2D> texs = null;
        if(null != sprites)
        {
            texs = new List<Texture2D>();
            for(int i=0; i != sprites.Count; ++i)
            {
                texs.Add(sprites[i].tex);
            }
        }

        return texs;
    }


    /// <summary>
    /// 分解图集，得到其中的sprites
    /// </summary>
    private List<SpriteEntry> ExtractSprites(UIAtlas atlas)
    {
        List<SpriteEntry> finalSprites = new List<UIAtlasMaker.SpriteEntry>();

        // Make the atlas texture readable
        Texture2D tex = ImportTexture(atlas.texture, true, true, false);

        if (tex != null)
        {
            Color32[] pixels = null;
            int width = tex.width;
            int height = tex.height;
            List<UISpriteData> sprites = atlas.spriteList;
            float count = sprites.Count;
            int index = 0;

            foreach (UISpriteData es in sprites)
            {
                //ShowProgress((index++) / count);

                bool found = false;

                foreach (SpriteEntry fs in finalSprites)
                {
                    if (es.name == fs.name)
                    {
                        fs.CopyBorderFrom(es);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    if (pixels == null) pixels = tex.GetPixels32();
                    SpriteEntry sprite = ExtractSprite(es, pixels, width, height);
                    if (sprite != null) finalSprites.Add(sprite);
                }
            }
        }

        // The atlas no longer needs to be readable
        ImportTexture(atlas.texture, false, false, !atlas.premultipliedAlpha);

        return finalSprites;
    }

    /// <summary>
    /// 从图集纹理中分解特定的sprite
    /// </summary>
    SpriteEntry ExtractSprite(UISpriteData es, Color32[] oldPixels, int oldWidth, int oldHeight)
    {
        int xmin = Mathf.Clamp(es.x, 0, oldWidth);
        int ymin = Mathf.Clamp(es.y, 0, oldHeight);
        int xmax = Mathf.Min(xmin + es.width, oldWidth - 1);
        int ymax = Mathf.Min(ymin + es.height, oldHeight - 1);
        int newWidth = Mathf.Clamp(es.width, 0, oldWidth);
        int newHeight = Mathf.Clamp(es.height, 0, oldHeight);

        if (newWidth == 0 || newHeight == 0) return null;

        Color32[] newPixels = new Color32[newWidth * newHeight];

        for (int y = 0; y < newHeight; ++y)
        {
            int cy = ymin + y;
            if (cy > ymax) cy = ymax;

            for (int x = 0; x < newWidth; ++x)
            {
                int cx = xmin + x;
                if (cx > xmax) cx = xmax;

                int newIndex = (newHeight - 1 - y) * newWidth + x;
                int oldIndex = (oldHeight - 1 - cy) * oldWidth + cx;

                newPixels[newIndex] = oldPixels[oldIndex];
            }
        }

        // Create a new sprite
        SpriteEntry sprite = new SpriteEntry();
        sprite.CopyFrom(es);
        sprite.SetRect(0, 0, newWidth, newHeight);
        sprite.SetTexture(newPixels, newWidth, newHeight);
        return sprite;
    }

    /// <summary>
    /// 改变某一贴图的设置，使其可读
    /// </summary>
    private bool MakeTextureReadable(string path, bool force)
    {
        if (string.IsNullOrEmpty(path)) return false;
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return false;

        TextureImporterSettings settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        if (force || !settings.readable || settings.npotScale != TextureImporterNPOTScale.None || settings.alphaIsTransparency)
        {
            settings.readable = true;
#if !UNITY_4_7 && !UNITY_5_3 && !UNITY_5_4
			if (NGUISettings.trueColorAtlas)
			{
				var platform = ti.GetDefaultPlatformTextureSettings();
				platform.format = TextureImporterFormat.RGBA32;
			}
#else
            if (NGUISettings.trueColorAtlas) settings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
#endif
            settings.npotScale = TextureImporterNPOTScale.None;
            settings.alphaIsTransparency = false;
            ti.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
        return true;
    }

    /// <summary>
    /// 改变贴图设置, 使其可以适合图集
    /// </summary>
    bool MakeTextureAnAtlas(string path, bool force, bool alphaTransparency)
    {
        if (string.IsNullOrEmpty(path)) return false;
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return false;

        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        if (force || settings.readable ||
#if UNITY_5_5_OR_NEWER
			ti.maxTextureSize < 4096 ||
			(NGUISettings.trueColorAtlas && ti.textureCompression != TextureImporterCompression.Uncompressed) ||
#else
            settings.maxTextureSize < 4096 ||
#endif
            settings.wrapMode != TextureWrapMode.Clamp ||
            settings.npotScale != TextureImporterNPOTScale.ToNearest)
        {
            settings.readable = false;
#if !UNITY_4_7 && !UNITY_5_3 && !UNITY_5_4
			ti.maxTextureSize = 4096;
#else
            settings.maxTextureSize = 4096;
#endif
            settings.wrapMode = TextureWrapMode.Clamp;
            settings.npotScale = TextureImporterNPOTScale.ToNearest;

            if (NGUISettings.trueColorAtlas)
            {
#if UNITY_5_5_OR_NEWER
				ti.textureCompression = TextureImporterCompression.Uncompressed;
#else
                settings.textureFormat = TextureImporterFormat.ARGB32;
#endif
                settings.filterMode = FilterMode.Trilinear;
            }

            settings.aniso = 4;
            settings.alphaIsTransparency = alphaTransparency;
            ti.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
        return true;
    }

    /// <summary>
    /// 修改贴图的设置，如果有必要请重新导入
    /// </summary>
    public Texture2D ImportTexture(string path, bool forInput, bool force, bool alphaTransparency)
    {
        if (!string.IsNullOrEmpty(path))
        {
            if (forInput) { if (!MakeTextureReadable(path, force)) return null; }
            else if (!MakeTextureAnAtlas(path, force, alphaTransparency)) return null;
            //return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;

            Texture2D tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return tex;
        }
        return null;
    }

    /// <summary>
    /// 修改贴图的设置，如果有必要请重新导入
    /// </summary>
    public Texture2D ImportTexture(Texture tex, bool forInput, bool force, bool alphaTransparency)
    {
        if (tex != null)
        {
            string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
            return ImportTexture(path, forInput, force, alphaTransparency);
        }
        return null;
    }

    /// <summary>
    /// 编辑器进度条
    /// </summary>
    private void ShowProgress(float val)
    {
        EditorUtility.DisplayProgressBar("Updating", "Updating the atlas, please wait...", val);
    }

    #endregion

    #region 拆分图集=>小图集

    private void SplitAtlasToSamllerAtlas(UIAtlas atlasSource, UIAtlas altasSplitNew, List<ToggleRowData> rowData)
    {
        SplitAtlasToAtlas(atlasSource, altasSplitNew, rowData, true);
    }

    private void SplitAtlasToAtlas(UIAtlas atlasSource, UIAtlas altasSplitNew, List<ToggleRowData> rowData, bool over)
    {
        List<SpriteEntry> sprites = ExtractSprites(atlasSource);
        List<SpriteEntry> selectSprites = new List<SpriteEntry>();

        for (int j = sprites.Count -1; j != -1; --j)
        {
            SpriteEntry sprite = sprites[j];
            for (int i = rowData.Count - 1; i != -1; --i)
            {
                if (rowData[i].isCheck && sprite.name.Equals(rowData[i].content))
                {
                    selectSprites.Add(sprite);
                    sprites.Remove(sprite);
                    rowData.RemoveAt(i);
                    break;
                }
            }            
        }

        //是否覆盖
        if(!over)
            selectSprites.AddRange(ExtractSprites(altasSplitNew));

        AddOrUpdate(altasSplitNew, selectSprites);
        AddOrUpdate(atlasSource, sprites);
    }

    /// <summary>
    /// 创建图集
    /// </summary>
    private UIAtlas CreateAtlas(string path)
    {
        UIAtlas atlas = null;
        if (!string.IsNullOrEmpty(path))
        {
            GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            string matPath = path.Replace(".prefab", ".mat");

            // Try to load the material
            Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;

            // If the material doesn't exist, create it
            if (mat == null)
            {
                Shader shader = Shader.Find(NGUISettings.atlasPMA ? "Unlit/Premultiplied Colored" : "Unlit/Transparent Colored");
                mat = new Material(shader);

                // Save the material
                AssetDatabase.CreateAsset(mat, matPath);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                // Load the material so it's usable
                mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
            }

            // Create a new prefab for the atlas
            Object prefab = (go != null) ? go : PrefabUtility.CreateEmptyPrefab(path);

            // Create a new game object for the atlas
            string atlasName = path.Replace(".prefab", "");
            atlasName = atlasName.Substring(path.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
            go = new GameObject(atlasName);
            go.AddComponent<UIAtlas>().spriteMaterial = mat;

            // Update the prefab
            PrefabUtility.ReplacePrefab(go, prefab);
            DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // Select the atlas
            go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            Selection.activeGameObject = go;

            atlas = go.GetComponent<UIAtlas>();

        }

        return atlas;
    }

    private void AddOrUpdate(UIAtlas atlas, List<SpriteEntry> sprites)
    {
        if (sprites.Count > 0)
        {
            // Combine all sprites into a single texture and save it
            if (UIAtlasMaker.UpdateTexture(atlas, sprites))
            {
                // Replace the sprites within the atlas
                UIAtlasMaker.ReplaceSprites(atlas, sprites);
            }

            // Release the temporary textures
            UIAtlasMaker.ReleaseSprites(sprites);
            EditorUtility.ClearProgressBar();
            return;
        }
    }

    private void Remove(UIAtlas atlas, List<SpriteEntry> sprites)
    {

    }

    #endregion

    #region 拆分图集=>其他图集

    private void SplitAtlasToOtherAtlas(UIAtlas atlasSource, UIAtlas altasSplitNew, List<ToggleRowData> rowData)
    {
        SplitAtlasToAtlas(atlasSource, altasSplitNew, rowData, false);
    }

    #endregion
}
