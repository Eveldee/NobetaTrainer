using System.Collections.Generic;
using UnityEngine;

namespace NobetaTrainer.Prefabs;

public class BoxColliderRenderer
{
    private readonly Transform _parent;

    private readonly GameObject _downFace;
    private readonly GameObject _upFace;

    private readonly GameObject _frontFace;
    private readonly GameObject _backFace;

    private readonly GameObject _leftFace;
    private readonly GameObject _rightFace;

    public BoxColliderRenderer(string name, Transform parent, BoxCollider boxCollider)
    {
        _parent = parent;

        var center = parent.position + boxCollider.center;
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
        var downPoints = new Vector3[]
        {
            new(xMin, yMin, zMax),
            new(xMin, yMin, zMin),
            new(xMax, yMin, zMin),
            new(xMax, yMin, zMax),
            new(xMin, yMin, zMax)
        };
        // Up
        var upPoints = new Vector3[]
        {
            new(xMin, yMax, zMin),
            new(xMin, yMax, zMax),
            new(xMax, yMax, zMax),
            new(xMax, yMax, zMin),
            new(xMin, yMax, zMin)
        };
        // Front
        var frontPoints = new Vector3[]
        {
            new(xMin, yMin, zMin),
            new(xMin, yMax, zMin),
            new(xMax, yMax, zMin),
            new(xMax, yMin, zMin),
            new(xMin, yMin, zMin)
        };
        // Back
        var backPoints = new Vector3[]
        {
            new(xMax, yMin, zMax),
            new(xMax, yMax, zMax),
            new(xMin, yMax, zMax),
            new(xMin, yMin, zMax),
            new(xMax, yMin, zMax)
        };
        // Left
        var leftPoints = new Vector3[]
        {
            new(xMin, yMin, zMax),
            new(xMin, yMax, zMax),
            new(xMin, yMax, zMin),
            new(xMin, yMin, zMin),
            new(xMin, yMin, zMax)
        };
        // Right
        var rightPoints = new Vector3[]
        {
            new(xMax, yMin, zMin),
            new(xMax, yMax, zMin),
            new(xMax, yMax, zMax),
            new(xMax, yMin, zMax),
            new(xMax, yMin, zMin)
        };

        _downFace = CreateColliderRenderer($"{name}_Down", downPoints);
        _upFace = CreateColliderRenderer($"{name}_Up", upPoints);
        _frontFace = CreateColliderRenderer($"{name}_Front", frontPoints);
        _backFace = CreateColliderRenderer($"{name}_Back", backPoints);
        _leftFace = CreateColliderRenderer($"{name}_Left", leftPoints);
        _rightFace = CreateColliderRenderer($"{name}_Right", rightPoints);
    }

    public void SetVisible(bool drawLines)
    {
        _downFace.active = drawLines;
        _upFace.active = drawLines;
        _frontFace.active = drawLines;
        _backFace.active = drawLines;
        _leftFace.active = drawLines;
        _rightFace.active = drawLines;
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
                parent = _parent
            }
        };

        // Create one line render per linePoints
        var lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.2f;

        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.red;

        lineRenderer.positionCount = linePoints.Length;
        lineRenderer.SetPositions(linePoints);

        // TODO mesh renderer + mesh filter + mesh https://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html

        // Activate object
        gameObject.SetActive(true);

        return gameObject;
    }
}