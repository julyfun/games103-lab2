using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SdfSphere : SdfDraggable
{
    [Tooltip("Visual radius of the sphere.")]
    [Range(0.05f, 5f)]
    public float vis_radius = 2.5f;
    float col_radius
    {
        get
        {
            return this.vis_radius * 1.1f;
        }
    }

    // 各种几何体的距离场函数参见：
    // https://iquilezles.org/articles/distfunctions/
    public override float sdf(Vector3 pos)
    {
        return (pos - this.transform.position).magnitude - this.col_radius;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        this.transform.localScale = new Vector3(this.vis_radius * 2, this.vis_radius * 2, this.vis_radius * 2);
    }
}
