using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;




[CustomEditor(typeof(GPUSkinningSampler))]
public class GPUSkinningSamplerEditor : Editor
{
    private GPUSkinningAnimation        anim = null;
    private Mesh                        mesh = null;
    private Material                    mtrl = null;
    private TextAsset                   texture = null;
    private RenderTexture               rt = null;
    private RenderTexture               rtGamma = null;
    private Material                    linearToGammeMtrl = null;
    private Camera                      cam = null;
    private int                         previewClipIndex = 0;
    private float                       time = 0;
    private Vector3                     camLookAtOffset = Vector3.zero;
    private Rect                        previewEditBtnRect;
    private Rect                        interactionRect;
    private GameObject[]                boundsGos = null;
    private Bounds                      bounds;
    private Material                    boundsMtrl = null;
    private bool                        isBoundsVisible = true;
    private GameObject[]                arrowGos = null;
    private Material[]                  arrowMtrls = null;
    private float                       boundsAutoExt = 0.1f;
    private bool                        isJointsFoldout = true;
    private bool                        isBoundsFoldout = true;
    private bool                        isLODFoldout = true;
    private bool                        isRootMotionFoldout = true;
    private bool                        isAnimEventsFoldout = true;
    private bool                        rootMotionEnabled = false;
    private GameObject[]                gridGos = null;
    private Material                    gridMtrl = null;
    private bool                        guiEnabled = false;
    private GPUSkinningPlayerMono       preview = null;
    public override void OnInspectorGUI()
    {
        GPUSkinningSampler sampler = target as GPUSkinningSampler;
        if (sampler == null)
        {
            return;
        }

        sampler.MappingAnimationClips();
        OnGUISampler(sampler);
        OnGUIPreview(sampler);
        if (preview != null)
        {
            Repaint();
        }
    }

    private void Awake()
    {
        EditorApplication.update += UpdateHandler;
        time = Time.realtimeSinceStartup;

        if (!Application.isPlaying)
        {
            Object obj = AssetDatabase.LoadMainAssetAtPath(GPUSkinningSampler.ReadTempData(GPUSkinningSampler.TEMP_SAVED_ANIM_PATH));
            if (obj != null && obj is GPUSkinningAnimation)
            {
                serializedObject.FindProperty("anim").objectReferenceValue = obj;
            }

            obj = AssetDatabase.LoadMainAssetAtPath(GPUSkinningSampler.ReadTempData(GPUSkinningSampler.TEMP_SAVED_MESH_PATH));
            if (obj != null && obj is Mesh)
            {
                serializedObject.FindProperty("savedMesh").objectReferenceValue = obj;
            }

            obj = AssetDatabase.LoadMainAssetAtPath(GPUSkinningSampler.ReadTempData(GPUSkinningSampler.TEMP_SAVED_MTRL_PATH));
            if (obj != null && obj is Material)
            {
                serializedObject.FindProperty("savedMtrl").objectReferenceValue = obj;
            }

            obj = AssetDatabase.LoadMainAssetAtPath(GPUSkinningSampler.ReadTempData(GPUSkinningSampler.TEMP_SAVED_SHADER_PATH));
            if (obj != null && obj is Shader)
            {
                serializedObject.FindProperty("savedShader").objectReferenceValue = obj;
            }
            obj = AssetDatabase.LoadMainAssetAtPath(GPUSkinningSampler.ReadTempData(GPUSkinningSampler.TEMP_SAVED_TEXTURE_PATH));
            if (obj != null && obj is TextAsset)
            {
                serializedObject.FindProperty("texture").objectReferenceValue = obj;
            }

            serializedObject.ApplyModifiedProperties();

            GPUSkinningSampler.DeleteTempData(GPUSkinningSampler.TEMP_SAVED_ANIM_PATH);
            GPUSkinningSampler.DeleteTempData(GPUSkinningSampler.TEMP_SAVED_MESH_PATH);
            GPUSkinningSampler.DeleteTempData(GPUSkinningSampler.TEMP_SAVED_MTRL_PATH);
            GPUSkinningSampler.DeleteTempData(GPUSkinningSampler.TEMP_SAVED_SHADER_PATH);
            GPUSkinningSampler.DeleteTempData(GPUSkinningSampler.TEMP_SAVED_TEXTURE_PATH);
        }

        isBoundsFoldout         = GetEditorPrefsBool("isBoundsFoldout", true);
        isJointsFoldout         = GetEditorPrefsBool("isJointsFoldout", true);
        isRootMotionFoldout     = GetEditorPrefsBool("isRootMotionFoldout", true);
        isLODFoldout            = GetEditorPrefsBool("isLODFoldout", true);
        isAnimEventsFoldout     = GetEditorPrefsBool("isAnimEventsFoldout", true);
        rootMotionEnabled       = true;
    }

