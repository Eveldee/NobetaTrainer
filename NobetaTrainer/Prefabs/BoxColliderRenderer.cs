using System.Collections.Generic;
using System.Linq;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Config.Models;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;
using UnityEngine;

namespace NobetaTrainer.Prefabs;

public class BoxColliderRenderer
{
    public ColliderType ColliderType { get; }

    private readonly GameObject _container;
    private readonly Vector3 _centerOffset;

    private readonly BoxColliderRendererConfig _rendererConfig;

    private readonly IEnumerable<LineRenderer> _lineRenderers;
    private readonly IEnumerable<MeshRenderer> _meshRenderers;

    private readonly Material sharedLineMaterial = new(Shader.Find("Sprites/Default"));

    // Points from bottom face to top face clockwise
    private readonly Vector3[] _points = new Vector3[8];

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
        _rendererConfig = rendererConfig;
        ColliderType = colliderType;

        // Parent is a global container for everything except others which are contained in their respective scenes,
        // this allow dynamically loading them to avoid lag
        var parent =
            ColliderType == ColliderType.Other && target.GetComponentInParent<SceneIsHide>()?.transform is { } scene
                ? scene
                : CollidersRenderPatches.RenderersContainer.transform;

        _container = new GameObject($"{name}_Container")
        {
            transform =
            {
                parent = parent,
                position = target.position,
                rotation = target.rotation,
                localScale = target.lossyScale
            }
        };
        _centerOffset = boxCollider.center;

        // Create surface and line renderers
        var xExtent = boxCollider.extents.x;
        var yExtent = boxCollider.extents.y;
        var zExtent = boxCollider.extents.z;

        // Points from bottom face to top face clockwise
        _points[0] = new Vector3(-xExtent, -yExtent, -zExtent);
        _points[1] = new Vector3(-xExtent, -yExtent, +zExtent);
        _points[2] = new Vector3(+xExtent, -yExtent, +zExtent);
        _points[3] = new Vector3(+xExtent, -yExtent, -zExtent);
        _points[4] = new Vector3(-xExtent, +yExtent, -zExtent);
        _points[5] = new Vector3(-xExtent, +yExtent, +zExtent);
        _points[6] = new Vector3(+xExtent, +yExtent, +zExtent);
        _points[7] = new Vector3(+xExtent, +yExtent, -zExtent);

        CreateColliderRenderer($"{name}_Down", new[] { _points[1], _points[0], _points[3], _points[2], _points[1] });
        CreateColliderRenderer($"{name}_Up", new[] { _points[0 + 4], _points[1 + 4], _points[2 + 4], _points[3 + 4], _points[0 + 4] });
        CreateColliderRenderer($"{name}_Front", new[] { _points[0], _points[0 + 4], _points[3 + 4], _points[3], _points[0] });
        CreateColliderRenderer($"{name}_Back", new[] { _points[2], _points[2 + 4], _points[1 + 4], _points[1], _points[2] });
        CreateColliderRenderer($"{name}_Left", new[] { _points[1], _points[1 + 4], _points[0 + 4], _points[0], _points[1] });
        CreateColliderRenderer($"{name}_Right", new[] { _points[3], _points[3 + 4], _points[2 + 4], _points[2], _points[3] });

        _lineRenderers = _container.GetComponentsInChildren<LineRenderer>();
        _meshRenderers = _container.GetComponentsInChildren<MeshRenderer>();

        _container.SetActive(IsActive());
    }

    public void UpdateDisplay()
    {
        var active = IsActive();
        _container.SetActive(active);

        foreach (var lineRenderer in _lineRenderers)
        {
            lineRenderer.enabled = active && _rendererConfig.DrawLines;
            lineRenderer.widthMultiplier = _rendererConfig.LineWidth;
            lineRenderer.startColor = _rendererConfig.LineStartColor.ToColor();
            lineRenderer.endColor = _rendererConfig.LineEndColor.ToColor();
        }

        foreach (var meshRenderer in _meshRenderers)
        {
            meshRenderer.enabled = active && _rendererConfig.DrawSurfaces;
            meshRenderer.material.color = _rendererConfig.SurfaceColor.ToColor();
        }
    }

    private bool IsActive()
    {
        return _rendererConfig.Enable;
    }

    public void Destroy()
    {
        Object.Destroy(_container);
    }

    private void CreateColliderRenderer(string name, Vector3[] meshPoints)
    {
        var gameObject = new GameObject(name)
        {
            transform =
            {
                parent = _container.transform,
                localPosition = _centerOffset,
                localRotation = Quaternion.identity
            }
        };

        // Create line renderer for borders
        var lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = sharedLineMaterial;

        lineRenderer.widthMultiplier = _rendererConfig.LineWidth;

        lineRenderer.startColor = _rendererConfig.LineStartColor.ToColor();
        lineRenderer.endColor = _rendererConfig.LineEndColor.ToColor();

        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = meshPoints.Length;
        lineRenderer.SetPositions(meshPoints);

        // Create mesh for solid surface
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            color = _rendererConfig.SurfaceColor.ToColor()
        };

        var meshFilter = gameObject.AddComponent<MeshFilter>();

        var mesh = new Mesh
        {
            vertices = meshPoints.SkipLast(1).ToArray(),
            triangles = _triangles,
            uv = _uv
        };

        meshFilter.mesh = mesh;

        // Activate object
        lineRenderer.enabled = _rendererConfig.DrawLines;
        meshRenderer.enabled = _rendererConfig.DrawSurfaces;
        gameObject.SetActive(true);
    }
}