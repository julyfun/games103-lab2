using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SdfCube : SdfDraggable
{
    [Tooltip("Visual Length")]
    [Range(0.05f, 5f)]
    public float vis_x = 2.5f;
    [Tooltip("Visual Height")]
    [Range(0.05f, 5f)]
    public float vis_y = 2.5f;
    [Tooltip("Visual Width")]
    [Range(0.05f, 5f)]
    public float vis_z = 2.5f;
    // 碰撞体积
    float col_x { get { return this.vis_x * 1.2f; } }
    float col_y { get { return this.vis_y * 1.2f; } }
    float col_z { get { return this.vis_z * 1.2f; } }
    public override float sdf(Vector3 pos)
    {
        var rel_pos = pos - this.transform.position;
        var rot_pos = Quaternion.Inverse(this.transform.rotation) * rel_pos;
        return Mathf.Max(Mathf.Max(Mathf.Abs(rot_pos.x) - this.col_x / 2,
            Mathf.Abs(rot_pos.y) - this.col_y / 2),
            Mathf.Abs(rot_pos.z) - this.col_z / 2);
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        // 通过修改缩放来改变物体的可视大小
        this.transform.localScale = new Vector3(this.vis_x, this.vis_y, this.vis_z);
    }
}
