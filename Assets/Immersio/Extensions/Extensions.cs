using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


public static class Extensions
{

    // Transform
    public static void SetX(this Transform t, float x)
    {
        t.position = new Vector3(x, t.position.y, t.position.z);
    }
    public static void SetY(this Transform t, float y)
    {
        t.position = new Vector3(t.position.x, y, t.position.z);
    }
    public static void SetZ(this Transform t, float z)
    {
        t.position = new Vector3(t.position.x, t.position.y, z);
    }

    public static void SetLocalX(this Transform t, float x)
    {
        t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
    }
    public static void SetLocalY(this Transform t, float y)
    {
        t.localPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);
    }
    public static void SetLocalZ(this Transform t, float z)
    {
        t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, z);
    }

    public static void AddToX(this Transform t, float amount)
    {
        Vector3 p = t.position;
        t.position = new Vector3(p.x + amount, p.y, p.z);
    }
    public static void AddToY(this Transform t, float amount)
    {
        Vector3 p = t.position;
        t.position = new Vector3(p.x, p.y + amount, p.z);
    }
    public static void AddToZ(this Transform t, float amount)
    {
        Vector3 p = t.position;
        t.position = new Vector3(p.x, p.y, p.z + amount);
    }

    public static void AddToLocalX(this Transform t, float amount)
    {
        Vector3 p = t.localPosition;
        t.localPosition = new Vector3(p.x + amount, p.y, p.z);
    }
    public static void AddToLocalY(this Transform t, float amount)
    {
        Vector3 p = t.localPosition;
        t.localPosition = new Vector3(p.x, p.y + amount, p.z);
    }
    public static void AddToLocalZ(this Transform t, float amount)
    {
        Vector3 p = t.localPosition;
        t.localPosition = new Vector3(p.x, p.y, p.z + amount);
    }
    

    public static void InsertNewParentAtPosition(this Transform t, Vector3 newPos_World)
    {
        Vector3 s = t.localScale;
        Vector3 offset = t.position - newPos_World;

        GameObject newParent = new GameObject(t.gameObject.name + "_PARENT");
        Transform parentT = newParent.transform;
        parentT.parent = t.parent;
        parentT.position = newPos_World;
        parentT.localScale = s;

        t.parent = parentT;
        t.position = newPos_World + offset;
        t.localScale = s;
    }

    // Removes t's parent Transform from the hierarchy and 
    //  assigns t's grandparent as the new parent for t and all of t's siblings
    public static Transform RemoveParent(this Transform t)
    {
        Transform tParent = t.parent;
        if (tParent == null)
            return null;

        Transform newParent = tParent.parent;
        Vector3 parentScale = tParent.localScale;

        Transform[] siblings = tParent.Cast<Transform>().ToArray();
        for (int i = 0; i < siblings.Length; i++)
        {
            Transform sibling = siblings[i];
            Vector3 p = sibling.position;
            sibling.parent = newParent;
            sibling.position = p;
            sibling.localScale = parentScale;
        }

        tParent.parent = null;

        return tParent;
    }


    // GameObject
    public static Dictionary<string, GameObject> GetChildren(this GameObject g, Dictionary<string, GameObject> dict = null, bool recursive = true, bool includeChildLeavesOnly = false)
    {
        if (dict == null)
            dict = new Dictionary<string, GameObject>();

        if (!recursive)
            includeChildLeavesOnly = false;		// this option is only valid for recursive searches

        foreach (Transform child in g.transform)
        {
            GameObject childGameObject = child.gameObject;

            bool childIsALeaf = child.childCount == 0;
            if (childIsALeaf || !includeChildLeavesOnly)
            {
                try
                {
                    dict.Add(childGameObject.name, childGameObject);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.Log("childGameObject.name: " + childGameObject.name);
                }
            }

            if (recursive && !childIsALeaf)
                childGameObject.GetChildren(dict, true, includeChildLeavesOnly);
        }

        return dict;
    }
    public static void PrintChildren(this GameObject g, bool recursive = true, bool includeChildLeavesOnly = false)
    {
        int count = 0;
        foreach (string childName in g.GetChildren(null, recursive, includeChildLeavesOnly).Keys)
        {
            Debug.Log(childName);
            count++;
        }
        Debug.Log(count);
    }
    public static T GetSafeComponent<T>(this GameObject obj) where T : MonoBehaviour
    {
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError("GetSafeComponent --> Expected to find component of type " + typeof(T) + " but found none", obj);
        }
        return component;
    }

    // Camera
    public static Vector3 LocalToScreenPoint(this Camera camera, Vector3 point, Transform localTransform)
    {
        return camera.WorldToScreenPoint(localTransform.TransformPoint(point));
    }

    // Color
    public static Color Color255(int r, int g, int b, int a)
    {
        float f = 1 / 255.0f;
        return new Color(r * f, g * f, b * f, a * f);
    }
    public static void SetR_255(this Color color, int r)
    {
        color = new Color(r / 255.0f, color.g, color.b, color.a);
    }
    public static void SetG_255(this Color color, int g)
    {
        color = new Color(color.r, g / 255.0f, color.b, color.a);
    }
    public static void SetB_255(this Color color, int b)
    {
        color = new Color(color.r, color.g, b / 255.0f, color.a);
    }
    public static void SetA_255(this Color color, int a)
    {
        color = new Color(color.r, color.g, color.b, a / 255.0f);
    }

    // Vector3
    public static Vector3 SnappedToGrid(this Vector3 pt, Vector3 increment, Vector3 offset)
    {
        Vector3 p = pt;
        if (increment.x > 0) { p.x = (((int)((p.x - offset.x) / increment.x)) * increment.x) + offset.x; }
        if (increment.y > 0) { p.y = (((int)((p.y - offset.y) / increment.y)) * increment.y) + offset.y; }
        if (increment.z > 0) { p.z = (((int)((p.z - offset.z) / increment.z)) * increment.z) + offset.z; }
        return p;
    }
    public static Vector3 SnappedToGrid(this Vector3 pt, Vector3 increment)
    {
        return (pt.SnappedToGrid(increment, Vector3.zero));
    }

    // Vector2
    public static Vector2 GetNearestNormalizedAxisDirection(this Vector2 dir)
    {
        Vector2 unitDirection = dir;

        if (dir.x == 0 && dir.y == 0) 
        {
            unitDirection = Vector2.zero;
        }
        else if (dir.y == 0)
        {
            unitDirection.y = 0;
        }
        else
        {
            float r = Mathf.Abs(dir.x / dir.y);
            if (r < 1)
            {
                unitDirection.x = 0;
            }
            else
            {
                unitDirection.y = 0;
            }
        }

        unitDirection.Normalize();
        return unitDirection;
    }

    // LayerMask
    public static LayerMask GetLayerMaskIncludingLayers(params string[] layerNames)
    {
        int layerMask = 0;
        foreach (var name in layerNames)
        {
            // bit shift the index of the layer to get a bit mask
            layerMask |= (1 << LayerMask.NameToLayer(name));
        }
        return layerMask;
    }
    public static LayerMask GetLayerMaskExcludingLayers(params string[] layerNames)
    {
        return ~GetLayerMaskIncludingLayers(layerNames);
    }
    public static LayerMask IntersectedWithLayerMask(this LayerMask maskA, LayerMask maskB)
    {
        return (maskA & maskB);
    }
    public static LayerMask UnionWithLayerMask(this LayerMask maskA, LayerMask maskB)
    {
        return (maskA | maskB);
    }
    public static bool IncludesLayerMask(this LayerMask maskA, LayerMask maskB)
    {
        LayerMask intersection = maskA.IntersectedWithLayerMask(maskB);
        return (intersection == maskB);
    }

    // Rect
    public static bool OverlapsRect(this Rect r1, Rect r2)
    {
        return rectangle_collision(r1.x, r1.y, r1.width, r1.height, r2.x, r2.y, r2.width, r2.height);
    }
    static bool rectangle_collision(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
    {
        return !(x1 > x2 + w2 || x1 + w1 < x2 || y1 > y2 + h2 || y1 + h1 < y2);
    }

    // Bounds
    public static Rect ToRectangle_XY(this Bounds bounds)
    {
        return new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
    }

    // String
    public static string[] SplitByNewLine(this string text, StringSplitOptions options = StringSplitOptions.None)
    {
        return text.Split(new string[] { "\r\n", "\n" }, options);
    }

    // Sprite
    public static Texture2D GetTextureSegment(this Sprite sprite)
    {
        Rect r = sprite.textureRect;
        Color[] pixels = sprite.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);

        Texture2D tex = new Texture2D((int)r.width, (int)r.height);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }

    // Random
    public static bool FlipCoin(float chanceOfTrue = 0.5f)
    {
        return (UnityEngine.Random.Range(0.0f, 1.0f) < chanceOfTrue);
    }

    #region Untested

    // Collider
    public static Bounds GetTestBoundsForPosition(this Collider collider, Vector3 position, float expandBy = 0)
    {
        return collider.GetTestBoundsForPosition(position, collider.transform.rotation, expandBy);
    }
    public static Bounds GetTestBoundsForPosition(this Collider collider, Vector3 position, Quaternion rotation, float expandBy = 0)
    {
        Transform ct = collider.transform;
        //Vector3 storedPos = ct.position;
        Quaternion storedRot = ct.rotation;
        //ct.position = position;
        ct.rotation = rotation;

        Bounds testBounds = collider.bounds;
        testBounds.Expand(expandBy);
        testBounds.center += (position - ct.position);

        //ct.position = storedPos;
        ct.rotation = storedRot;

        return testBounds;
    }

    // SphereCollider
    public static float GetScaledRadius(this SphereCollider c)
    {
        float scaledRadius = c.radius;
        Vector3 s = c.transform.localScale;
        if (s.x == s.y && s.y == s.z)		// Only scale if Uniform
            scaledRadius *= s.x;

        return scaledRadius;
    }

    /*public static Bounds GetBoundsInWorldSpace (this GameObject g) {
        Bounds bounds;
        if(g.collider)
            bounds = g.collider.bounds;
        else if(g.renderer)
            bounds = g.renderer.bounds;
        else
            bounds = new Bounds();
        return g.transform.TransformBounds(bounds);
    }

    public static Bounds TransformBounds (this Transform transform, Bounds bounds) {
        Vector2 newCenter = transform.TransformPoint(bounds.center);
        Vector3 newSize = bounds.size;
        newSize.Scale(transform.localScale);
        return new Bounds(newCenter, newSize);
    }
    public static Rect TransformRect (this Transform transform, Rect rect) {
        Vector2 TL = transform.TransformPoint(rect.xMin, rect.yMax, 0);
        Vector2 TR = transform.TransformPoint(rect.xMax, rect.yMax, 0);
        Vector2 BL = transform.TransformPoint(rect.xMin, rect.yMin, 0);
        //Vector2 BR = transform.TransformPoint(rect.xMax, rect.yMin, 0);
        return new Rect(TL.x, TL.y, TR.x - TL.x, TL.y - BL.y);
    }
    */

    #endregion

}
