using System.Collections.Generic;
using System.Linq;
using NobetaTrainer.Config.Models;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;
using UnityEngine;

namespace NobetaTrainer.Prefabs;

public class BoxColliderRenderer
{
    public ColliderType ColliderType { get; }

    private readonly GameObject _parent;

    private readonly GameObject _downFace;
    private readonly GameObject _upFace;

    private readonly GameObject _frontFace;
    private readonly GameObject _backFace;

    private readonly GameObject _leftFace;
    private readonly GameObject _rightFace;

    private readonly BoxColliderRendererConfig _rendererConfig;

    private readonly IEnumerable<GameObject> _faces;
    private readonly IEnumerable<LineRenderer> _lineRenderers;
    private readonly IEnumerable<MeshRenderer> _meshRenderers;

    private readonly Material sharedLineMaterial = new(Shader.Find("Sprites/Default"));

    private readonly int[] _triangles =
    {
        // bottom left triangle
        0, 1, 3,
        // upper right triangle
        1, 2, 3
    };
    private readonly Vector2[] _uv =
    {
        new(0, 0),
        new (1, 0),
        new (0, 1),
        new (1, 1)
    };

    public BoxColliderRenderer(string name, Transform target, BoxCollider boxCollider, BoxColliderRendererConfig rendererConfig, ColliderType colliderType)
    {
        Vector3 OffsetPointWithRotation(Vector3 point)
        {
            return (target.rotation * point) + target.position;
        }

        _rendererConfig = rendererConfig;
        ColliderType = colliderType;

        // Parent is a global container for everything except others which are contained in their respective scenes,
        // this allow dynamically loading them to avoid lag
        _parent = CollidersRenderPatches.RenderersContainer;
        if (ColliderType == ColliderType.Other && target.GetComponentInParent<SceneIsHide>()?.transform is { } parent)
        {
            _parent = parent.gameObject;
        }

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
        _meshRenderers = _faces.Select(face => face.GetComponent<MeshRenderer>()).ToArray();
    }

    public void UpdateDisplay()
    {
        foreach (var face in _faces)
        {
            face.SetActive(IsActive());
        }

        foreach (var lineRenderer in _lineRenderers)
        {
            lineRenderer.enabled = _rendererConfig.DrawLines;
            lineRenderer.widthMultiplier = _rendererConfig.LineWidth;
            lineRenderer.startColor = _rendererConfig.LineStartColor.ToColor();
            lineRenderer.endColor = _rendererConfig.LineEndColor.ToColor();
        }

        foreach (var meshRenderer in _meshRenderers)
        {
            meshRenderer.enabled = _rendererConfig.DrawSurfaces;
            meshRenderer.material.color = _rendererConfig.SurfaceColor.ToColor();
        }
    }

    private bool IsActive()
    {
        // If it's an object in a scene it's active if the scene is active
        if (ColliderType == ColliderType.Other && _parent.GetComponent<SceneIsHide>() is { } sceneIsHide)
        {
            return !sceneIsHide.g_bIsHide && CollidersRenderPatches.ShowColliders && _rendererConfig.Enable;
        }

        return _parent.active && _rendererConfig.Enable;
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
                parent = _parent.transform
            }
        };

        // Create line renderer for borders
        var lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = sharedLineMaterial;

        lineRenderer.widthMultiplier = _rendererConfig.LineWidth;

        lineRenderer.startColor = _rendererConfig.LineStartColor.ToColor();
        lineRenderer.endColor = _rendererConfig.LineEndColor.ToColor();

        lineRenderer.positionCount = linePoints.Length;
        lineRenderer.SetPositions(linePoints);

        // Create mesh for solid surface
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            color = _rendererConfig.SurfaceColor.ToColor()
        };

        var meshFilter = gameObject.AddComponent<MeshFilter>();

        var mesh = new Mesh
        {
            vertices = linePoints.SkipLast(1).ToArray(),
            triangles = _triangles,
            uv = _uv
        };

        meshFilter.mesh = mesh;

        // Activate object
        lineRenderer.enabled = _rendererConfig.DrawLines;
        meshRenderer.enabled = _rendererConfig.DrawSurfaces;
        gameObject.SetActive(IsActive());

        return gameObject;
    }
}