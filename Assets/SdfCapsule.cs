
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SdfCapsule : SdfDraggable
{
    [Tooltip("Visual radius (central length twice)")]
    [Range(0.05f, 5f)]
    public float vis_radius = 1.5f;
    float col_radius { get { return this.vis_radius * 1.1f; } }
    public override float sdf(Vector3 pos)
    {
        var rel_pos = pos - this.transform.position;
        var rot_pos = Quaternion.Inverse(this.transform.rotation) * rel_pos;
        float h = this.col_radius * 2f;
        float r = this.col_radius;
        rot_pos.y -= Mathf.Clamp(rot_pos.y, -h / 2, h / 2);
        return rot_pos.magnitude - r;
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        this.transform.localScale = new Vector3(this.vis_radius * 2.0f, this.vis_radius * 2.0f, this.vis_radius * 2.0f);
    }
}
