using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MinMaxSliderAttribute attr = (MinMaxSliderAttribute)attribute;

        if (property.propertyType == SerializedPropertyType.Vector2)
        {
            Vector2 range = property.vector2Value;
            float minVal = range.x;
            float maxVal = range.y;

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Calculate rects for values and slider
            float valueWidth = 45f;
            float spacing = 5f;
            float sliderWidth = position.width - (valueWidth * 2) - (spacing * 2);

            Rect minRect = new Rect(position.x, position.y, valueWidth, position.height);
            Rect sliderRect = new Rect(position.x + valueWidth + spacing, position.y, sliderWidth, position.height);
            Rect maxRect = new Rect(position.x + valueWidth + spacing + sliderWidth + spacing, position.y, valueWidth, position.height);

            // Draw min value input
            minVal = EditorGUI.FloatField(minRect, minVal);

            // Draw min-max slider
            EditorGUI.MinMaxSlider(sliderRect, ref minVal, ref maxVal, attr.min, attr.max);

            // Draw max value input
            maxVal = EditorGUI.FloatField(maxRect, maxVal);

            // Clamp values
            if (minVal < attr.min) minVal = attr.min;
            if (maxVal > attr.max) maxVal = attr.max;
            if (minVal > maxVal) minVal = maxVal;

            // Round values to 2 decimals for readability
            minVal = Mathf.Round(minVal * 100f) / 100f;
            maxVal = Mathf.Round(maxVal * 100f) / 100f;

            property.vector2Value = new Vector2(minVal, maxVal);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use only with Vector2");
        }
    }
}
