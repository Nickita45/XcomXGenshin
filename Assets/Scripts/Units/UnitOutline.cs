using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitOutline : MonoBehaviour
{
    // We need two outlines for the weapon-wielding units:
    // one for the model, and one for the gun.
    //
    // We cannot use a single outline here because of the implementation
    // details of the Outline class.
    private Outline _modelOutline, _gunOutline;

    [SerializeField]
    private OutlineColor outlineColor;

    public void Init(GameObject modelObject)
    {
        (Color color, float width) = GetParameters();
        _modelOutline = ObjectUtils.AddOutline(modelObject, color, width);
    }

    public void Init(GameObject modelObject, GameObject gunObject)
    {
        (Color color, float width) = GetParameters();
        _modelOutline = ObjectUtils.AddOutline(modelObject, color, width);
        _gunOutline = ObjectUtils.AddOutline(gunObject, color, width);
    }

    private (Color, float) GetParameters()
    {
        string hexColor = "";
        switch (outlineColor)
        {
            case OutlineColor.Character:
                hexColor = ConfigurationManager.GlobalDataJson.outlineCharacterColor;
                break;
            case OutlineColor.Enemy:
                hexColor = ConfigurationManager.GlobalDataJson.outlineEnemyColor;
                break;
        }

        Color color;
        ColorUtility.TryParseHtmlString(hexColor, out color);
        float width = ConfigurationManager.GlobalDataJson.outlineWidth;

        return (color, width);
    }

    private Color GetTargetColor()
    {
        string hexColor = "";
        switch (outlineColor)
        {
            case OutlineColor.Character:
                hexColor = ConfigurationManager.GlobalDataJson.outlineCharacterTargetColor;
                break;
            case OutlineColor.Enemy:
                hexColor = ConfigurationManager.GlobalDataJson.outlineEnemyTargetColor;
                break;
        }

        Color color;
        ColorUtility.TryParseHtmlString(hexColor, out color);
        return color;
    }

    public void SetOutline(bool enabled)
    {
        _modelOutline.enabled = enabled;
        if (_gunOutline) _gunOutline.enabled = enabled;
    }

    public void SetTargetColor()
    {
        Color color = GetTargetColor();
        _modelOutline.OutlineColor = color;
        if (_gunOutline) _gunOutline.OutlineColor = color;
    }

    public void SetNormalColor()
    {
        (Color color, _) = GetParameters();
        _modelOutline.OutlineColor = color;
        if (_gunOutline) _gunOutline.OutlineColor = color;
    }
}

public enum OutlineColor
{
    Character,
    Enemy
}