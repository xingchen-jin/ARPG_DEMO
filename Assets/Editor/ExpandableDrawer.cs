#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// [Expandable] 特性的编辑器抽屉：
/// 让挂在场景脚本上的 ScriptableObject 引用，可以直接在 Inspector 中展开编辑，
/// 不必再单独点开 SO 资源文件。
/// </summary>
[CustomPropertyDrawer(typeof(ExpandableAttribute))]
public class ExpandableDrawer : PropertyDrawer
{
    // 按引用对象缓存 Editor，多个字段 / 数组元素各自独立，避免互相覆盖；复用缓存以提升性能
    private readonly Dictionary<Object, Editor> _editors = new Dictionary<Object, Editor>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        Object target = property.objectReferenceValue;
        if (target == null || !property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight;                                             // 标题行
        height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;       // 引用字段
        height += EditorGUIUtility.standardVerticalSpacing + GetNestedHeight(GetEditor(target));      // 嵌套 Inspector
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Object target = property.objectReferenceValue;

        // 空引用：退化为普通对象字段
        if (target == null)
        {
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
            return;
        }

        // 1. 标题行：折叠箭头 + 字段名 + 资源名（点击标签也可折叠）
        Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

        Rect nameRect = new Rect(
            headerRect.x + EditorGUIUtility.labelWidth,
            headerRect.y,
            headerRect.width - EditorGUIUtility.labelWidth,
            headerRect.height);
        GUI.Label(nameRect, target.name, EditorStyles.miniLabel);

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        // 2. 对象引用字段（可拖拽更换资源）
        Rect objectRect = new Rect(
            position.x,
            headerRect.yMax + EditorGUIUtility.standardVerticalSpacing,
            position.width,
            EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(objectRect, property, GUIContent.none);

        // 3. 嵌套 Inspector（逐字段矩形绘制，全宽显示，避免 GUILayout 嵌套导致的半宽/错位）
        Editor editor = GetEditor(target);
        if (editor != null)
        {
            Rect nestedRect = new Rect(
                position.x,
                objectRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                GetNestedHeight(editor));

            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            DrawNested(editor.serializedObject, nestedRect);
            if (EditorGUI.EndChangeCheck())
            {
                editor.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    /// <summary>
    /// 用矩形布局逐字段绘制嵌套 SO，替代 GUILayout 方式：
    /// 每个字段都占满可用宽度，不存在 BeginArea 内 GUILayout 宽度计算错误（半宽）的问题。
    /// </summary>
    private void DrawNested(SerializedObject so, Rect rect)
    {
        if (so == null) return;
        so.Update();

        float y = rect.y;
        SerializedProperty it = so.GetIterator();
        bool enterChildren = true;
        while (it.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (it.propertyPath == "m_Script") continue;   // 跳过脚本引用

            float h = EditorGUI.GetPropertyHeight(it, true);
            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, h), it, true);
            y += h + EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private Editor GetEditor(Object target)
    {
        // 惰性清理已销毁的引用，防止缓存无限增长
        if (_editors.Count > 8)
        {
            List<Object> dead = null;
            foreach (KeyValuePair<Object, Editor> kv in _editors)
            {
                if (kv.Value == null || kv.Key == null)
                {
                    if (dead == null) dead = new List<Object>();
                    dead.Add(kv.Key);
                }
            }
            if (dead != null)
            {
                foreach (Object key in dead) _editors.Remove(key);
            }
        }

        if (!_editors.TryGetValue(target, out Editor editor) || editor == null)
        {
            // 此重载返回 void，结果通过 ref 参数回传
            Editor.CreateCachedEditor(target, null, ref editor);
            _editors[target] = editor;
        }
        return editor;
    }

    /// <summary>遍历所有可见字段累加高度，精确计算嵌套 Inspector 的高度。</summary>
    private static float GetNestedHeight(Editor editor)
    {
        if (editor == null || editor.serializedObject == null) return 0f;

        float height = 0f;
        SerializedProperty it = editor.serializedObject.GetIterator();
        bool enterChildren = true;
        while (it.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (it.propertyPath == "m_Script") continue;   // 与 DrawNested 保持一致，跳过脚本引用

            height += EditorGUI.GetPropertyHeight(it, true) + EditorGUIUtility.standardVerticalSpacing;
        }
        return height;
    }
}
#endif