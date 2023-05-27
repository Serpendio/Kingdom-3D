using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TestSpace
{
    [System.Serializable]
    public class ClassB
    {
        public Vector3 varB;
        public ClassB(Vector3 values)
        {
            varB = values;
        }
    }

    [System.Serializable]
    public class ClassA
    {
        [SerializeReference] private readonly ClassB classB;
        public Vector3 varA;

        public ClassA(Vector3 values) 
        { 
            varA = values;
            classB = new ClassB(values);
        }

    }

    [ExecuteAlways]
    public class NestedSerializeTest : MonoBehaviour
    {
        [SerializeReference] private ClassA classA;
        private void Awake()
        {
            if (classA == null)
            {
                classA = new(Vector3.one);
                new SerializedObject(this).ApplyModifiedPropertiesWithoutUndo();
            }

        }
    }

    [CustomEditor(typeof(NestedSerializeTest))]
    public class NestedSerializeTestInspector : Editor
    {
        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            NestedSerializeTest test = target as NestedSerializeTest;
            return base.CreateInspectorGUI();
        }
    }
}