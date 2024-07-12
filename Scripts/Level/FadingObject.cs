using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadingObject : MonoBehaviour, System.IEquatable<FadingObject>
{
    public List<Renderer> renderers = new List<Renderer>();
    public Vector3 Position;
    public List<Material> materials = new List<Material>();
    [HideInInspector]
    public float InitialAlpha;

    private void Awake()
    {
        Position = transform.position;
        if(renderers.Count == 0) 
        {
            renderers.AddRange(GetComponentsInChildren<Renderer>());
        }
        for (int i = 0; i < renderers.Count; i++) 
        {
            materials.AddRange(renderers[i].materials);
        }

        InitialAlpha = materials[0].color.a;
    }

    public bool Equals(FadingObject other)
    {
        return Position.Equals(other.Position);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }


}
