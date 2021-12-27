using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;





public class GPUSkinningMeshConvertEditor : EditorWindow
{
    [MenuItem("Resources Check/GPU Skin Gen Normalize", priority = 2)]
    public static void ShowWindow()
    {
        var win          = GetWindowWithRect<GPUSkinningMeshConvertEditor>( new Rect(0, 0, 500, 500) );
        win.titleContent = new GUIContent("生成GPU蒙皮法线数据");
    }


    private List<string>    convertFiles = new List<string>();
    private FileView        fileView = null;
    private void OnGUI()
    {
        GUILayout.BeginVertical("Box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        DrawFiles();
        DrawToolbar();
        GUILayout.EndVertical();
    }


    private void OnEnable()
    {
        fileView            = new FileView(convertFiles);
    }


    private void DrawFiles()
    {
        GUILayout.BeginVertical("Box");
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("文件列表");
            EditorGUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            {
                var viewrect = GUILayoutUtility.GetRect(GUIContent.none, "Box", GUILayout.ExpandWidth(true), GUILayout.Height(420));
                var dragObjs = GetDrawObject(viewrect);
                foreach( var obj in dragObjs )
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    AddFile(path);
                }

                if (fileView.GetRows().Count != convertFiles.Count)
                    fileView.Reload();
                fileView.OnGUI(viewrect);

                var previewSize = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(150), GUILayout.Height(200));
                GUI.Box(previewSize, "", "AnimationKeyframeBackground");
                if( 0 <= fileView.SelectIndex && fileView.SelectIndex < convertFiles.Count )
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Mesh>(convertFiles[fileView.SelectIndex]);
                    if( obj )
                    {
                        bool flag = AssetPreview.IsLoadingAssetPreview(obj.GetInstanceID());
                        Texture2D previewMesh = AssetPreview.GetAssetPreview(obj);
                        if( !previewMesh )
                        {
                            if (flag)
                                Repaint();

                            previewMesh = AssetPreview.GetMiniThumbnail(obj);
                        }
                        EditorGUI.DrawTextureTransparent(previewSize, previewMesh, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUI.Label(previewSize, "数据错误");
                    }
                }
                else
                {
                    GUI.Label(previewSize, "None");
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }


    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal("Box", GUILayout.Height(50), GUILayout.ExpandWidth(true));
        {
            if( GUILayout.Button("开始转化", GUILayout.ExpandHeight(true), GUILayout.Width( 20), GUILayout.ExpandWidth(true)))
            {
                float fprosses = 1f * convertFiles.Count;
                EditorUtility.ClearProgressBar();
                for( int i = 0; i < convertFiles.Count; i++ )
                {
                    var filePath = convertFiles[i];
                    EditorUtility.DisplayProgressBar("convert mesh", "convert mesh with of normal!!", i / fprosses );
                    ConvertMesh(filePath);
                }
                EditorUtility.ClearProgressBar();
            }
        }
        GUILayout.EndHorizontal();
    }


    public void AddFile( params string[] files )
    {
        foreach( var file in files )
        {
            if (!convertFiles.Contains(file))
                convertFiles.Add(file);
        }
    }

    public void ClearFile() => convertFiles.Clear();


    private UnityEngine.Object[] GetDrawObject(Rect area )
    {
        var result = new UnityEngine.Object[0];
        if( Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform )
        {
            if( area.Contains( Event.current.mousePosition ))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if( Event.current.type == EventType.DragPerform )
                {
                    DragAndDrop.AcceptDrag();
                    result = DragAndDrop.objectReferences;
                }
                Event.current.Use();
            }
        }

