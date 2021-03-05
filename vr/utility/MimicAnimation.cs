using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MimicAnimation : MonoBehaviour
{
    public AnimationClip targetClip;
    public int targetFrame = 0;

    Dictionary<string, TransformProperties> bindingPropertyMap;
    Dictionary<string, Transform> bindingTransformMap;

    class TransformProperties
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        public float RotationW { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }

        public TransformProperties(Vector3 position, Vector4 rotation, Vector3 scale)
        {
            PositionX = position.x;
            PositionY = position.x;
            PositionZ = position.x;

            RotationX = rotation.x;
            RotationY = rotation.y;
            RotationZ = rotation.z;
            RotationW = rotation.w;

            ScaleX = scale.x;
            ScaleY = scale.y;
            ScaleZ = scale.z;
        }
    }

    [ContextMenu("MimicTransform")]
    public void MimicTransform()
    {
        bindingPropertyMap = new Dictionary<string, TransformProperties>();
        bindingTransformMap = new Dictionary<string, Transform>();

        // Build up the maps
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(targetClip);
        foreach (EditorCurveBinding binding in bindings)
        {
            // Initialize
            if (!bindingTransformMap.ContainsKey(binding.path))
            {
                Transform targetTransform = GameObject.Find(name + "/" + binding.path).transform;
                bindingTransformMap.Add(binding.path, targetTransform);

                bindingPropertyMap.Add(binding.path, 
                    new TransformProperties(
                        targetTransform.localPosition,
                        new Vector4(targetTransform.localRotation.x, targetTransform.localRotation.y, targetTransform.localRotation.z, targetTransform.localRotation.w),
                        targetTransform.localScale
                    )
                );
            }

            AnimationCurve curve = AnimationUtility.GetEditorCurve(targetClip, binding);
            float val = curve[targetFrame].value;

            //Debug.Log(binding.path + " : " + binding.propertyName + " = " + val.ToString());
            string propertyName = binding.propertyName;
            switch (propertyName)
            {
                // Position
                case "m_LocalPosition.x":
                    bindingPropertyMap[binding.path].PositionX = val;
                    break;
                case "m_LocalPosition.y":
                    bindingPropertyMap[binding.path].PositionY = val;
                    break;
                case "m_LocalPosition.z":
                    bindingPropertyMap[binding.path].PositionZ = val;
                    break;
                // Rotation
                case "m_LocalRotation.x":
                    bindingPropertyMap[binding.path].RotationX = val;
                    break;
                case "m_LocalRotation.y":
                    bindingPropertyMap[binding.path].RotationY = val;
                    break;
                case "m_LocalRotation.z":
                    bindingPropertyMap[binding.path].RotationZ = val;
                    break;
                case "m_LocalRotation.w":
                    bindingPropertyMap[binding.path].RotationW = val;
                    break;
                // Scale
                case "m_LocalScale.x":
                    bindingPropertyMap[binding.path].ScaleX = val;
                    break;
                case "m_LocalScale.y":
                    bindingPropertyMap[binding.path].ScaleY = val;
                    break;
                case "m_LocalScale.z":
                    bindingPropertyMap[binding.path].ScaleZ = val;
                    break;
            }
        }
        
        foreach (KeyValuePair<string, Transform> bindingTransformPair in bindingTransformMap)
        {
            string bindingPath = bindingTransformPair.Key;
            Transform bindingTransform = bindingTransformPair.Value;

            TransformProperties props = bindingPropertyMap[bindingPath];
            bindingTransform.localPosition = new Vector3(props.PositionX, props.PositionY, props.PositionZ);
            bindingTransform.localRotation = new Quaternion(props.RotationX, props.RotationY, props.RotationZ, props.RotationW);
            bindingTransform.localScale = new Vector3(props.ScaleX, props.ScaleY, props.ScaleZ);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
