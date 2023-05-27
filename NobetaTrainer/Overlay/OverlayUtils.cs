﻿using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using EnumsNET;
using Humanizer;
using ImGuiNET;
using NobetaTrainer.Utils;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer.Overlay;

public partial class TrainerOverlay
{
    private static void ShowValue(string title, object value, string format = null, string help = null)
    {
        ImGui.Text(title);
        ImGui.SameLine();

        if (format is null)
        {
            ImGui.TextColored(ValueColor, string.Format(CultureInfo.InvariantCulture, "{0}", value));
        }
        else
        {
            ImGui.TextColored(ValueColor, string.Format(CultureInfo.InvariantCulture, $"{{0:{format}}}", value));
        }

        if (help is not null)
        {
            HelpMarker(help);
        }
    }

     private static void ShowValueExpression(object value, string format = null, string help = null, [CallerArgumentExpression(nameof(value))] string valueExpression = default)
    {
        // TODO Replace by a compiled Regex
        // Remove .Get .Is and g_[b|f]
        valueExpression = valueExpression!.Replace(".Is", ".");
        valueExpression = valueExpression!.Replace(".GetIs", ".");
        valueExpression = valueExpression!.Replace(".Get", ".");
        valueExpression = valueExpression!.Replace(".g_f", ".");
        valueExpression = valueExpression!.Replace(".g_b", ".");
        valueExpression = valueExpression!.Replace(".g_", ".");
        valueExpression = valueExpression!.Replace("Null", "Arcane");

        if (valueExpression!.EndsWith("Format()"))
        {
            valueExpression = valueExpression[..valueExpression.LastIndexOf('.')];
            ShowValue($"{valueExpression![(valueExpression.LastIndexOf('.')+1)..].Humanize(LetterCasing.Title)}:", value, format, help);
        }
        else
        {
            ShowValue($"{valueExpression![(valueExpression.LastIndexOf('.')+1)..].Humanize(LetterCasing.Title)}:", value, format, help);
        }
    }

     private static bool ShowValueModifiable(string propertyName, object instance, string title = null, string format = null, string help = null)
     {
         return ShowValueModifiable(instance.GetType().GetProperty(propertyName), instance, title, format, help);
     }
     private static bool ShowValueModifiable(PropertyInfo propertyInfo, object instance, string title = null, string format = null, string help = null)
     {
         var valueReference = propertyInfo.GetValue(instance);

         if (ShowValueModifiable(title ?? propertyInfo.Name.Humanize(LetterCasing.Title), ref valueReference, format, help))
         {
            propertyInfo.SetValue(instance, valueReference);

            return true;
         }

         return false;
     }
    private static bool ShowValueModifiable(string title, ref object value, string format = null, string help = null)
    {
        bool TextReturn(string label)
        {
            ImGui.Text(label);

            return false;
        }
        bool ModifyBool(string label, ref object target)
        {
            bool value = (bool)target;
            if (ImGui.Checkbox(label, ref value))
            {
                target = value;

                return true;
            }

            return false;
        }
        bool ModifyInt(string label, ref object target)
        {
            int value = (int)target;
            if (ImGui.DragInt(label, ref value))
            {
                target = value;

                return true;
            }

            return false;
        }
        bool ModifyFloat(string label, ref object target)
        {
            float value = (float)target;
            if (ImGui.DragFloat(label, ref value))
            {
                target = value;

                return true;
            }

            return false;
        }
        bool ModifyString(string label, ref object target)
        {
            string value = (string)target;
            if (ImGui.InputText(label, ref value, 50))
            {
                target = value;

                return true;
            }

            return false;
        }
        bool ModifyVector3(string label, ref object target)
        {
            Vector3 vector3 = (Vector3)target;

            System.Numerics.Vector3 wrapper = new System.Numerics.Vector3(vector3.x, vector3.y, vector3.z);
            if (ImGui.DragFloat3(label, ref wrapper))
            {
                target = new Vector3(wrapper.X, wrapper.Y, wrapper.Z);

                return true;
            }

            return false;
        }
        bool ModifyQuaternion(string label, ref object target)
        {
            Quaternion quaternion = (Quaternion)target;
            var eulerAngles = quaternion.eulerAngles;

            System.Numerics.Vector3 wrapper = new System.Numerics.Vector3(eulerAngles.x, eulerAngles.y, eulerAngles.z);
            if (ImGui.DragFloat3(label, ref wrapper))
            {
                target = UnityEngine.Quaternion.Euler(wrapper.X, wrapper.Y, wrapper.Z);

                return true;
            }

            return false;
        }
        bool ModifyEnum(string label, ref object target)
        {
            int valueIndex = (int) target;
            var possibleValues = Enums.GetNames(target.GetType()).ToArray();

            if (ImGui.Combo(label, ref valueIndex, possibleValues, possibleValues.Length))
            {
                target = valueIndex;

                return true;
            }

            return false;
        }

        var result = value switch
        {
            bool => ModifyBool($"##{title}", ref value),
            int => ModifyInt($"##{title}", ref value),
            float => ModifyFloat($"##{title}", ref value),
            string => ModifyString($"##{title}", ref value),
            Vector3 => ModifyVector3($"##{title}", ref value),
            Quaternion => ModifyQuaternion($"##{title}", ref value),
            Enum => ModifyEnum($"##{title}", ref value),
            _ => TextReturn($"Not modifiable type: {value.GetType()}")
        };

        ImGui.SameLine();
        ImGui.Text(title);

        if (help is not null)
        {
            HelpMarker(help);
        }

        return result;
    }

    private static void HelpMarker(string description, bool sameLine = true)
    {
        if (sameLine)
        {
            ImGui.SameLine();
        }

        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort) && ImGui.BeginTooltip())
        {
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(description);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    private bool ButtonColored(Vector4 color, string label, float gradientStep = 0.1f)
    {
        var gradient = color.IntensityGradient(gradientStep, 3);

        ImGui.PushStyleColor(ImGuiCol.Button, gradient[0]);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, gradient[1]);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, gradient[2]);

        var result = ImGui.Button(label);

        ImGui.PopStyleColor(3);

        return result;
    }

    private void TreeNode(string label, Action whenOpened)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, InfoColor);
        if (ImGui.TreeNode(label))
        {
            ImGui.PopStyleColor();

            whenOpened();

            ImGui.TreePop();
        }
        else
        {
            ImGui.PopStyleColor();
        }
    }
}