        return result;
    }

    private void ConvertMesh( string file )
    {
        if( !string.IsNullOrEmpty(file) )
        {
            Mesh temp = AssetDatabase.LoadAssetAtPath<Mesh>(file);
            if( temp != null )
            {
                temp.RecalculateNormals();
                Vector3[] normals = SmoothMeshNormal( temp, 60f );
                temp.normals      = normals;
            }
            EditorUtility.SetDirty(temp);
            AssetDatabase.SaveAssets();
        }
    }

    public Vector3[] SmoothMeshNormal( Mesh mesh, float angle )
    {
        float cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);
        Vector3[] vertices    = mesh.vertices;
        Vector3[] normals     = new Vector3[ vertices.Length ];
        Vector3[] triNormals;
        var dictionary        = new Dictionary<VertexKey, List<VertexEntry>>(vertices.Length);

        {
            var triangles     = mesh.triangles;
            triNormals        = new Vector3[triangles.Length / 3 ];
            for( int i = 0; i < triangles.Length; i += 3 )
            {
                int v1        = triangles[i];
                int v2        = triangles[i+1];
                int v3        = triangles[i+2];

                Vector3 p1      = vertices[v2] - vertices[v1];
                Vector3 p2      = vertices[v3] - vertices[v1];
                Vector3 normal  = Vector3.Cross(p1, p2).normalized;
                int triIndex    = i / 3;
                triNormals[triIndex] = normal;

                List<VertexEntry> entry;
                VertexKey key;
                if( !dictionary.TryGetValue( key = new VertexKey(vertices[v1]), out entry))
                {
                    entry = new List<VertexEntry>(4);
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(0, triIndex, v1));

                if (!dictionary.TryGetValue(key = new VertexKey(vertices[v2]), out entry))
                {
                    entry = new List<VertexEntry>(4);
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(0, triIndex, v2));

                if (!dictionary.TryGetValue(key = new VertexKey(vertices[v3]), out entry))
                {
                    entry = new List<VertexEntry>(4);
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(0, triIndex, v3));
            }
        }

        foreach( var vertList in dictionary.Values )
        {
            for( var i = 0; i < vertList.Count; ++i )
            {
                var Smooth   = new Vector3();
                var lhsEntry = vertList[i];
                for (var n = 0; n < vertList.Count; ++n)
                {
                    var rhsEntry = vertList[n];
                    if( lhsEntry.VertexIndex == rhsEntry.VertexIndex )
                    {
                        Smooth += triNormals[rhsEntry.TriangleIndex];
                    }
                    else
                    {
                        var dot = Vector3.Dot(triNormals[lhsEntry.TriangleIndex], triNormals[rhsEntry.TriangleIndex]);
                        if (dot >= cosineThreshold)
                            Smooth += triNormals[rhsEntry.TriangleIndex];
                    }
                }

                normals[lhsEntry.VertexIndex] = Smooth.normalized;
            }
        }
        return normals;
    }

    private struct VertexKey
    {
        private readonly long _x;
        private readonly long _y;
        private readonly long _z;
        private const int  Tolerance  = 100000;
        private const long FNV32Init  = 0x811c9dc5;
        private const long FNV32Prime = 0x01000193;

        public VertexKey( Vector3 position )
        {
            _x = (long)(Mathf.Round(position.x * Tolerance));
            _y = (long)(Mathf.Round(position.y * Tolerance));
            _z = (long)(Mathf.Round(position.z * Tolerance));
        }

        public override bool Equals(object obj)
        {
            var key = (VertexKey)obj;
            return _x == key._x && _y == key._y && _z == key._z;
        }
        public override int GetHashCode()
        {
            long rv = FNV32Init;
            rv      ^= _x;
            rv      *= FNV32Prime;
            rv      ^= _y;
            rv      *= FNV32Prime;
            rv      ^= _z;
            rv      *= FNV32Prime;
            return rv.GetHashCode();
        }
    }

    private struct VertexEntry
    {
        public int MeshIndex;
        public int TriangleIndex;
        public int VertexIndex;

        public VertexEntry(int meshIndex, int triIndex, int vertIndex)
        {
            MeshIndex       = meshIndex;
            TriangleIndex   = triIndex;
            VertexIndex     = vertIndex;
        }
    }

    public class FileView : TreeView
    {
        public int           SelectIndex = -1;
        private List<string> m_filesList  = null;
        public FileView( List<string> files ) : base ( new TreeViewState() )
        {
            m_filesList         = files;
            showBorder          = false;
            showAlternatingRowBackgrounds = true;
            useScrollView       = true;
            MultiColumnHeaderState.Column[] array = new MultiColumnHeaderState.Column[1];
            array[0]            = new MultiColumnHeaderState.Column();
            array[0].width      = 1000f;
            array[0].canSort    = false;


            MultiColumnHeaderState state = new MultiColumnHeaderState(array);
            multiColumnHeader   = new MultiColumnHeader(state)
            {
                height          = 0
            };
            multiColumnHeader.ResizeToFit();
            Reload();
        }

        public Action<List<int>> selectionChangedCallBack
        {
            get;
            set;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var rect        = args.GetCellRect(0);
            CenterRectUsingSingleLineHeight(ref rect);
            var value       = m_filesList[args.item.id ];
            GUI.Label(rect, value);
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem
            {
                id = -1,
                depth = -1,
                displayName = "Root",
            };

            List<TreeViewItem> list = new List<TreeViewItem>(m_filesList.Count);
            for( int i = 0; i < m_filesList.Count; i++ )
            {
                TreeViewItem item = new TreeViewItem(i, 0, m_filesList[i]);
                list.Add(item);
            }
            SetupParentsAndChildrenFromDepths(root, list);
            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if( selectionChangedCallBack != null )
            {
                selectionChangedCallBack.Invoke(Enumerable.ToList<int>(selectedIds));
            }

            if( selectedIds.Count > 0 )
            {
                SelectIndex = selectedIds[0];
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return false;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            Reload();
        }

        protected override void KeyEvent()
        {
            base.KeyEvent();
            if( Event.current.type == EventType.ValidateCommand || Event.current.type == EventType.ExecuteCommand )
            {
                if( Event.current.commandName != null && (Event.current.commandName == "SoftDelete" || Event.current.commandName == "Delete" ))
                {
                    if( 0 <= SelectIndex && SelectIndex < m_filesList.Count )
                    {
                        m_filesList.RemoveAt(SelectIndex);
                        Reload();
                    }

                    Event.current.Use();
                }
            }
        }
    }
}
