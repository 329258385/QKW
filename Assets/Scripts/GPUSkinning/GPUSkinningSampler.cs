/*
-----------------------------------------------------------------------------------------------------
    骨骼动画---普通骨骼动画转GPU骨骼动画
    log:  add by ljp 2021--3-29
-----------------------------------------------------------------------------------------------------
*/
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif





[ExecuteInEditMode]
public class GPUSkinningSampler : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    public string                   animName = null;

    [HideInInspector]
    [System.NonSerialized]
    public AnimationClip            animClip = null;

    [HideInInspector]
    [SerializeField]
    public AnimationClip[]          animClips = null;

    [HideInInspector]
    [SerializeField]
    public GPUSkinningWrapMode[]    wrapModes = null;

    [HideInInspector]
    [SerializeField]
    public int[]                    fpsList = null;

    [HideInInspector]
    [SerializeField]
    public bool[]                   rootMotionEnabled = null;

    [HideInInspector]
    [SerializeField]
    public bool[]                   individualDifferenceEnabled = null;

    [HideInInspector]
    [SerializeField]
    public Mesh[]                   lodMeshes = null;

    [HideInInspector]
    [SerializeField]
    public float[]                  lodDistances = null;

    [HideInInspector]
    [SerializeField]
    private float                   sphereRadius = 1.0f;

    [HideInInspector]
    [System.NonSerialized]
    public int                      samplingClipIndex = -1;

    [HideInInspector]
    [SerializeField]
    public TextAsset                texture = null;

    [HideInInspector]
    [SerializeField]
    public GPUSkinningQuality       skinQuality = GPUSkinningQuality.Bone2;

    [HideInInspector]
    [SerializeField]
    public Transform                rootBoneTransform = null;

    [HideInInspector]
    [SerializeField]
    public GPUSkinningAnimation     anim = null;

    [HideInInspector]
    [SerializeField]
    public GPUSkinningShaderType    shaderType = GPUSkinningShaderType.Unlit;

    [HideInInspector]
    [System.NonSerialized]
    public bool                     isSampling = false;

    [HideInInspector]
    [SerializeField]
    public Mesh                     savedMesh = null;

    [HideInInspector]
    [SerializeField]
    public Material                 savedMtrl = null;

    [HideInInspector]
    [SerializeField]
    public Shader                   savedShader = null;

    [HideInInspector]
    [SerializeField]
    public bool                     updateOrNew = true;

    [HideInInspector]
    [System.NonSerialized]
    public int                      samplingTotalFrams = 0;

    [HideInInspector]
    [System.NonSerialized]
    public int                      samplingFrameIndex = 0;

    ///-------------------------------------------------------------------------------------------------
    private Animation               animation = null;
    private Animator                animator = null;
    private RuntimeAnimatorController runtimeAnimatorController = null;
    private SkinnedMeshRenderer     smr = null;
    private GPUSkinningAnimation    gpuSkinningAnimation = null;
    private GPUSkinningAnimationClip gpuSkinningClip = null;
    private Vector3                 rootMotionPosition;
    private Quaternion              rootMotionRotation;

    public const string TEMP_SAVED_ANIM_PATH    = "GPUSkinning_Temp_Save_Anim_Path";
    public const string TEMP_SAVED_MTRL_PATH    = "GPUSkinning_Temp_Save_Mtrl_Path";
    public const string TEMP_SAVED_MESH_PATH    = "GPUSkinning_Temp_Save_Mesh_Path";
    public const string TEMP_SAVED_SHADER_PATH  = "GPUSkinning_Temp_Save_Shader_Path";
    public const string TEMP_SAVED_TEXTURE_PATH = "GPUSkinning_Temp_Save_Texture_Path";


    private void Awake()
    {
        animation           = GetComponent<Animation>();
        animator            = GetComponent<Animator>();
        if (animator == null && animation == null)
        {
            DestroyImmediate(this);
            ShowDialog("Cannot find Animator Or Animation Component");
            return;
        }

        if (animator != null && animation != null)
        {
            DestroyImmediate(this);
            ShowDialog("Animation is not coexisting with Animator");
            return;
        }

        if(animator != null )
        {
            if( animator.runtimeAnimatorController == null )
            {
                DestroyImmediate(this);
                ShowDialog("Missing RuntimeAnimatorController");
                return;
            }

            if (animator.runtimeAnimatorController is AnimatorOverrideController)
            {
                DestroyImmediate(this);
                ShowDialog("RuntimeAnimatorController could not be a AnimatorOverrideController");
                return;
            }

            this.runtimeAnimatorController = animator.runtimeAnimatorController;
            this.animator.cullingMode      = AnimatorCullingMode.AlwaysAnimate;
            InitTransform();
            return;
        }

        if( animation != null )
        {
            animation.Stop();
            animation.cullingType = AnimationCullingType.AlwaysAnimate;
            InitTransform();
        }
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (!isSampling)
        {
            return;
        }

        int totalFrams = (int)(gpuSkinningClip.length * gpuSkinningClip.fps);
        samplingTotalFrams = totalFrams;
        if( samplingFrameIndex >= totalFrams )
        {
            if (animator != null)
            {
                animator.StopPlayback();
            }

            string savePath = null;
            if (anim == null)
            {
                savePath        = EditorUtility.SaveFolderPanel("GPUSkinning Sampler Save", GetUserPreferDir(), animName);
            }
            else
            {
                string animPath = AssetDatabase.GetAssetPath(anim);
                savePath        = new FileInfo(animPath).Directory.FullName.Replace('\\', '/');
            }

            if (!string.IsNullOrEmpty(savePath))
            {
                if (!savePath.Contains(Application.dataPath.Replace('\\', '/')))
                {
                    ShowDialog("Must select a directory in the project's Asset folder.");
                }
                else
                {
                    SaveUserPreferDir(savePath);
                    string dir           = "Assets" + savePath.Substring(Application.dataPath.Length);
                    string savedAnimPath = dir + "/GPUSK_Anim_" + animName + ".asset";

                    SetSthAboutTexture(gpuSkinningAnimation);
                    EditorUtility.SetDirty(gpuSkinningAnimation);
                    if (anim != gpuSkinningAnimation)
                    {
                        AssetDatabase.CreateAsset(gpuSkinningAnimation, savedAnimPath);
                    }
                    WriteTempData(TEMP_SAVED_ANIM_PATH, savedAnimPath);
                    anim            = gpuSkinningAnimation;

                    CreateTextureMatrix(dir, anim);
                    if (samplingClipIndex == 0)
                    {

                        SkinnedMeshRenderer[] meshFilter = this.GetComponentsInChildren<SkinnedMeshRenderer>();
                        Mesh newMesh = null;
                        if (meshFilter.Length > 1)
                        {
                            Matrix4x4 matrix = transform.worldToLocalMatrix;
                            CombineInstance[] combines = new CombineInstance[meshFilter.Length];
                            for (int i = 0; i < meshFilter.Length; i++)
                            {
                                combines[i].mesh = meshFilter[i].sharedMesh;
                                combines[i].transform = meshFilter[i].transform.localToWorldMatrix * matrix;
                            }
                            
                            Mesh temp = new Mesh();
                            temp.CombineMeshes(combines, false);
                            newMesh = CreateNewMesh(temp, "GPUSK_Mesh");
                            temp = null;
                        }
                        else
                        {
                            newMesh = CreateNewMesh(smr.sharedMesh, "GPUSK_Mesh");
                        }
                        
                        if (savedMesh != null)
                        {
                            newMesh.bounds = savedMesh.bounds;
                        }
                        string savedMeshPath    = dir + "/GPUSK_Mesh_" + animName + ".asset";
                        AssetDatabase.CreateAsset(newMesh, savedMeshPath);
                        WriteTempData(TEMP_SAVED_MESH_PATH, savedMeshPath);
                        savedMesh               = newMesh;
                        CreateShaderAndMaterial(dir);
                    }

                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                }
            }

            isSampling = false;
            return;
        }
        float time = gpuSkinningClip.length * ((float)samplingFrameIndex / totalFrams);
        GPUSkinningFrame frame = new GPUSkinningFrame();
        gpuSkinningClip.frames[samplingFrameIndex] = frame;
        frame.matrices = new Matrix4x4[gpuSkinningAnimation.bones.Length];
        if( animation == null )
        {
            animator.playbackTime = time;
            animator.Update(0);
        }
        else
        {
            animation.Stop();
            AnimationState animState = animation[animClip.name];
            if( animState != null )
            {
                animState.time = time;
                animation.Sample();
                animation.Play();
            }
        }

        StartCoroutine(SamplingCoroutine(frame, totalFrams));
        #endif
    }


    private IEnumerator SamplingCoroutine( GPUSkinningFrame frame, int totalFrames )
    {
        yield return new WaitForEndOfFrame();

        GPUSkinningBone[] bones = gpuSkinningAnimation.bones;
        int numBones = bones.Length;
        for( int i = 0; i < numBones; ++i )
        {
            Transform boneTransform = bones[i].transform;
            GPUSkinningBone currentBone = GetBoneByTransform(boneTransform);
            frame.matrices[i] = currentBone.bindpose;
            do
            {
                Matrix4x4 mat = Matrix4x4.TRS(currentBone.transform.localPosition, currentBone.transform.localRotation, currentBone.transform.localScale);
                frame.matrices[i] = mat * frame.matrices[i];
                if (currentBone.parentBoneIndex == -1)
                    break;
                else
                    currentBone = bones[currentBone.parentBoneIndex];
            }
            while (true);
        }

        if( samplingFrameIndex == 0 )
        {
            rootMotionPosition      = bones[gpuSkinningAnimation.rootBoneIndex].transform.localPosition;
            rootMotionRotation      = bones[gpuSkinningAnimation.rootBoneIndex].transform.localRotation;
        }
        else
        {
            Vector3 newPosition     = bones[gpuSkinningAnimation.rootBoneIndex].transform.localPosition;
            Quaternion newRotation  = bones[gpuSkinningAnimation.rootBoneIndex].transform.localRotation;
            Vector3 deltaPosition   = newPosition - rootMotionPosition;
            frame.rootMotionDeltaPositionQ  = Quaternion.Inverse(Quaternion.Euler(transform.forward.normalized)) * Quaternion.Euler(deltaPosition.normalized);
            frame.rootMotionDeltaPositionL  = deltaPosition.magnitude;
            frame.rootMotionDeltaRotation   = Quaternion.Inverse(rootMotionRotation) * newRotation;
            rootMotionPosition              = newPosition;
            rootMotionRotation              = newRotation;

            if (samplingFrameIndex == 1)
            {
                gpuSkinningClip.frames[0].rootMotionDeltaPositionQ = gpuSkinningClip.frames[1].rootMotionDeltaPositionQ;
                gpuSkinningClip.frames[0].rootMotionDeltaPositionL = gpuSkinningClip.frames[1].rootMotionDeltaPositionL;
                gpuSkinningClip.frames[0].rootMotionDeltaRotation   = gpuSkinningClip.frames[1].rootMotionDeltaRotation;
            }
        }
        ++samplingFrameIndex;
    }

    /// <summary>
    /// 计算存储本animtor 需要多大的纹理
    /// </summary>
    /// <param name="gpuSkinningAnim"></param>
    private void SetSthAboutTexture( GPUSkinningAnimation gpuSkinningAnim )
    {
        int numPixels = 0;
        GPUSkinningAnimationClip[] clips = gpuSkinningAnim.clips;
        int numClips = clips.Length;
        for( int clipIndex = 0; clipIndex < numClips; ++clipIndex )
        {
            GPUSkinningAnimationClip clip   = clips[clipIndex];
            clip.pixelSegmentation          = numPixels;

            GPUSkinningFrame[] frames       = clip.frames;
            int numFrames                   = frames.Length;
            numPixels                       += gpuSkinningAnim.bones.Length * 3 * numFrames;
        }

        CalculateTextureSize(numPixels, out gpuSkinningAnim.textureWidth, out gpuSkinningAnim.textureHeight);
    }

    /// <summary>
    /// 动画帧数据存储到纹理
    /// </summary>
    private void CreateTextureMatrix( string dir, GPUSkinningAnimation  gpuSkinningAnim )
    {
        Texture2D texture   = new Texture2D(gpuSkinningAnim.textureWidth, gpuSkinningAnim.textureHeight, TextureFormat.RGBAHalf, false, true);
        Color[] pixels      = texture.GetPixels();
        int pixelIndex      = 0;
        for( int clipIndex = 0; clipIndex < gpuSkinningAnim.clips.Length; ++clipIndex )
        {
            GPUSkinningAnimationClip clip   = gpuSkinningAnim.clips[clipIndex];
            GPUSkinningFrame[] frames       = clip.frames;
            int numFrames                   = frames.Length;
            for( int frameIndex = 0; frameIndex < numFrames; ++frameIndex )
            {
                /// 当前帧所有骨骼的位移、缩放、旋转信息
                GPUSkinningFrame frame      = frames[frameIndex];
                Matrix4x4[] matrices        = frame.matrices;
                int numMatrices             = matrices.Length;
                for( int matrixIndex = 0; matrixIndex < numMatrices; ++matrixIndex )
                {
                    Matrix4x4 matrix        = matrices[matrixIndex];
                    pixels[pixelIndex++]    = new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03);
                    pixels[pixelIndex++]    = new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13);
                    pixels[pixelIndex++]    = new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23);
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        string savedPath    = dir + "/GPUSK_Texture_" + animName + ".bytes";
        using (FileStream fileStream = new FileStream(savedPath, FileMode.Create))
        {
            byte[] bytes = texture.GetRawTextureData();
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Flush();
            fileStream.Close();
            fileStream.Dispose();
        }
        WriteTempData(TEMP_SAVED_TEXTURE_PATH, savedPath);
    }

    /// <summary>
    /// 创建相关的shader 和材质
    /// </summary>
    private void CreateShaderAndMaterial(string dir)
    {
#if UNITY_EDITOR
        Shader shader = null;
        {
            string shaderName   = "GPUSkinning/GPUSkinning_Unlit_Skin";
            shaderName          +=
                                    skinQuality == GPUSkinningQuality.Bone1 ? 1 :
                                    skinQuality == GPUSkinningQuality.Bone2 ? 2 :
                                    skinQuality == GPUSkinningQuality.Bone4 ? 4 : 1;

            shader              = Shader.Find(shaderName);
            WriteTempData(TEMP_SAVED_SHADER_PATH, AssetDatabase.GetAssetPath(shader));
        }

        Material mtrl = new Material(shader);
        if (smr.sharedMaterial != null)
        {
            mtrl.CopyPropertiesFromMaterial(smr.sharedMaterial);
        }

        string savedMtrlPath = dir + "/GPUSK_Material_" + animName + ".mat";
        AssetDatabase.CreateAsset(mtrl, savedMtrlPath);
        WriteTempData(TEMP_SAVED_MTRL_PATH, savedMtrlPath);
        #endif
    }

    /// <summary>
    /// 创建新的蒙皮数据
    /// </summary>
    private Mesh CreateNewMesh(Mesh mesh, string meshName)
    {
        Vector3[] normals   = mesh.normals;
        Vector4[] tangents  = mesh.tangents;
        Color[] colors      = mesh.colors;
        Vector2[] uv        = mesh.uv;

        Mesh newMesh        = new Mesh();
        newMesh.name        = meshName;
        newMesh.vertices    = mesh.vertices;
        if (normals != null && normals.Length > 0) { newMesh.normals = normals; }
        if (tangents != null && tangents.Length > 0) { newMesh.tangents = tangents; }
        if (colors != null && colors.Length > 0) { newMesh.colors = colors; }
        if (uv != null && uv.Length > 0) { newMesh.uv = uv; }

        int numVertices         = mesh.vertexCount;
        BoneWeight[] boneWeights = mesh.boneWeights;
        Vector4[] uv2           = new Vector4[numVertices];
        Vector4[] uv3           = new Vector4[numVertices];
        Transform[] smrBones    = smr.bones;
        for (int i = 0; i < numVertices; ++i)
        {
            BoneWeight boneWeight = boneWeights[i];

            BoneWeightSortData[] weights = new BoneWeightSortData[4];
            weights[0] = new BoneWeightSortData() { index = boneWeight.boneIndex0, weight = boneWeight.weight0 };
            weights[1] = new BoneWeightSortData() { index = boneWeight.boneIndex1, weight = boneWeight.weight1 };
            weights[2] = new BoneWeightSortData() { index = boneWeight.boneIndex2, weight = boneWeight.weight2 };
            weights[3] = new BoneWeightSortData() { index = boneWeight.boneIndex3, weight = boneWeight.weight3 };
            System.Array.Sort(weights);
            if(weights[0].index >= smrBones.Length )
            {
                int x = 0;
                x++;
            }

            GPUSkinningBone bone0 = GetBoneByTransform(smrBones[weights[0].index]);
            GPUSkinningBone bone1 = GetBoneByTransform(smrBones[weights[1].index]);
            GPUSkinningBone bone2 = GetBoneByTransform(smrBones[weights[2].index]);
            GPUSkinningBone bone3 = GetBoneByTransform(smrBones[weights[3].index]);

            Vector4 skinData_01 = new Vector4();
            skinData_01.x = GetBoneIndex(bone0);
            skinData_01.y = weights[0].weight;
            skinData_01.z = GetBoneIndex(bone1);
            skinData_01.w = weights[1].weight;
            uv2[i] = skinData_01;

            Vector4 skinData_23 = new Vector4();
            skinData_23.x = GetBoneIndex(bone2);
            skinData_23.y = weights[2].weight;
            skinData_23.z = GetBoneIndex(bone3);
            skinData_23.w = weights[3].weight;
            uv3[i] = skinData_23;
        }
        newMesh.SetUVs(1, new List<Vector4>(uv2));
        newMesh.SetUVs(2, new List<Vector4>(uv3));

        newMesh.triangles = mesh.triangles;
        return newMesh;
    }

    /// <summary>
    /// 创建新的蒙皮数据
    /// </summary>
   
    private class BoneWeightSortData : System.IComparable<BoneWeightSortData>
    {
        public int      index = 0;
        public float    weight = 0;

        public int CompareTo(BoneWeightSortData b)
        {
            return weight > b.weight ? -1 : 1;
        }
    }


    private void CalculateTextureSize( int numPixels, out int texWidth, out int texHeight )
    {
        texWidth    = 1;
        texHeight   = 1;
        while (true)
        {
            if (texWidth * texHeight >= numPixels) break;
            texWidth *= 2;
            if (texWidth * texHeight >= numPixels) break;
            texHeight *= 2;
        }
    }

    private int GetBoneIndex(GPUSkinningBone bone)
    {
        return System.Array.IndexOf(gpuSkinningAnimation.bones, bone);
    }

    private GPUSkinningBone GetBoneByTransform(Transform transform)
    {
        GPUSkinningBone[] bones = gpuSkinningAnimation.bones;
        int numBones = bones.Length;
        for (int i = 0; i < numBones; ++i)
        {
            if (bones[i].transform == transform)
            {
                return bones[i];
            }
        }
        return null;
    }

    private void InitTransform()
    {
        transform.parent        = null;
        transform.position      = Vector3.zero;
        transform.eulerAngles   = Vector3.zero;
    }

    public static void ShowDialog(string msg)
    {
        #if UNITY_EDITOR
        EditorUtility.DisplayDialog("GPUSkinning", msg, "OK");
        #endif
    }
    public static void WriteTempData(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public static string ReadTempData(string key)
    {
        return PlayerPrefs.GetString(key, string.Empty);
    }

    public static void DeleteTempData(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    private void SaveUserPreferDir(string dirPath)
    {
        PlayerPrefs.SetString("GPUSkinning_UserPreferDir", dirPath);
    }

    private string GetUserPreferDir()
    {
        return PlayerPrefs.GetString("GPUSkinning_UserPreferDir", Application.dataPath);
    }

    /// ----------------------------------------------------------------------------------------------------------------
    /// 开始动画采用，并清除数据，为了不用修复脚本
    /// ----------------------------------------------------------------------------------------------------------------
    public void BeginSample()
    {
        this.anim                   = null;
        this.gpuSkinningClip        = null;
        this.samplingClipIndex      = 0;
        this.gpuSkinningAnimation   = null;
    }

    public void EndSample()
    {
        this.anim                   = null;
        this.gpuSkinningClip        = null;
        this.samplingClipIndex      = -1;
        this.gpuSkinningAnimation   = null;
    }

    public bool IsSamplingProgress()
    {
        return samplingClipIndex != -1;
    }

    public bool IsAnimatorOrAnimation()
    {
        return animator != null;
    }

    ///-------------------------------------------------------------------------------------------------------
    /// 开始动作采样到纹理
    ///-------------------------------------------------------------------------------------------------------
    public void StartSample()
    {
        if (isSampling) return;

        if( string.IsNullOrEmpty( animName.Trim() ))
        {
            ShowDialog("Animation name is empty!!!");
            return;
        }

        if (rootBoneTransform == null)
        {
            ShowDialog("Please set Root Bone.");
            return;
        }

        if (animClips == null || animClips.Length == 0)
        {
            ShowDialog("Please set Anim Clips.");
            return;
        }

        animClip = animClips[samplingClipIndex];
        if (animClip == null)
        {
            isSampling = false;
            return;
        }

        int numFrames = (int)(GetClipFPS(animClip, samplingClipIndex) * animClip.length);
        if (numFrames == 0)
        {
            isSampling = false;
            return;
        }

        smr = GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr == null)
        {
            ShowDialog("Cannot find SkinnedMeshRenderer.");
            return;
        }
        if (smr.sharedMesh == null)
        {
            ShowDialog("Cannot find SkinnedMeshRenderer.mesh.");
            return;
        }
        Mesh mesh = smr.sharedMesh;
        if (mesh == null)
        {
            ShowDialog("Missing Mesh");
            return;
        }

        samplingFrameIndex          = 0;
        gpuSkinningAnimation        = anim == null ? ScriptableObject.CreateInstance<GPUSkinningAnimation>() : anim;
        gpuSkinningAnimation.name   = animName;
        if (anim == null)
        {
            gpuSkinningAnimation.guid = System.Guid.NewGuid().ToString();
        }

        /// 骨骼
        List<GPUSkinningBone> bones_result = new List<GPUSkinningBone>();
        CollectBones(bones_result, smr.bones, mesh.bindposes, null, rootBoneTransform, 0);
        GPUSkinningBone[] newBones = bones_result.ToArray();
        GenerateBonesGUID(newBones);
        if (anim != null) RestoreCustomBoneData(anim.bones, newBones);
        gpuSkinningAnimation.bones = newBones;
        gpuSkinningAnimation.rootBoneIndex = 0;

        int numClips = gpuSkinningAnimation.clips == null ? 0 : gpuSkinningAnimation.clips.Length;
        int overrideClipIndex = -1;
        for (int i = 0; i < numClips; ++i)
        {
            if (gpuSkinningAnimation.clips[i].name == animClip.name)
            {
                overrideClipIndex = i;
                break;
            }
        }

        gpuSkinningClip             = new GPUSkinningAnimationClip();
        gpuSkinningClip.name        = animClip.name;
        gpuSkinningClip.fps         = GetClipFPS(animClip, samplingClipIndex);
        gpuSkinningClip.length      = animClip.length;
        gpuSkinningClip.wrapMode    = wrapModes[samplingClipIndex];
        gpuSkinningClip.frames      = new GPUSkinningFrame[numFrames];
        gpuSkinningClip.rootMotionEnabled = rootMotionEnabled[samplingClipIndex];
        gpuSkinningClip.individualDifferenceEnabled = individualDifferenceEnabled[samplingClipIndex];

        if (gpuSkinningAnimation.clips == null)
        {
            gpuSkinningAnimation.clips = new GPUSkinningAnimationClip[] { gpuSkinningClip };
        }
        else
        {
            if (overrideClipIndex == -1)
            {
                List<GPUSkinningAnimationClip> clips = new List<GPUSkinningAnimationClip>(gpuSkinningAnimation.clips);
                clips.Add(gpuSkinningClip);
                gpuSkinningAnimation.clips = clips.ToArray();
            }
            else
            {
                GPUSkinningAnimationClip overridedClip = gpuSkinningAnimation.clips[overrideClipIndex];
                gpuSkinningAnimation.clips[overrideClipIndex] = gpuSkinningClip;
            }
        }

        SetCurrentAnimationClip();
        PrepareRecordAnimator();
        isSampling = true;
    }

    public void MappingAnimationClips()
    {
#if UNITY_EDITOR
        if (animation == null)
            return;

        List<AnimationClip> newClips = null;
        AnimationClip[] clips = AnimationUtility.GetAnimationClips(gameObject);
        if( clips != null )
        {
            for( int i = 0; i < clips.Length; ++i )
            {
                AnimationClip clip = clips[i];
                if( clip != null )
                {
                    if (animClips == null || System.Array.IndexOf(animClips, clip) == -1)
                    {
                        if (newClips == null)
                        {
                            newClips = new List<AnimationClip>();
                        }
                        newClips.Clear();
                        if (animClips != null) newClips.AddRange(animClips);
                        newClips.Add(clip);
                        animClips = newClips.ToArray();
                    }
                }
            }
        }
        if (animClips != null && clips != null)
        {
            for (int i = 0; i < animClips.Length; ++i)
            {
                AnimationClip clip = animClips[i];
                if (clip != null)
                {
                    if (System.Array.IndexOf(clips, clip) == -1)
                    {
                        if (newClips == null)
                        {
                            newClips = new List<AnimationClip>();
                        }
                        newClips.Clear();
                        newClips.AddRange(animClips);
                        newClips.RemoveAt(i);
                        animClips = newClips.ToArray();
                        --i;
                    }
                }
            }
        }
#endif
    }

    private int GetClipFPS(AnimationClip clip, int clipIndex)
    {
        return fpsList[clipIndex] == 0 ? (int)clip.frameRate : fpsList[clipIndex];
    }

    private void CollectBones(List<GPUSkinningBone> bones_result, Transform[] bones_smr, Matrix4x4[] bindposes, GPUSkinningBone parentBone, Transform currentBoneTransform, int currentBoneIndex)
    {
        GPUSkinningBone currentBone = new GPUSkinningBone();
        bones_result.Add(currentBone);

        int indexOfSmrBones     = System.Array.IndexOf(bones_smr, currentBoneTransform);
        currentBone.transform   = currentBoneTransform;
        currentBone.name        = currentBone.transform.gameObject.name;
        currentBone.bindpose    = indexOfSmrBones == -1 ? Matrix4x4.identity : bindposes[indexOfSmrBones];
        currentBone.parentBoneIndex = parentBone == null ? -1 : bones_result.IndexOf(parentBone);
        if (parentBone != null)
        {
            parentBone.childrenBonesIndices[currentBoneIndex] = bones_result.IndexOf(currentBone);
        }

        int numChildren         = currentBone.transform.childCount;
        if (numChildren > 0)
        {
            currentBone.childrenBonesIndices = new int[numChildren];
            for (int i = 0; i < numChildren; ++i)
            {
                CollectBones(bones_result, bones_smr, bindposes, currentBone, currentBone.transform.GetChild(i), i);
            }
        }
    }

    private void GenerateBonesGUID(GPUSkinningBone[] bones)
    {
        int numBones = bones == null ? 0 : bones.Length;
        for (int i = 0; i < numBones; ++i)
        {
            string boneHierarchyPath = GPUSkinningUtil.BoneHierarchyPath(bones, i);
            string guid = GPUSkinningUtil.MD5(boneHierarchyPath);
            bones[i].guid = guid;
        }
    }

    private void RestoreCustomBoneData(GPUSkinningBone[] bonesOrig, GPUSkinningBone[] bonesNew)
    {
        for (int i = 0; i < bonesNew.Length; ++i)
        {
            for (int j = 0; j < bonesOrig.Length; ++j)
            {
                if (bonesNew[i].guid == bonesOrig[j].guid)
                {
                    bonesNew[i].isExposed = bonesOrig[j].isExposed;
                    break;
                }
            }
        }
    }

    private void SetCurrentAnimationClip()
    {
        if (animation == null)
        {
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
            AnimationClip[] clips = runtimeAnimatorController.animationClips;
            AnimationClipPair[] pairs = new AnimationClipPair[clips.Length];
            for (int i = 0; i < clips.Length; ++i)
            {
                AnimationClipPair pair = new AnimationClipPair();
                pairs[i] = pair;
                pair.originalClip = clips[i];
                pair.overrideClip = animClip;
            }
            animatorOverrideController.runtimeAnimatorController = runtimeAnimatorController;
            animatorOverrideController.clips = pairs;
            animator.runtimeAnimatorController = animatorOverrideController;
        }
    }

    private void PrepareRecordAnimator()
    {
        if (animator != null)
        {
            int numFrames = (int)(gpuSkinningClip.fps * gpuSkinningClip.length);

            animator.applyRootMotion = gpuSkinningClip.rootMotionEnabled;
            animator.Rebind();
            animator.recorderStartTime = 0;
            animator.StartRecording(numFrames);
            for (int i = 0; i < numFrames; ++i)
            {
                animator.Update(1.0f / gpuSkinningClip.fps);
            }
            animator.StopRecording();
            animator.StartPlayback();
        }
    }
}