    private void OnDestroy()
    {
        EditorApplication.update -= UpdateHandler;
        EditorUtility.ClearProgressBar();
        DestroyPreview();
    }


    private void UpdateHandler()
    {
        if (preview != null && EditorApplication.isPlaying)
        {
            DestroyPreview();
            return;
        }

        GPUSkinningSampler sampler = target as GPUSkinningSampler;

        if (EditorApplication.isCompiling)
        {
            if (Selection.activeGameObject == sampler.gameObject)
            {
                Selection.activeGameObject = null;
                return;
            }
        }

        float deltaTime = Time.realtimeSinceStartup - time;

        if (preview != null)
        {
            PreviewDrawBounds();
            preview.UpdateAnimr(deltaTime);
            cam.Render();
        }

        time = Time.realtimeSinceStartup;

        if (!sampler.isSampling && sampler.IsSamplingProgress())
        {
            if (++sampler.samplingClipIndex < sampler.animClips.Length)
            {
                sampler.StartSample();
            }
            else
            {
                sampler.EndSample();
                EditorApplication.isPlaying = false;
                EditorUtility.ClearProgressBar();
                LockInspector(false);
            }
        }

        if (sampler.isSampling)
        {
            string msg = sampler.animClip.name + "(" + (sampler.samplingClipIndex + 1) + "/" + sampler.animClips.Length + ")";
            EditorUtility.DisplayProgressBar("Sampling, DONOT stop playing", msg, (float)(sampler.samplingFrameIndex + 1) / sampler.samplingTotalFrams);
        }
    }


    private bool GetEditorPrefsBool(string key, bool defaultValue)
    {
        return EditorPrefs.GetBool("GPUSkinningSamplerEditorPrefs_" + key, defaultValue);
    }

    private void SetEditorPrefsBool(string key, bool value)
    {
        EditorPrefs.SetBool("GPUSkinningSamplerEditorPrefs_" + key, value);
    }

    private void BeginBox()
    {
        EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
        EditorGUILayout.Space();
    }

    private void EndBox()
    {
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }

