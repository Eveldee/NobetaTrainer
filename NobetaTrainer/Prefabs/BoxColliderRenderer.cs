using System.Collections.Generic;
using System.Linq;
using NobetaTrainer.Config.Models;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;
using UnityEngine;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer.Prefabs;

public class BoxColliderRenderer
{
    public ColliderType ColliderType { get; }

    private readonly Transform _parent;

    private readonly GameObject _downFace;
    private readonly GameObject _upFace;

    private readonly GameObject _frontFace;
    private readonly GameObject _backFace;

    private readonly GameObject _leftFace;
    private readonly GameObject _rightFace;

    private readonly BoxColliderRendererConfig _rendererConfig;

    private readonly IEnumerable<GameObject> _faces;
    private readonly IEnumerable<LineRenderer> _lineRenderers;

    public BoxColliderRenderer(string name, Transform parent, BoxCollider boxCollider, BoxColliderRendererConfig rendererConfig, ColliderType colliderType)
    {
        Vector3 OffsetPointWithRotation(Vector3 point)
        {
            return (parent.rotation * point) + parent.position;
        }

        _parent = parent;
        _rendererConfig = rendererConfig;
        ColliderType = colliderType;

        var center = boxCollider.center;
        var xExtent = boxCollider.extents.x;
        var yExtent = boxCollider.extents.y;
        var zExtent = boxCollider.extents.z;

        var xMin = center.x - xExtent;
        var xMax = center.x + xExtent;
        var yMin = center.y - yExtent;
        var yMax = center.y + yExtent;
        var zMin = center.z - zExtent;
        var zMax = center.z + zExtent;

        // Faces clockwise from down to sides to up
        var rotation = parent.rotation;

        // Down
        var downPoints = new[]
        {
            OffsetPointWithRotation(new Vector3(xMin, yMin, zMax)),
            OffsetPointWithRotation(new Vector3(xMin, yMin, zMin)),
            OffsetPointWithRotation(new Vector3(xMax, yMin, zMin)),
            OffsetPointWithRotation(new Vector3(xMax, yMin, zMax)),
            OffsetPointWithRotation(new Vector3(xMin, yMin, zMax))
        };
        // Up
        var upPoints = new[]
        {
            OffsetPointWithRotation(new Vector3(xMin, yMax, zMin)),
            OffsetPointWithRotation(new Vector3(xMin, yMax, zMax)),
            OffsetPointWithRotation(new Vector3(xMax, yMax, zMax)),
            OffsetPointWithRotation(new Vector3(xMax, yMax, zMin)),
            OffsetPointWithRotation(new Vector3(xMin, yMax, zMin))
        };
        // Front
        var frontPoints = new[]
        {
            OffsetPointWithRotation(new Vector3(xMin, yMin, zMin)),
            OffsetPointWithRotation(new Vector3(xMin, yMax, zMin)),
            OffsetPointWithRotation(new Vector3(xMax, yMax, zMin)),
            OffsetPointWithRotation(new Vector3(xMax, yMin, zMin)),
            OffsetPointWithRotation(new Vector3(xMin, yMin, zMin))
        };
        // Back
        var backPoints = new[]
        {
            OffsetPointWithRotation(new Vector3(xMax, yMin, zMax)),
            OffsetPointWithRotation(new Vector3(xMax, yMax, zMax)),
            OffsetPointWithRotation(new Vector3(xMin, yMax, zMax)),
            OffsetPointWithRotation(new Vector3(xMin, yMin, zMax)),
            OffsetPointWithRotation(new Vector3(xMax, yMin, zMax))
        };
        // Left
        var leftPoints = new[]
        {
            OffsetPointWithRotation(new Vector3(xMin, yMin, zMax)),
            OffsetPointWithRotation(new Vector3(xMin, yMax, zMax)),
            OffsetPointWithRotation(new Vector3(xMin, yMax, zMin)),
            OffsetPointWithRotation(new Vector3(xMin, yMin, zMin)),
            OffsetPointWithRotation(new Vector3(xMin, yMin, zMax))
        };
        // Right
        var rightPoints = new[]
        {
            OffsetPointWithRotation(new Vector3(xMax, yMin, zMin)),
            OffsetPointWithRotation(new Vector3(xMax, yMax, zMin)),
            OffsetPointWithRotation(new Vector3(xMax, yMax, zMax)),
            OffsetPointWithRotation(new Vector3(xMax, yMin, zMax)),
            OffsetPointWithRotation(new Vector3(xMax, yMin, zMin))
        };

        _downFace = CreateColliderRenderer($"{name}_Down", downPoints);
        _upFace = CreateColliderRenderer($"{name}_Up", upPoints);
        _frontFace = CreateColliderRenderer($"{name}_Front", frontPoints);
        _backFace = CreateColliderRenderer($"{name}_Back", backPoints);
        _leftFace = CreateColliderRenderer($"{name}_Left", leftPoints);
        _rightFace = CreateColliderRenderer($"{name}_Right", rightPoints);

        _faces = new[] { _downFace, _upFace, _frontFace, _backFace, _leftFace, _rightFace };
        _lineRenderers = _faces.Select(face => face.GetComponent<LineRenderer>()).ToArray();
    }

    public void UpdateDisplay()
    {
        foreach (var face in _faces)
        {
            face.SetActive(_rendererConfig.Enable);
        }

        foreach (var lineRenderer in _lineRenderers)
        {
            lineRenderer.enabled = _rendererConfig.DrawLines;
            lineRenderer.widthMultiplier = _rendererConfig.LineWidth;
            lineRenderer.startColor = _rendererConfig.LineStartColor.ToColor();
            lineRenderer.endColor = _rendererConfig.LineEndColor.ToColor();
        }
    }

    public void Destroy()
    {
        Object.Destroy(_downFace);
        Object.Destroy(_upFace);
        Object.Destroy(_frontFace);
        Object.Destroy(_backFace);
        Object.Destroy(_leftFace);
        Object.Destroy(_rightFace);
    }

    private GameObject CreateColliderRenderer(string name, Vector3[] linePoints)
    {
        var gameObject = new GameObject(name)
        {
            transform =
            {
                parent = CollidersRenderPatches.RenderersContainer.transform
            }
        };

        // Create one line render per linePoints
        var lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = _rendererConfig.LineWidth;

        lineRenderer.startColor = _rendererConfig.LineStartColor.ToColor();
        lineRenderer.endColor = _rendererConfig.LineEndColor.ToColor();

        lineRenderer.positionCount = linePoints.Length;
        lineRenderer.SetPositions(linePoints);

        // TODO mesh renderer + mesh filter + mesh https://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html

        // Activate object
        gameObject.SetActive(_rendererConfig.Enable);

        return gameObject;
    }
}