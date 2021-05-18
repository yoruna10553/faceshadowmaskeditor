using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Unity.Mathematics;
using System.IO;
using System;
using System.Linq;
using LitJson;

public class HairShadowEditor : EditorWindow
{
    [System.Serializable]
    public class Json
    {
        public List<HairData> HairList = new List<HairData>();

    }
    [System.Serializable]
    public class HairData
    {
        public int Hair_Count;
        public string Hair_name;
        public Vector2 Hair_offset;
        public Vector2 Hair_tiling;

    }
    [System.Serializable]
    public class Newjson
    {

        public List<NewHairData> newHairlist = new List<NewHairData>();


    }
    [System.Serializable]
    public class NewHairData
    {
        public int Hair_Count;
        public string Hair_name;
        public string HairMesh_GUID;

        public Vector2 Hair_offset;
        public Vector2 Hair_tiling;

    }
    [Serializable]
    public class MeshData
    {
        public GameObject Meshobject;
        public MeshFilter meshes;
        public MeshRenderer renderer;
        public Mesh SharedMesh { get { return meshes.sharedMesh; } }
        public Matrix4x4 LocalToWorld { get { return meshes.transform.localToWorldMatrix; } }
    }
    [Serializable]
    public class HairmeshData
    {
        public MeshFilter Hairfbx;
        public Mesh Hairmeshes;

    }
    [HeaderAttribute("Set TextureComputeShader")]
    public  ComputeShader Texturecomputeshader;
    [HeaderAttribute("Set FaceMaskTexture (Drag Textures in List)")]
    public List<Texture2D> combineAllTexturesList = new List<Texture2D>();
    [HeaderAttribute("FaceMaskOutputSetting")]
    public RawImage outputTexture;
   
    [HideInInspector] public RenderTexture combineFinishTexture;
    [HideInInspector] public Vector2 _offset;
    private Vector2 white_offset;
    private Vector2 _tiling;

    [HideInInspector] public Texture2D saveFaceMasktexture;
    [SpaceAttribute]
    [HeaderAttribute("TextureNameSetting")]
    public Texture_name texture_name;
    public enum Texture_name {BoyMaskTexture,GirlMaskTexture};

    [HeaderAttribute("TextureJsonNameSetting")]
    public Texture_json_name texture_json_name;
    public enum Texture_json_name { BoyMask, GirlMask };

    [HeaderAttribute("MeshJsonNameSetting")]
    public json_name HairMesh_json_Name;
    public enum json_name { BoyMask, GirlMask }
    private string jsonname;

    [HeaderAttribute("HairMeshSetting")]
    [HideInInspector] public List<HairmeshData> hairMesheslist = new List<HairmeshData>();
    [HideInInspector] public List<MeshData> FaceMeshslist = new List<MeshData>();

    [HeaderAttribute("Set HairMeshComputeShader")]
    public ComputeShader hairMeshcomputeShader;
    [HeaderAttribute("Set FaceMaskTexture")]
    public Texture2D maskTexture;

    [HeaderAttribute("Set FaceMaskMeshSetting")]
    public Mesh FaceMaskBasis;
    public Mesh originMesh;

    [HeaderAttribute("Choose Mask")]
    public Mesh_json_name  mesh_json_Name;
    public enum Mesh_json_name { BoyMask, GirlMask }
    private string jsonname02;


    Mesh combinedMesh;
    private Vector2 faceMaskoffset;
    private Vector2 faceMaskTiling;

    Vector2 mScroll = Vector2.zero;
    bool MaskTextureArea = true;
    bool Makejsonfile = false;
    bool MakeMeshArea = false;
    bool hairenable = true;
    bool boyable = false;
    bool girlable = false;

    bool boymask = false;
    bool girlmask = false;


