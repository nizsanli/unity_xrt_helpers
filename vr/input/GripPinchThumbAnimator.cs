using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.InputSystem;
using System;

public class GripPinchThumbAnimator : MonoBehaviour
{
    [Serializable]
    public class FloatInputAction
    {
        public InputActionProperty actionProperty;
        public float defaultValue;
        public float Value { get; set; }

        public FloatInputAction(InputActionProperty actionProperty, float defaultValue)
        {
            this.actionProperty = actionProperty;
            this.defaultValue = defaultValue;
        }

        public void Enable()
        {
            if (actionProperty != null)
            {
                actionProperty.action.Enable();
            }
        }

        public void UpdateValue()
        {
            Value = actionProperty != null ? actionProperty.action.ReadValue<float>() : defaultValue;
        }
    }

    public FloatInputAction grip;
    public FloatInputAction pinch;
    public FloatInputAction triggerTouch;
    public FloatInputAction thumbTouch;

    public Animator handAnimator;

    public string animLayerNameBase = "Base Layer";
    public string animLayerNamePoint = "Point Layer";
    public string animLayerNameThumb = "Thumb Layer";

    public string animParamNameGrip = "Flex";
    public string animParamNamePinch = "Pinch";
    public string animParamNamePose = "Pose";

    public float inputMoveSpeed = 13f;

    private int animLayerIndexBase = -1;
    private int animLayerIndexThumb = -1;
    private int animLayerIndexPoint = -1;

    private float gripBlend = 0f;
    private float pinchBlend = 0f;
    private float pointBlend = 1f;
    private float thumbUpBlend = 1f;

    // Start is called before the first frame update
    void Start()
    {
        // Monitor controls
        EnableActions();

        // Get animator layer indices by name, for later use switching between hand visuals
        animLayerIndexBase = handAnimator.GetLayerIndex(animLayerNameBase);
        animLayerIndexPoint = handAnimator.GetLayerIndex(animLayerNamePoint);
        animLayerIndexThumb = handAnimator.GetLayerIndex(animLayerNameThumb);
    }

    // Update is called once per frame
    void Update()
    {
        // Query input from controls
        UpdateActionValues();

        // Calculate blend values for animation
        UpdateBlendValues();

        // Use blend values
        UpdateAnimator();
    }

    void EnableActions()
    {
        grip.Enable();
        pinch.Enable();
        triggerTouch.Enable();
        thumbTouch.Enable();
    }

    void UpdateActionValues()
    {
        grip.UpdateValue();
        pinch.UpdateValue();
        triggerTouch.UpdateValue();
        thumbTouch.UpdateValue();
    }

    void UpdateBlendValues()
    {
        // Interpolate dependent on frame rate
        float interpolationFactor = inputMoveSpeed * Time.deltaTime;

        // Update Base Layer values
        InterpolateReferenceToTarget(ref gripBlend, grip.Value, interpolationFactor);
        InterpolateReferenceToTarget(ref pinchBlend, pinch.Value, interpolationFactor);

        // Update Point Layer values
        float indexFingerUpValue = -(triggerTouch.Value - 1f);
        float targetPointBlend = indexFingerUpValue;
        if (indexFingerUpValue == 0)
        {
            float touchPointBaseAmount = .55f;
            targetPointBlend = touchPointBaseAmount * (1f - pinch.Value);
        }
        InterpolateReferenceToTarget(ref pointBlend, targetPointBlend, interpolationFactor);

        // Update Thumb Layer values
        float thumbUpValue = -(thumbTouch.Value - 1f);
        float thumbUpNerf = .45f;
        float targetThumbUpBlend = thumbUpValue * thumbUpNerf * (1f - grip.Value);
        if (thumbUpValue > 0)
        {
            targetThumbUpBlend = thumbUpNerf + (1f - thumbUpNerf) * grip.Value;
        }
        InterpolateReferenceToTarget(ref thumbUpBlend, targetThumbUpBlend, interpolationFactor);
    }

    void UpdateAnimator()
    {
        // Set param values
        handAnimator.SetFloat(animParamNameGrip, gripBlend);
        handAnimator.SetFloat(animParamNamePinch, pinchBlend);

        // Set layer values
        handAnimator.SetLayerWeight(animLayerIndexPoint, pointBlend);
        handAnimator.SetLayerWeight(animLayerIndexThumb, thumbUpBlend);
    }

    public void InterpolateReferenceToTarget(ref float current, float target, float t)
    {
        current = Mathf.Lerp(current, target, t);
    }
}