    private void OnGUISampler( GPUSkinningSampler sampler )
    {
        guiEnabled = !Application.isPlaying;
        BeginBox();
        {
            GUI.enabled = guiEnabled;
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animName"), new GUIContent("Animation Name"));

                GUI.enabled = false;
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("anim"), new GUIContent());
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("savedMesh"), new GUIContent());
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("savedMtrl"), new GUIContent());
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("savedShader"), new GUIContent());
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("texture"), new GUIContent());
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                GUI.enabled = true && guiEnabled;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("skinQuality"), new GUIContent("Quality"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("shaderType"), new GUIContent("Shader Type"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("rootBoneTransform"), new GUIContent("Root Bone"));
                OnGUIAnimationClips(sampler);
            }
            GUI.enabled = true;

            if (GUILayout.Button("Step1: Play Scene"))
            {
                DestroyPreview();
                EditorApplication.isPlaying = true;
            }

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Step2: Start Sample"))
                {
                    DestroyPreview();
                    LockInspector(true);
                    sampler.BeginSample();
                    sampler.StartSample();
                }
            }
        }
        EndBox();
    }


    private SerializedProperty          animClips_array_size_sp = null;
    private SerializedProperty          wrapModes_array_size_sp = null;
    private SerializedProperty          fpsList_array_size_sp = null;
    private SerializedProperty          rootMotionEnabled_array_size_sp = null;
    private SerializedProperty          individualDifferenceEnabled_array_size_sp = null;
    private List<SerializedProperty>    animClips_item_sp = null;
    private List<SerializedProperty>    wrapModes_item_sp = null;
    private List<SerializedProperty>    fpsList_item_sp = null;
    private List<SerializedProperty>    rootMotionEnabled_item_sp = null;
    private List<SerializedProperty>    individualDifferenceEnabled_item_sp = null;
    private int animClips_count = 0;
    private void OnGUIAnimationClips(GPUSkinningSampler sampler)
    {
        System.Action ResetItemSp = () =>
        {
            animClips_item_sp.Clear();
            wrapModes_item_sp.Clear();
            fpsList_item_sp.Clear();
            rootMotionEnabled_item_sp.Clear();
            individualDifferenceEnabled_item_sp.Clear();

            wrapModes_array_size_sp.intValue = animClips_array_size_sp.intValue;
            fpsList_array_size_sp.intValue = animClips_array_size_sp.intValue;
            rootMotionEnabled_array_size_sp.intValue = animClips_array_size_sp.intValue;
            individualDifferenceEnabled_array_size_sp.intValue = animClips_array_size_sp.intValue;

            for (int i = 0; i < animClips_array_size_sp.intValue; i++)
            {
                animClips_item_sp.Add(serializedObject.FindProperty(string.Format("animClips.Array.data[{0}]", i)));
                wrapModes_item_sp.Add(serializedObject.FindProperty(string.Format("wrapModes.Array.data[{0}]", i)));
                fpsList_item_sp.Add(serializedObject.FindProperty(string.Format("fpsList.Array.data[{0}]", i)));
                rootMotionEnabled_item_sp.Add(serializedObject.FindProperty(string.Format("rootMotionEnabled.Array.data[{0}]", i)));
                individualDifferenceEnabled_item_sp.Add(serializedObject.FindProperty(string.Format("individualDifferenceEnabled.Array.data[{0}]", i)));
            }

            animClips_count = animClips_item_sp.Count;
        };

        if (animClips_array_size_sp == null) animClips_array_size_sp = serializedObject.FindProperty("animClips.Array.size");
        if (wrapModes_array_size_sp == null) wrapModes_array_size_sp = serializedObject.FindProperty("wrapModes.Array.size");
        if (fpsList_array_size_sp == null) fpsList_array_size_sp = serializedObject.FindProperty("fpsList.Array.size");
        if (rootMotionEnabled_array_size_sp == null) rootMotionEnabled_array_size_sp = serializedObject.FindProperty("rootMotionEnabled.Array.size");
        if (individualDifferenceEnabled_array_size_sp == null) individualDifferenceEnabled_array_size_sp = serializedObject.FindProperty("individualDifferenceEnabled.Array.size");
        if (animClips_item_sp == null)
        {
            animClips_item_sp = new List<SerializedProperty>();
            wrapModes_item_sp = new List<SerializedProperty>();
            fpsList_item_sp = new List<SerializedProperty>();
            rootMotionEnabled_item_sp = new List<SerializedProperty>();
            individualDifferenceEnabled_item_sp = new List<SerializedProperty>();
            ResetItemSp();
        }

        BeginBox();
        {
            if (!sampler.IsAnimatorOrAnimation())
            {
                EditorGUILayout.HelpBox("Set AnimClips with Animation Component", MessageType.Info);
            }

            EditorGUILayout.PrefixLabel("Sample Clips");

            GUI.enabled = sampler.IsAnimatorOrAnimation() && guiEnabled;
            int no = animClips_array_size_sp.intValue;
            int no2 = wrapModes_array_size_sp.intValue;
            int no3 = fpsList_array_size_sp.intValue;
            int no4 = rootMotionEnabled_array_size_sp.intValue;
            int no5 = individualDifferenceEnabled_array_size_sp.intValue;

            EditorGUILayout.BeginHorizontal();
            {
                animClips_count = EditorGUILayout.IntField("Size", animClips_count);
                if (GUILayout.Button("Apply", GUILayout.Width(60)))
                {
                    if (animClips_count != no)
                    {
                        animClips_array_size_sp.intValue = animClips_count;
                    }
                    if (animClips_count != no2)
                    {
                        wrapModes_array_size_sp.intValue = animClips_count;
                    }
                    if (animClips_count != no3)
                    {
                        fpsList_array_size_sp.intValue = animClips_count;
                    }
                    if (animClips_count != no4)
                    {
                        rootMotionEnabled_array_size_sp.intValue = animClips_count;
                    }
                    if (animClips_count != no5)
                    {
                        individualDifferenceEnabled_array_size_sp.intValue = animClips_count;
                        ResetItemSp();
                    }
                    return;
                }
                if (GUILayout.Button("Reset", GUILayout.Width(60)))
                {
                    ResetItemSp();
                    GUI.FocusControl(string.Empty);
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true && guiEnabled;

            EditorGUILayout.BeginHorizontal();
            {
                for (int j = -1; j < 5; ++j)
                {
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (j == -1)
                            {
                                GUILayout.Label("   ");
                            }
                            if (j == 0)
                            {
                                GUILayout.Label("FPS");
                            }
                            if (j == 1)
                            {
                                GUILayout.Label("Wrap Mode");
                            }
                            if (j == 2)
                            {
                                GUILayout.Label("Anim Clip");
                            }
                            if (j == 3)
                            {
                                GUILayout.Label("Root Motion");
                            }
                            if (j == 4)
                            {
                                GUILayout.Label("Individual Difference");
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        for (int i = 0; i < no; i++)
                        {
                            var prop = animClips_item_sp[i];
                            var prop2 = wrapModes_item_sp[i];
                            var prop3 = fpsList_item_sp[i];
                            var prop4 = rootMotionEnabled_item_sp[i];
                            var prop5 = individualDifferenceEnabled_item_sp[i];
                            if (prop != null)
                            {
                                if (j == -1)
                                {
                                    GUILayout.Label((i + 1) + ":    ");
                                }
                                if (j == 0)
                                {
                                    EditorGUILayout.PropertyField(prop3, new GUIContent());
                                    prop3.intValue = Mathf.Clamp(prop3.intValue, 0, 60);
                                }
                                if (j == 1)
                                {
                                    EditorGUILayout.PropertyField(prop2, new GUIContent());
                                }
                                if (j == 2)
                                {
                                    GUI.enabled = sampler.IsAnimatorOrAnimation() && guiEnabled;
                                    EditorGUILayout.PropertyField(prop, new GUIContent());
                                    GUI.enabled = true && guiEnabled;
                                }
                                if (j == 3)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    prop4.boolValue = GUILayout.Toggle(prop4.boolValue, string.Empty);
                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.EndHorizontal();
                                }
                                if (j == 4)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    GUI.enabled = prop2.enumValueIndex == 1 && guiEnabled;
                                    prop5.boolValue = GUILayout.Toggle(prop5.boolValue, string.Empty);
                                    if (!GUI.enabled)
                                    {
                                        prop5.boolValue = false;
                                    }
                                    GUI.enabled = true && guiEnabled;
                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EndBox();
    }


    private void OnGUIPreview( GPUSkinningSampler sampler )
    {
        BeginBox();
        {
            if (GUILayout.Button("Preview/Edit"))
            {
                anim = sampler.anim;
                mesh = sampler.savedMesh;
                mtrl = sampler.savedMtrl;
                texture = sampler.texture;
                if (mesh != null)
                {
                    bounds = mesh.bounds;
                }
                if (anim == null || mesh == null || mtrl == null || texture == null)
                {
                    EditorUtility.DisplayDialog("GPUSkinning", "Missing Sampling Resources", "OK");
                }
                else
                {
                    if (rt == null && !EditorApplication.isPlaying)
                    {
                        linearToGammeMtrl = new Material(Shader.Find("GPUSkinning/GPUSkinningSamplerEditor_LinearToGamma"));
                        linearToGammeMtrl.hideFlags = HideFlags.HideAndDontSave;

                        rt = new RenderTexture(1024, 1024, 32, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
                        rt.hideFlags = HideFlags.HideAndDontSave;

                        if (PlayerSettings.colorSpace == ColorSpace.Linear)
                        {
                            rtGamma = new RenderTexture(512, 512, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                            rtGamma.hideFlags = HideFlags.HideAndDontSave;
                        }

                        GameObject camGo = new GameObject("GPUSkinningSamplerEditor");
                        camGo.hideFlags = HideFlags.HideAndDontSave;
                        cam = camGo.AddComponent<Camera>();
                        cam.hideFlags = HideFlags.HideAndDontSave;
                        cam.farClipPlane = 100;
                        cam.targetTexture = rt;
                        cam.enabled = false;
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
                        camGo.transform.position = new Vector3(999, 1002, 1010);
                        camGo.transform.localEulerAngles = new Vector3(0, -180f, 0);

                        previewClipIndex = 0;

                        GameObject previewGo = new GameObject("GPUSkinningPreview");
                        previewGo.hideFlags = HideFlags.HideAndDontSave;
                        previewGo.transform.position = new Vector3(999, 999, 1002);
                        preview = previewGo.AddComponent<GPUSkinningPlayerMono>();
                        preview.hideFlags = HideFlags.HideAndDontSave;
                        preview.Init(anim, mesh, mtrl, texture);
                        preview.Player.RootMotionEnabled = rootMotionEnabled;
                        preview.Player.CullingMode = GPUSKinningCullingMode.AlwaysAnimate;
                    }
                }
            }
            GetLastGUIRect(ref previewEditBtnRect);

            if (rt != null)
            {
                int previewRectSize = Mathf.Min((int)(previewEditBtnRect.width * 0.9f), 512);
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginVertical();
                    {
                        if (PlayerSettings.colorSpace == ColorSpace.Linear)
                        {
                            RenderTexture tempRT = RenderTexture.active;
                            Graphics.Blit(rt, rtGamma, linearToGammeMtrl);
                            RenderTexture.active = tempRT;
                            GUILayout.Box(rtGamma, GUILayout.Width(previewRectSize), GUILayout.Height(previewRectSize));
                        }
                        else
                        {
                            GUILayout.Box(rt, GUILayout.Width(previewRectSize), GUILayout.Height(previewRectSize));
                        }
                        
                        EditorGUILayout.HelpBox("Drag to Orbit\nCtrl + Drag to Pitch\nAlt+ Drag to Zoom\nPress P Key to Pause", MessageType.None);
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUI.ProgressBar(new Rect(interactionRect.x, interactionRect.y + interactionRect.height, interactionRect.width, 5), preview.Player.NormalizedTime, string.Empty);

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                OnGUIPreviewClipsOptions();

                OnGUIAnimTimeline();

                EditorGUILayout.Space();

                OnGUIRootMotion();

                EditorGUILayout.Space();

                OnGUIEditBounds();

                EditorGUILayout.Space();
            }
        }
        EndBox();

        serializedObject.ApplyModifiedProperties();
    }

    ///--------------------------------------------------------------------------------------------------------
    private void DestroyPreview()
    {
        if (rt != null)
        {
            cam.targetTexture = null;
            DestroyImmediate(linearToGammeMtrl);
            linearToGammeMtrl = null;

            DestroyImmediate(rt);
            rt = null;
            if (rtGamma != null)
            {
                DestroyImmediate(rtGamma);
                rtGamma = null;
            }

            DestroyImmediate(cam.gameObject);
            cam = null;

            DestroyImmediate(preview.gameObject);
            preview = null;

            if (boundsGos != null)
            {
                foreach (GameObject boundsGo in boundsGos)
                {
                    DestroyImmediate(boundsGo);
                }
                boundsGos = null;
            }

            if (arrowGos != null)
            {
                foreach (GameObject arrowGo in arrowGos)
                {
                    DestroyImmediate(arrowGo);
                }
                arrowGos = null;

                foreach (Material mtrl in arrowMtrls)
                {
                    DestroyImmediate(mtrl);
                }
                arrowMtrls = null;
            }

            if (gridGos != null)
            {
                foreach (GameObject gridGo in gridGos)
                {
                    DestroyImmediate(gridGo);
                }
                gridGos = null;

                DestroyImmediate(gridMtrl);
                gridMtrl = null;
            }

            DestroyImmediate(boundsMtrl);
            boundsMtrl = null;
        }
    }

    private void PreviewDrawBounds()
    {
        if (boundsGos == null)
        {
            boundsMtrl = new Material(Shader.Find("GPUSkinning/GPUSkinningSamplerEditor_UnlitColor"));
            boundsMtrl.color = Color.white;

            boundsGos = new GameObject[12];
            for (int i = 0; i < boundsGos.Length; ++i)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.hideFlags = HideFlags.HideAndDontSave;
                go.GetComponent<MeshRenderer>().sharedMaterial = boundsMtrl;
                boundsGos[i] = go;
            }
        }

        if (boundsGos != null && preview != null)
        {
            float thinness = 0.01f;
            boundsGos[0].transform.parent = preview.transform;
            boundsGos[0].transform.localScale = new Vector3(thinness, thinness, bounds.extents.z * 2);
            boundsGos[0].transform.localPosition = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, 0);
            boundsGos[1].transform.parent = preview.transform;
            boundsGos[1].transform.localScale = new Vector3(thinness, thinness, bounds.extents.z * 2);
            boundsGos[1].transform.localPosition = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, 0);
            boundsGos[2].transform.parent = preview.transform;
            boundsGos[2].transform.localScale = new Vector3(bounds.extents.x * 2, thinness, thinness);
            boundsGos[2].transform.localPosition = bounds.center + new Vector3(0, -bounds.extents.y, bounds.extents.z);
            boundsGos[3].transform.parent = preview.transform;
            boundsGos[3].transform.localScale = new Vector3(bounds.extents.x * 2, thinness, thinness);
            boundsGos[3].transform.localPosition = bounds.center + new Vector3(0, -bounds.extents.y, -bounds.extents.z);

            boundsGos[4].transform.parent = preview.transform;
            boundsGos[4].transform.localScale = new Vector3(thinness, thinness, bounds.extents.z * 2);
            boundsGos[4].transform.localPosition = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, 0);
            boundsGos[5].transform.parent = preview.transform;
            boundsGos[5].transform.localScale = new Vector3(thinness, thinness, bounds.extents.z * 2);
            boundsGos[5].transform.localPosition = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, 0);
            boundsGos[6].transform.parent = preview.transform;
            boundsGos[6].transform.localScale = new Vector3(bounds.extents.x * 2, thinness, thinness);
            boundsGos[6].transform.localPosition = bounds.center + new Vector3(0, bounds.extents.y, bounds.extents.z);
            boundsGos[7].transform.parent = preview.transform;
            boundsGos[7].transform.localScale = new Vector3(bounds.extents.x * 2, thinness, thinness);
            boundsGos[7].transform.localPosition = bounds.center + new Vector3(0, bounds.extents.y, -bounds.extents.z);

            boundsGos[8].transform.parent = preview.transform;
            boundsGos[8].transform.localScale = new Vector3(thinness, bounds.extents.y * 2, thinness);
            boundsGos[8].transform.localPosition = bounds.center + new Vector3(bounds.extents.x, 0, bounds.extents.z);
            boundsGos[9].transform.parent = preview.transform;
            boundsGos[9].transform.localScale = new Vector3(thinness, bounds.extents.y * 2, thinness);
            boundsGos[9].transform.localPosition = bounds.center + new Vector3(-bounds.extents.x, 0, bounds.extents.z);
            boundsGos[10].transform.parent = preview.transform;
            boundsGos[10].transform.localScale = new Vector3(thinness, bounds.extents.y * 2, thinness);
            boundsGos[10].transform.localPosition = bounds.center + new Vector3(bounds.extents.x, 0, -bounds.extents.z);
            boundsGos[11].transform.parent = preview.transform;
            boundsGos[11].transform.localScale = new Vector3(thinness, bounds.extents.y * 2, thinness);
            boundsGos[11].transform.localPosition = bounds.center + new Vector3(-bounds.extents.x, 0, -bounds.extents.z);

            for (int i = 0; i < boundsGos.Length; ++i)
            {
                GameObject boundsGo = boundsGos[i];
                boundsGo.SetActive(isBoundsVisible);
            }
        }
    }

    private void LockInspector(bool isLocked)
    {
        System.Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
        FieldInfo field = type.GetField("m_AllInspectors", BindingFlags.Static | BindingFlags.NonPublic);
        System.Collections.ArrayList windows = new System.Collections.ArrayList(field.GetValue(null) as System.Collections.ICollection);
        foreach (var window in windows)
        {
            PropertyInfo property = type.GetProperty("isLocked");
            property.SetValue(window, isLocked, null);
        }
    }

    private void GetLastGUIRect(ref Rect rect)
    {
        Rect guiRect = GUILayoutUtility.GetLastRect();
        if (guiRect.x != 0)
        {
            rect = guiRect;
        }
    }

    private void OnGUIPreviewClipsOptions()
    {
        if (anim.clips == null || anim.clips.Length == 0 || preview == null)
        {
            return;
        }

        previewClipIndex = Mathf.Clamp(previewClipIndex, 0, anim.clips.Length - 1);
        string[] options = new string[anim.clips.Length];
        for (int i = 0; i < anim.clips.Length; ++i)
        {
            options[i] = anim.clips[i].name;
        }
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            previewClipIndex = EditorGUILayout.Popup(string.Empty, previewClipIndex, options);
            if (EditorGUI.EndChangeCheck())
            {
                preview.Player.Play(options[previewClipIndex]);
            }
            if (preview.Player.IsPlaying && !preview.Player.IsTimeAtTheEndOfLoop)
            {
                if (GUILayout.Button("||", GUILayout.Width(50)))
                {
                    preview.Player.Stop();
                }
            }
            else
            {
                Color guiColor = GUI.color;
                GUI.color = Color.red;
                if (GUILayout.Button(">", GUILayout.Width(50)))
                {
                    if (preview.Player.IsTimeAtTheEndOfLoop)
                    {
                        preview.Player.Play(options[previewClipIndex]);
                    }
                    else
                    {
                        preview.Player.Resume();
                    }
                }
                GUI.color = guiColor;
            }
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    private bool animTimeline_dragging = false;
    private void OnGUIAnimTimeline()
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.Space();
            BeginBox();
            {
                EditorGUILayout.Space();
                if (isAnimEventsFoldout) EditorGUILayout.BeginVertical(GUILayout.Height(120));
                else EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        isAnimEventsFoldout = EditorGUILayout.Foldout(isAnimEventsFoldout, isAnimEventsFoldout ? string.Empty : "Events");
                        SetEditorPrefsBool("isAnimEventsFoldout", isAnimEventsFoldout);
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    if (isAnimEventsFoldout)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        rect.x += 10;
                        rect.y += 20;
                        rect.width -= 20;
                        EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.2f));
                        OnGUIAnimEvents_DrawThumb(rect, preview.Player.NormalizedTime, animTimeline_dragging);
                        Event e = Event.current;
                        Vector2 mousePos = e.mousePosition;
                        if (rect.Contains(mousePos))
                        {
                            if (e.type == EventType.MouseDown)
                            {
                                animTimeline_dragging = true;
                                OnGUI_AnimTimeline_DraggingUpdate(mousePos, rect);
                            }
                        }
                        if (e.type == EventType.MouseUp)
                        {
                            animTimeline_dragging = false;
                        }
                        if (animTimeline_dragging && e.type == EventType.MouseDrag)
                        {
                            OnGUI_AnimTimeline_DraggingUpdate(mousePos, rect);
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EndBox();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndHorizontal();
    }

    private Rect OnGUIAnimEvents_DrawThumb(Rect bgRect, float value01, bool isDragging)
    {
        Color c = isDragging ? new Color(0, 0.6f, 0.6f, 0.6f) : new Color(0, 0, 0, 0.5f);
        Color bc = new Color(0, 0, 0, 0.3f);

        Rect rectThumb = bgRect;
        rectThumb.y -= 4;
        rectThumb.height += 8;
        rectThumb.width = 10;
        rectThumb.x = bgRect.x + bgRect.width * value01 - rectThumb.width * 0.5f;
        EditorGUI.DrawRect(rectThumb, c);

        float borderSize = 1f;
        Rect rectBorder = rectThumb;
        rectBorder.width = borderSize;
        rectBorder.x -= borderSize;
        EditorGUI.DrawRect(rectBorder, bc);
        rectBorder.x += rectThumb.width + borderSize;
        EditorGUI.DrawRect(rectBorder, bc);
        rectBorder = rectThumb;
        rectBorder.height = borderSize;
        rectBorder.y -= borderSize;
        EditorGUI.DrawRect(rectBorder, bc);
        rectBorder.y += rectThumb.height + borderSize;
        EditorGUI.DrawRect(rectBorder, bc);

        return rectThumb;
    }

    private float OnGUI_AnimTimeline_MouseDown_NormalizedTime(Vector2 mousePos, Rect rect)
    {
        return Mathf.Clamp01((mousePos.x - rect.x) / rect.width);
    }


    private void OnGUI_AnimTimeline_DraggingUpdate(Vector2 mousePos, Rect rect)
    {
        float normalizedTime = OnGUI_AnimTimeline_MouseDown_NormalizedTime(mousePos, rect);
        preview.Player.NormalizedTime = normalizedTime;
        OnGUI_AnimTimeline_PlayerUpdate();
    }

    private void OnGUI_AnimTimeline_PlayerUpdate()
    {
        preview.Player.Resume();
        preview.Player.Update_Internal(0);
        preview.Player.Stop();
    }

    private void OnGUIRootMotion()
    {
        List<GPUSkinningAnimationClip> rootMotionClips = new List<GPUSkinningAnimationClip>();
        for (int i = 0; i < anim.clips.Length; ++i)
        {
            if (anim.clips[i].rootMotionEnabled)
            {
                rootMotionClips.Add(anim.clips[i]);
            }
        }

        if (rootMotionClips.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space();
                BeginBox();
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            isRootMotionFoldout = EditorGUILayout.Foldout(isRootMotionFoldout, isRootMotionFoldout ? string.Empty : "Root Motion");
                            SetEditorPrefsBool("isRootMotionFoldout", isRootMotionFoldout);
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        if (isRootMotionFoldout)
                        {
                            EditorGUI.BeginChangeCheck();
                            GUI.enabled = anim.clips[previewClipIndex].rootMotionEnabled && guiEnabled;
                            rootMotionEnabled = EditorGUILayout.Toggle("Apply Root Motion", rootMotionEnabled);
                            GUI.enabled = true && guiEnabled;
                            if (EditorGUI.EndChangeCheck())
                            {
                                preview.Player.RootMotionEnabled = rootMotionEnabled;
                            }
                        }

                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                EndBox();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void OnGUIEditBounds()
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.Space();
            BeginBox();
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        isBoundsFoldout = EditorGUILayout.Foldout(isBoundsFoldout, isBoundsFoldout ? string.Empty : "Bounds");
                        SetEditorPrefsBool("isBoundsFoldout", isBoundsFoldout);
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    if (isBoundsFoldout)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Bounds");
                            boundsAutoExt = GUILayout.HorizontalSlider(boundsAutoExt, 0.0f, 1.0f);
                            if (GUILayout.Button("Calculate Auto", GUILayout.Width(100)))
                            {
                                CalculateBoundsAuto();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();

                        isBoundsVisible = EditorGUILayout.Toggle("Visible", isBoundsVisible);

                        EditorGUILayout.Space();

                        Color tempGUIColor = GUI.color;
                        Vector3 boundsCenter = bounds.center;
                        Vector3 boundsExts = bounds.extents;
                        {
                            GUI.color = Color.red;
                            boundsCenter.x = EditorGUILayout.Slider("center.x", boundsCenter.x, -5, 5);
                            boundsExts.x = EditorGUILayout.Slider("extends.x", boundsExts.x, 0.1f, 5);

                            GUI.color = Color.green;
                            boundsCenter.y = EditorGUILayout.Slider("center.y", boundsCenter.y, -5, 5);
                            boundsExts.y = EditorGUILayout.Slider("extends.y", boundsExts.y, 0.1f, 5);
                            GUI.color = Color.blue;
                            boundsCenter.z = EditorGUILayout.Slider("center.z", boundsCenter.z, -5, 5);
                            boundsExts.z = EditorGUILayout.Slider("extends.z", boundsExts.z, 0.1f, 5);
                        }
                        bounds.center = boundsCenter;
                        bounds.extents = boundsExts;
                        GUI.color = tempGUIColor;

                        EditorGUILayout.Space();

                        if (GUILayout.Button("Apply"))
                        {
                            mesh.bounds = bounds;
                            anim.bounds = bounds;
                            EditorUtility.SetDirty(mesh);
                            ApplyAnimModification();
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EndBox();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void CalculateBoundsAuto()
    {
        Matrix4x4[] matrices = anim.clips[0].frames[0].matrices;
        Matrix4x4 rootMotionInv = anim.clips[0].rootMotionEnabled ? matrices[anim.rootBoneIndex].inverse : Matrix4x4.identity;
        GPUSkinningBone[] bones = anim.bones;
        Vector3 min = Vector3.one * 9999;
        Vector3 max = min * -1;
        for (int i = 0; i < bones.Length; ++i)
        {
            Vector4 pos = (rootMotionInv * matrices[i] * bones[i].bindpose.inverse) * new Vector4(0, 0, 0, 1);
            min.x = Mathf.Min(min.x, pos.x);
            min.y = Mathf.Min(min.y, pos.y);
            min.z = Mathf.Min(min.z, pos.z);
            max.x = Mathf.Max(max.x, pos.x);
            max.y = Mathf.Max(max.y, pos.y);
            max.z = Mathf.Max(max.z, pos.z);
        }
        min -= Vector3.one * boundsAutoExt;
        max += Vector3.one * boundsAutoExt;
        bounds.min = min;
        bounds.max = max;
    }

    private void ApplyAnimModification()
    {
        if (preview != null && anim != null)
        {
            EditorUtility.SetDirty(anim);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}