    [MenuItem("FaceShadowEditor/createFaceShadow", false, 0)]
    public static void ShowHairMaskWindow()
    {
        var w = EditorWindow.GetWindow<HairShadowEditor>(false, "HairShadowEditor", true);
        w.Show();
        w.Focus();
        // w.autoRepaintOnSceneChange = true;
        w.FaceMaskBasis = new Mesh();
        string computepath = "Assets/FaceShadowMaskEditorFolder/";
        DirectoryInfo d = new DirectoryInfo(computepath);
        FileInfo[] computepathfiles = d.GetFiles();
        for (int i = 0; i < computepathfiles.Length; i++)
        {
            if (computepathfiles[i].Name.EndsWith(".compute") && computepathfiles[i].Name.StartsWith("CombineAllTexture"))
            {


                ComputeShader c = AssetDatabase.LoadAssetAtPath<ComputeShader>(computepath + computepathfiles[i].Name);
                w.Texturecomputeshader = c;


            }
            if (computepathfiles[i].Name.EndsWith(".compute") && computepathfiles[i].Name.StartsWith("ComibneFaceMesh_computeShader"))
            {


                ComputeShader q = AssetDatabase.LoadAssetAtPath<ComputeShader>(computepath + computepathfiles[i].Name);
                w.hairMeshcomputeShader = q;


            }
       

        }
        string rawImagepath = "Assets/FaceShadowMaskEditorFolder/";
        DirectoryInfo ds = new DirectoryInfo(rawImagepath);
        FileInfo[] rawImagefiles = ds.GetFiles();
        for (int i = 0; i < rawImagefiles.Length; i++)
        {
            if (rawImagefiles[i].Name.EndsWith(".prefab") && rawImagefiles[i].Name.StartsWith("RawImage"))
            {


               GameObject rawImageobject = AssetDatabase.LoadAssetAtPath<GameObject>(rawImagepath + rawImagefiles[i].Name);
                w.outputTexture = rawImageobject.GetComponent<RawImage>();


            }

        }
    }
    public void OnGUI()
    {
      
        mScroll = GUILayout.BeginScrollView(mScroll);
        GUIStyle style = new GUIStyle();
        GUILayout.BeginVertical(EditorStyles.textArea);
        MaskTextureArea = EditorGUILayout.BeginFoldoutHeaderGroup(MaskTextureArea,"CreateFaceMaskTexture");
        if (MaskTextureArea)
        {
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty a = so.FindProperty("Texturecomputeshader");
            EditorGUILayout.PropertyField(a, true);
            GUI.contentColor = Color.yellow;
            EditorGUILayout.LabelField("Put「Assets / FaceShadowMaskEditorFolder / CombineAllTexture」computeShader", GUILayout.Height(30));
            GUI.contentColor = Color.white;
            
         /*   SerializedProperty b = so.FindProperty("combineAllTexturesList");
            EditorGUILayout.PropertyField(b, true);
            GUI.contentColor = Color.yellow;
            EditorGUILayout.LabelField("put TextureName: Hair_00001 => Hair_00410=BoyMask/Hair_10001=>Hair_10405=GirlMask", GUILayout.Height(30));*/
            GUI.contentColor = Color.white;
            SerializedProperty c = so.FindProperty("outputTexture");
            EditorGUILayout.PropertyField(c, true);
            //  GUI.contentColor = Color.yellow;
            // EditorGUILayout.LabelField("Set a RawImage First", GUILayout.Height(30));
            GUILayout.Space(5f);
            GUI.contentColor = Color.white;
            EditorGUILayout.LabelField("Choose output Texture", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
           
            if (GUILayout.Button("BoyTexture",GUILayout.Width(200f)))
            {
                FindBoyHairTexture();
                boyable = true;
                girlable = false;
            }
            if (GUILayout.Button("GirlTexture",GUILayout.Width(200f)))
            {
                FindGirlHairTexture();
                girlable = true;
                boyable = false;
            }
            GUILayout.EndHorizontal();
            if (boyable==true)
            {
                GUI.contentColor = Color.red;
                EditorGUILayout.LabelField("Choose boy!!", EditorStyles.boldLabel);
            }
            if (girlable==true)
            {
                GUI.contentColor = Color.red;
                EditorGUILayout.LabelField("Choose Girl!!", EditorStyles.boldLabel);
            }
            GUI.contentColor = Color.white;
            SerializedProperty d = so.FindProperty("texture_name");
            EditorGUILayout.PropertyField(d, true);
            SerializedProperty i = so.FindProperty("texture_json_name");
            EditorGUILayout.PropertyField(i, true);
            so.ApplyModifiedProperties();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("CombineTexture"))
            {
                combinemap();
                combinemap();

            }
           
            if (GUILayout.Button("SaveTexture"))
            {
                savetexture();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginVertical(EditorStyles.textArea);
        Makejsonfile = EditorGUILayout.BeginFoldoutHeaderGroup(Makejsonfile, "CreateFaceMaskJson");
        if (Makejsonfile)
        {
            ScriptableObject targett = this;
            SerializedObject sooo = new SerializedObject(targett);
            SerializedProperty w = sooo.FindProperty("HairMesh_json_Name");
            EditorGUILayout.PropertyField(w, true);
            sooo.ApplyModifiedProperties();
            GUI.contentColor = Color.yellow;
            EditorGUILayout.LabelField("choose the same name in texture_json_name", GUILayout.Height(30));
            GUI.contentColor = Color.white;
            if (GUILayout.Button("ExportJson"))
            {
                findjson();
            }
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginVertical(EditorStyles.textArea);
        MakeMeshArea = EditorGUILayout.BeginFoldoutHeaderGroup(MakeMeshArea, "CreateFaceMaskMesh");
        if (MakeMeshArea)
        {
            ScriptableObject targets = this;
            SerializedObject soo = new SerializedObject(targets);
            SerializedProperty e = soo.FindProperty("hairMeshcomputeShader");
            EditorGUILayout.PropertyField(e, true);
            GUI.contentColor = Color.yellow;
            EditorGUILayout.LabelField("Put 「Assets / FaceShadowMaskEditorFolder /ComibneFaceMesh_computeShader」computeShader", GUILayout.Height(30));
            GUI.contentColor = Color.white;
            SerializedProperty we = soo.FindProperty("mesh_json_Name");
            EditorGUILayout.PropertyField(we, true);
            GUI.contentColor = Color.white;
            SerializedProperty f = soo.FindProperty("maskTexture");
            EditorGUILayout.PropertyField(f, true);
            GUI.contentColor = Color.white;
            GUI.contentColor = Color.yellow;
            EditorGUILayout.LabelField("Put BoyMaskTexture or GirlMaskTexture", GUILayout.Height(30));
            GUI.contentColor = Color.white;
            SerializedProperty g = soo.FindProperty("FaceMaskBasis");
            EditorGUILayout.PropertyField(g, true);
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Choose MaskMesh :boymask / girlmask", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            boymask = GUILayout.Toggle(boymask, " boymask",GUILayout.Width(80f));
            if (boymask)
            {
                fillboymesh();
                girlmask = false;
            }
            girlmask = GUILayout.Toggle(girlmask, " girlmask", GUILayout.Width(80f));
            if (girlmask)
            {
                fillgirlmesh();
                boymask = false;
            }
            GUILayout.EndHorizontal();
            SerializedProperty h = soo.FindProperty("originMesh");
            EditorGUILayout.PropertyField(h, true);
            GUI.contentColor = Color.yellow;
          
            soo.ApplyModifiedProperties();
            GUI.contentColor = Color.white;
            GUILayout.Space(10f);
            hairenable = GUILayout.Toggle(hairenable, "hairEnable");
            if (hairenable)
            {
                HairEnable();
            }
            if (!hairenable)
            {
                HairDisable();
            }
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OriginFaceMesh"))
            {
                clearList();
                SethairList();
                SetfaceList();
                returnmesh();

            }
            if (GUILayout.Button("FaceMaskMesh"))
            {
                clearList();
                SethairList();
                SetfaceList();
                combinemesh();

            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.EndScrollView();
        /*    GUILayout.BeginArea(new Rect(550, 0, 550, 550), EditorStyles.textArea);
                saveFaceMasktexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(maskTexture));
                if (maskTexture != null)
                {

                //  EditorGUILayout.ObjectField("FaceTexture:" + saveFaceMasktexture.name, saveFaceMasktexture, typeof(Texture2D), true);
                GUI.DrawTexture(new Rect(550, 0, 550, 550), saveFaceMasktexture);
                }

            GUILayout.EndArea();*/

       
    }
    string JsonPath()
    {
        jsonname = HairMesh_json_Name.ToString();

        return Application.dataPath + "/FaceShadowMaskEditorFolder/" + jsonname;
    }
    string JsonPath02()
    {
        jsonname02 = mesh_json_Name.ToString();

        return Application.dataPath + "/FaceShadowMaskEditorFolder/" + jsonname02;
    }
    
    public void FindBoyHairTexture()
    {
        combineAllTexturesList.Clear();
        string Texturepath = "Assets/02.Arts/Models/NOTFORBUILD/角色觀看區/角色檔/拆分角色/臉部投影遮罩/";
        DirectoryInfo di = new DirectoryInfo(Texturepath);
        FileInfo[] files = di.GetFiles();
        for (int a =0; a<= 410; a++)
        {
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = files[i].Name;
              
               if (a < 10 && fileName.EndsWith(".png") && fileName.StartsWith("Hair_0000" + a))
               {
                    Texture2D Texture01 = AssetDatabase.LoadAssetAtPath<Texture2D>(Texturepath+files[i].Name);
                    combineAllTexturesList.Add(Texture01);
                }

                if (a >=10 && a < 100 && fileName.EndsWith(".png") && fileName.StartsWith("Hair_000" + a))
                {
                    Texture2D Texture02 = AssetDatabase.LoadAssetAtPath<Texture2D>(Texturepath + files[i].Name);
                    combineAllTexturesList.Add(Texture02);

                }

                if (a >= 100 && fileName.EndsWith(".png") && fileName.StartsWith("Hair_00" + a))
                {
                    Texture2D Texture03 = AssetDatabase.LoadAssetAtPath<Texture2D>(Texturepath + files[i].Name);
                    combineAllTexturesList.Add(Texture03);

                }


            }
        }
    }
    public void FindGirlHairTexture()
    {
        combineAllTexturesList.Clear();
        string Texturepath02 = "Assets/02.Arts/Models/NOTFORBUILD/角色觀看區/角色檔/拆分角色/臉部投影遮罩/";
        DirectoryInfo di02 = new DirectoryInfo(Texturepath02);
        FileInfo[] files02 = di02.GetFiles();
        for (int a = 0; a <= 405; a++)
        {
            for (int i = 0; i < files02.Length; i++)
            {
                string fileName02 = files02[i].Name;

                if (a < 10 && fileName02.EndsWith(".png") && fileName02.StartsWith("Hair_1000" + a))
                {
                    Texture2D Texture001 = AssetDatabase.LoadAssetAtPath<Texture2D>(Texturepath02 + files02[i].Name);
                    combineAllTexturesList.Add(Texture001);
                }

                if (a >= 10 && a < 100 && fileName02.EndsWith(".png") && fileName02.StartsWith("Hair_100" + a))
                {
                    Texture2D Texture002 = AssetDatabase.LoadAssetAtPath<Texture2D>(Texturepath02 + files02[i].Name);
                    combineAllTexturesList.Add(Texture002);

                }

                if (a >= 100 && fileName02.EndsWith(".png") && fileName02.StartsWith("Hair_10" + a))
                {
                    Texture2D Texture003 = AssetDatabase.LoadAssetAtPath<Texture2D>(Texturepath02 + files02[i].Name);
                    combineAllTexturesList.Add(Texture003);

                }


            }
        }
    }
    public void combinemap()
    {
        Combine(combineAllTexturesList.ToArray(), ref combineFinishTexture);
        outputTexture.texture = combineFinishTexture;
       


    }
    public bool Combine(Texture2D[] localtexture, ref RenderTexture CombineTexture)
    {

        Json combinedata = new Json();
        
        CombineTexture = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGB32);
        CombineTexture.enableRandomWrite = true;
        CombineTexture.wrapMode = TextureWrapMode.Repeat;
        CombineTexture.Create();
        outputTexture.texture = CombineTexture;


        int kernel = Texturecomputeshader.FindKernel("Combine");
        uint x, y, z;

        Texturecomputeshader.SetVector("CombineTexelSize", new float4(CombineTexture.width, CombineTexture.height, 0.0f, 0.0f));
        Texturecomputeshader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
        Vector2 _tiling = new Vector2(16, 16);
        Texturecomputeshader.SetVector("_tiling", _tiling);
        Texturecomputeshader.SetFloat("mapCount", combineAllTexturesList.Count);
        Texturecomputeshader.SetTexture(kernel, "outputTexture", CombineTexture);




        for (int i = 1; i <= 16; i++)
        {
            var info = i <= localtexture.Length ? localtexture[i - 1] : null;
            var map = info != null ? info : Texture2D.whiteTexture;
            Texturecomputeshader.SetTexture(kernel, string.Format("Map_{0}", i.ToString("00")), map);
            _offset = new Vector2(i - 1, 15);
            var texelSize = new float4(map.width, map.height, _offset);
            Texturecomputeshader.SetVector(string.Format("Map_{0}_TexelSize", i.ToString("00")), texelSize);
            combinedata.HairList.Add(new HairData { Hair_Count = i - 1, Hair_name = combineAllTexturesList[i - 1].name, Hair_offset = new Vector2((_offset.x / 16), (_offset.y / 16)), Hair_tiling = new Vector2(0.0625f, 0.0625f) });
            for (int k = combinedata.HairList.Count - 1; k >= 0; k--)
            {
                if (combinedata.HairList[k].Hair_name == "UnityWhite")
                {
                    combinedata.HairList.Remove(combinedata.HairList[k]);

                }

            }
            if (combineAllTexturesList.Count < 17)
            {
                white_offset = new Vector2(i - 1, 15);
                texelSize = new float4(map.width, map.height, white_offset);
                Texturecomputeshader.SetVector(string.Format("Map_{0}_TexelSize", i.ToString("00")), texelSize);
                combineAllTexturesList.Add(Texture2D.whiteTexture);

            }
        }



        if (combineAllTexturesList.Count >= 17)
        {
            var secoundList = combineAllTexturesList.GetRange(16, combineAllTexturesList.Count - 16);

            for (int a = 1; a <= 16; a++)
            {
                var info = a <= secoundList.Count ? secoundList[a - 1] : null;
                var map = info != null ? info : Texture2D.whiteTexture;
                Texturecomputeshader.SetTexture(kernel, string.Format("Map2_{0}", a.ToString("00")), map);
                _offset = new Vector2(a - 1, 14);
                var texelSize = new float4(map.width, map.height, _offset);
                Texturecomputeshader.SetVector(string.Format("Map2_{0}_TexelSize", a.ToString("00")), texelSize);

                if (secoundList.Count < 17)
                {
                    white_offset = new Vector2(a - 1, 14);
                    texelSize = new float4(map.width, map.height, white_offset);
                    Texturecomputeshader.SetVector(string.Format("Map_{0}_TexelSize", a.ToString("00")), texelSize);
                    combineAllTexturesList.Add(Texture2D.whiteTexture);


                }
                try
                {
                    combinedata.HairList.Add(new HairData { Hair_Count = a + 15, Hair_name = combineAllTexturesList[a + 15].name, Hair_offset = new Vector2(_offset.x / 16, _offset.y / 16), Hair_tiling = new Vector2(0.0625f, 0.0625f) });
                }
                catch (ArgumentOutOfRangeException outOfRange)
                {
                    continue;
                }
                for (int k = combinedata.HairList.Count - 1; k >= 0; k--)
                {
                    if (combinedata.HairList[k].Hair_name == "UnityWhite")
                    {
                        combinedata.HairList.Remove(combinedata.HairList[k]);

                    }

                }
            }


        }


        if (combineAllTexturesList.Count >= 33)
        {
            var thirdList = combineAllTexturesList.GetRange(32, combineAllTexturesList.Count - 32);
            for (int b = 1; b <= 16; b++)
            {
                var info = b <= thirdList.Count ? thirdList[b - 1] : null;
                var map = info != null ? info : Texture2D.whiteTexture;
                Texturecomputeshader.SetTexture(kernel, string.Format("Map3_{0}", b.ToString("00")), map);
                _offset = new Vector2(b - 1, 13);
                var texelSize = new float4(map.width, map.height, _offset);
                Texturecomputeshader.SetVector(string.Format("Map3_{0}_TexelSize", b.ToString("00")), texelSize);
                if (thirdList.Count < 17)
                {
                    white_offset = new Vector2(b - 1, 13);
                    texelSize = new float4(map.width, map.height, white_offset);
                    Texturecomputeshader.SetVector(string.Format("Map_{0}_TexelSize", b.ToString("00")), texelSize);
                    combineAllTexturesList.Add(Texture2D.whiteTexture);
                }
                try
                {
                    combinedata.HairList.Add(new HairData { Hair_Count = b + 31, Hair_name = combineAllTexturesList[b + 31].name, Hair_offset = new Vector2(_offset.x / 16, _offset.y / 16), Hair_tiling = new Vector2(0.0625f, 0.0625f) });
                }
                catch (ArgumentOutOfRangeException outOfRange)
                {
                    continue;
                }
                for (int k = combinedata.HairList.Count - 1; k >= 0; k--)
                {
                    if (combinedata.HairList[k].Hair_name == "UnityWhite")
                    {
                        combinedata.HairList.Remove(combinedata.HairList[k]);

                    }

                }

            }
        }


        var whitemap = Texture2D.whiteTexture;
        var whitetexelsize = new float4(128, 128, new Vector2(16, 13));
        Texturecomputeshader.SetTexture(kernel, "whitemask", whitemap);
        Texturecomputeshader.SetVector("white_texelsize", whitetexelsize);


        Texturecomputeshader.Dispatch(kernel, Mathf.CeilToInt(CombineTexture.width / x), Mathf.CeilToInt(CombineTexture.height / y), Mathf.CeilToInt(z));
        string SaveJsonString = JsonUtility.ToJson(combinedata, true);
        StreamWriter jsonFile = new StreamWriter(System.IO.Path.Combine(Application.dataPath + "/FaceShadowMaskEditorFolder/", texture_json_name.ToString()));
        jsonFile.Write(SaveJsonString);
        jsonFile.Close();
        return CombineTexture != null;


    }
    
    public void savetexture()
    {
        combineFinishTexture = new RenderTexture((int)outputTexture.texture.width, (int)outputTexture.texture.height, 0);
        Graphics.Blit(outputTexture.texture, combineFinishTexture);

        Texture2D tex = new Texture2D(combineFinishTexture.width, combineFinishTexture.height);
        tex.ReadPixels(new Rect(0, 0, combineFinishTexture.width, combineFinishTexture.height), 0, 0);
        tex.Apply();
        //     string outPath = EditorUtility.SaveFilePanel("選擇圖片輸出路徑", Application.dataPath, "", "png");
        string outPath= EditorUtility.SaveFilePanelInProject("選擇圖片輸出路徑",texture_name.ToString(), "png",  "Enter a file name to save the Texture.");
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(outPath, bytes);
        AssetDatabase.Refresh();
        Action<TextureImporter> importAction = null;
        var importer = AssetImporter.GetAtPath(outPath) as TextureImporter;
        if (importAction != null)
            importAction(importer);
        
    }
    public void findjson()
    {
        string json = File.ReadAllText(JsonPath());
        Json jsons = new Json();
        jsons = JsonUtility.FromJson<Json>(json);

        Newjson newcombinedata = new Newjson();

        for (int q = 0; q < jsons.HairList.Count; q++)
        {

            GameObject findobject = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/02.Arts/Models/NOTFORBUILD/角色觀看區/角色檔/拆分角色/" + jsons.HairList[q].Hair_name + "_fbx.fbx");
            string MeshGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(findobject));
            newcombinedata.newHairlist.Add(new NewHairData
            {
                Hair_Count = jsons.HairList[q].Hair_Count,
                Hair_name = jsons.HairList[q].Hair_name,
                HairMesh_GUID = MeshGUID,
                Hair_offset = jsons.HairList[q].Hair_offset,
                Hair_tiling = jsons.HairList[q].Hair_tiling
            });

        }
        for (int it = 0; it < newcombinedata.newHairlist.Count; it++)
        {
            for (int ja = newcombinedata.newHairlist.Count - 1; ja > it; ja--)
            {

                if (newcombinedata.newHairlist[it].Hair_Count == newcombinedata.newHairlist[ja].Hair_Count)
                {
                    newcombinedata.newHairlist.RemoveAt(ja);
                }

            }
        }
       

        string SaveJsonString = JsonUtility.ToJson(newcombinedata, true);
        StreamWriter jsonFile = new StreamWriter(System.IO.Path.Combine(Application.dataPath + "/FaceShadowMaskEditorFolder/", HairMesh_json_Name.ToString()));
        jsonFile.Write(SaveJsonString);
        jsonFile.Close();
        /*  FileInfo fi = new FileInfo(Application.dataPath + "/FaceShadowMaskEditorFolder/");
          FileStream openfile = fi.Open(FileMode.Open);
          */

      //  float hundredPercent = 1000f;
      //  float amountDone = 0;
      //  EditorUtility.DisplayProgressBar("Savejson", "正在生成json....", amountDone ++ / hundredPercent);
        EditorUtility.DisplayDialog("SaveJsonComplete","json儲存完成! 儲存位置:"+ Application.dataPath + "/FaceShadowMaskEditorFolder/", "OK");
    }
   
    private void OnEnable()
    {
        if (combinedMesh == null)
            combinedMesh = new Mesh();

        FaceMaskBasis = combinedMesh;
        combinedMesh.Clear();


    }
    public void clearList()
    {
        if (HairMesh_json_Name == json_name.BoyMask)
        {
            if (hairMesheslist.Count > 1)
            {
                hairMesheslist.Clear();
            }
            if (FaceMeshslist.Count > 1)
            {
                FaceMeshslist.Clear();
            }
        }
        if (HairMesh_json_Name == json_name.GirlMask)
        {
            if (hairMesheslist.Count > 1)
            {
                hairMesheslist.Clear();
            }
            if (FaceMeshslist.Count > 1)
            {
                FaceMeshslist.Clear();
            }
        }
    }
    public void SethairList()
    {

        string json = File.ReadAllText(JsonPath02());
        Newjson jsonTemp = new Newjson();
        jsonTemp = JsonUtility.FromJson<Newjson>(json);

        GameObject[] findobject = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        for (int q = 0; q < jsonTemp.newHairlist.Count; q++)
        {
            foreach (var d in findobject)
            {
                if (d.name == (jsonTemp.newHairlist[q].Hair_name + "_fbx"))
                {

                    MeshFilter hf = d.GetComponentInChildren<MeshFilter>();
                    Mesh originhair = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GetAssetPath(hf.sharedMesh));
                    string a = AssetDatabase.GetAssetPath(originhair);
                    string b = AssetDatabase.AssetPathToGUID(a);
                    if (b == jsonTemp.newHairlist[q].HairMesh_GUID)
                    {
                        hairMesheslist.Add(new HairmeshData { Hairfbx = hf, Hairmeshes = originhair });
                    }
                    //  Debug.Log(a);
                }


            }
        }
        /*   for (int it = 0; it < hairMesheslist.Count; it++)
           {
               for (int ja = hairMesheslist.Count - 1; ja > it; ja--)
               {

                   if (hairMesheslist[it].Hairfbx == hairMesheslist[ja].Hairfbx || hairMesheslist[it].Hairmeshes == hairMesheslist[ja].Hairmeshes)
                   {
                       hairMesheslist.RemoveAt(ja);
                   }

               }
           }*/
    }

    public void SetfaceList()
    {
        string json = File.ReadAllText(JsonPath02());
        Newjson jsonTemp = new Newjson();
        jsonTemp = JsonUtility.FromJson<Newjson>(json);

        foreach (var s in hairMesheslist)
        {

            if (s.Hairfbx.transform.childCount == 0)
            {
                GameObject parentobject = s.Hairfbx.transform.parent.gameObject;
                MeshFilter secound = parentobject.transform.GetComponentInChildren<MeshFilter>();
                MeshRenderer secound_meshrenderer = parentobject.transform.GetComponentInChildren<MeshRenderer>();
                if (secound.name.StartsWith("Face_00001_fbx") || secound.name.StartsWith("Face_10001_fbx"))
                {
                    FaceMeshslist.Add(new MeshData { Meshobject = parentobject, meshes = secound, renderer = secound_meshrenderer });
                }

                for (int k = 0; k < parentobject.transform.childCount; k++)
                {

                    GameObject secoundobject = parentobject.transform.GetChild(k).gameObject;
                    MeshFilter secoundface = secoundobject.GetComponentInChildren<MeshFilter>();
                    MeshRenderer secoundface_meshrenderer = secoundobject.transform.GetComponentInChildren<MeshRenderer>();

                    if (secoundface.name.StartsWith("Face_00001_fbx") || secoundface.name.StartsWith("Face_10001_fbx"))
                    {
                        FaceMeshslist.Add(new MeshData { Meshobject = secoundobject, meshes = secoundface, renderer = secoundface_meshrenderer });


                    }
                }
            }
            if (s.Hairfbx.transform.childCount > 0)
            {


                for (int d = 0; d < s.Hairfbx.transform.childCount; d++)
                {

                    GameObject faceobject = s.Hairfbx.transform.GetChild(d).gameObject;
                    MeshFilter face = faceobject.GetComponentInChildren<MeshFilter>();
                    MeshRenderer face_meshrenderer = faceobject.transform.GetComponentInChildren<MeshRenderer>();
                    if (face.name.StartsWith("Face_00001_fbx") || face.name.StartsWith("Face_10001_fbx"))
                    {

                        FaceMeshslist.Add(new MeshData { Meshobject = faceobject, meshes = face, renderer = face_meshrenderer });

                    }

                }
            }

        }


        /* for (int it = 0; it < FaceMeshslist.Count; it++)
         {
             for (int ja = FaceMeshslist.Count - 1; ja > it; ja--)
             {

                 if (FaceMeshslist[it].Meshobject == FaceMeshslist[ja].Meshobject/*|| FaceMeshslist[it].renderer == FaceMeshslist[ja].renderer)
                 {
                     FaceMeshslist.RemoveAt(ja);
                 }

             }
         }*/
    }
    public void fillboymesh()
    {
        GameObject findboymesh = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/02.Arts/Models/NOTFORBUILD/角色觀看區/角色檔/拆分角色/Face_00001_fbx.fbx");
        originMesh = findboymesh.GetComponent<MeshFilter>().sharedMesh;

    }
    public void fillgirlmesh()
    {
        GameObject findgirlmesh = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/02.Arts/Models/NOTFORBUILD/角色觀看區/角色檔/拆分角色/Face_10001_fbx.fbx");
        originMesh = findgirlmesh.GetComponent<MeshFilter>().sharedMesh;

    }
    public void combinemesh()
    {

        combinedMesh.Clear();

        if (!File.Exists(JsonPath()))
        {
            Debug.LogError("文件不存在！");
            return;
        }
        string json = File.ReadAllText(JsonPath02());
        Newjson jsonTemp = new Newjson();
        jsonTemp = JsonUtility.FromJson<Newjson>(json);

        for (int i = 0; i < jsonTemp.newHairlist.Count; i++)
        {
            for (int a = 0; a < FaceMeshslist.Count; a++)
            {
                for (int h = 0; h < hairMesheslist.Count; h++)
                {


                    if (hairMesheslist[h].Hairfbx.transform.gameObject.name == FaceMeshslist[a].Meshobject.transform.parent.name
                       || hairMesheslist[h].Hairfbx.transform.parent.name == FaceMeshslist[a].Meshobject.transform.parent.name
                       || hairMesheslist[h].Hairfbx.transform.parent.name == FaceMeshslist[a].Meshobject.transform.name)
                    {
                        if (hairMesheslist[h].Hairmeshes.name == jsonTemp.newHairlist[i].Hair_name
                             || jsonTemp.newHairlist[i].Hair_name + "_fbx" == hairMesheslist[h].Hairmeshes.name)
                        {
                            FaceMeshslist[a].meshes.sharedMesh = Instantiate(FaceMeshslist[a].meshes.sharedMesh);
                            FaceMaskBasis = FaceMeshslist[a].meshes.sharedMesh;
                            combinedMesh = FaceMaskBasis;

                            faceMaskoffset = jsonTemp.newHairlist[i].Hair_offset;
                            faceMaskTiling = jsonTemp.newHairlist[i].Hair_tiling;

                        //    combineFaceResult.name = jsonTemp.newHairlist[i].Hair_name + "_Facemask";
                            combinedMesh.name = jsonTemp.newHairlist[i].Hair_name + "_Facemask";

                            string materialPath = "Assets/02.Arts/Models/Hairs/" + jsonTemp.newHairlist[i].Hair_name + "/Materials/" + jsonTemp.newHairlist[i].Hair_name + "_001_mat.mat";
                            //     combineHairMaterial.material = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
                            FaceMeshslist[a].renderer.sharedMaterial.SetTexture("_EffectiveMap", maskTexture);
                            FaceMeshslist[a].renderer.sharedMaterial.SetFloat("_FaceLightMapCombineMode", 1.0f);
                            //    combineHairResult.mesh = hairMesheslist[h].Hairmeshes;

                            Mesh faceMaskCloneMesh = Instantiate(FaceMeshslist[a].meshes.sharedMesh);
                            int faceMaskCount = faceMaskCloneMesh.uv.Length;

                            ComputeBuffer faceMaskUVBuffer = new ComputeBuffer(faceMaskCount, sizeof(float) * 2);
                            faceMaskUVBuffer.SetData(faceMaskCloneMesh.uv);

                            int kernel = hairMeshcomputeShader.FindKernel("CombineMesh");
                            uint x, y, z;
                            hairMeshcomputeShader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
                            hairMeshcomputeShader.SetInt("faceMaskUVLength", faceMaskCloneMesh.uv.Length);
                            hairMeshcomputeShader.SetBuffer(kernel, "faceMaskUV", faceMaskUVBuffer);

                            hairMeshcomputeShader.SetVector("faceMask_Offset", faceMaskoffset);
                            hairMeshcomputeShader.SetVector("faceMask_Tiling", faceMaskTiling);


                            hairMeshcomputeShader.Dispatch(kernel, Mathf.CeilToInt(faceMaskCount / (float)x), Mathf.CeilToInt(y), Mathf.CeilToInt(z));


                            Vector2[] faceMaskUV = new Vector2[faceMaskCount];
                            faceMaskCloneMesh.uv = faceMaskCloneMesh.uv;
                            faceMaskUVBuffer.GetData(faceMaskUV);
                            faceMaskCloneMesh.uv2 = faceMaskCloneMesh.uv;
                            faceMaskCloneMesh.uv3 = faceMaskUV.ToList().GetRange(0, faceMaskCloneMesh.uv.Length).ToArray();

                            Matrix4x4 identity = Matrix4x4.identity;
                            CombineInstance faceMaskMesh = new CombineInstance();

                            faceMaskMesh.mesh = faceMaskCloneMesh;
                            faceMaskMesh.transform = identity;

                            CombineInstance[] combines = new CombineInstance[] { faceMaskMesh };


                            combinedMesh.CombineMeshes(combines, true);


                            DestroyImmediate(faceMaskCloneMesh);
                            FaceMeshslist[a].meshes.sharedMesh = FaceMaskBasis;


                            faceMaskUVBuffer.Dispose();

                            //      Debug.Log(faceMaskoffset);

                            var s = FaceMeshslist.Select(f=>f.meshes.gameObject);
                                Selection.objects =s.ToArray();
                            


                        }

                    }
                }

            }

        }
    }
    public void changeMesh()
    {
        foreach (var t in FaceMeshslist)
        {
            t.meshes.sharedMesh = t.meshes.sharedMesh;

        }

    }
    public void returnmesh()
    {

        foreach (var t in FaceMeshslist)
        {
            t.meshes.sharedMesh = originMesh;

        }
    }
    public void HairEnable()
    {

        foreach (var y in hairMesheslist)
        {
            y.Hairfbx.GetComponent<MeshRenderer>().enabled = true;

        }
    }
    public void HairDisable()
    {

        foreach (var y in hairMesheslist)
        {
            y.Hairfbx.GetComponent<MeshRenderer>().enabled = false;

        }
    }
}
