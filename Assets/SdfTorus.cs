using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SdfTorus : SdfDraggable
{
    // 用环轴半径和截面半径来定义 Torus
    float radius = 4.0f;
    float section_radius = 1.0f;
    float col_section_radius { get { return this.section_radius * 1.1f; } }

    public override float sdf(Vector3 pos)
    {
        var rel_pos = pos - this.transform.position;
        var rot_pos = Quaternion.Inverse(this.transform.rotation) * rel_pos;
        float r1 = this.radius;
        float r2 = this.col_section_radius;
        float x = Mathf.Sqrt(rot_pos.x * rot_pos.x + rot_pos.z * rot_pos.z) - r1;
        float y = rot_pos.y;
        return Mathf.Sqrt(x * x + y * y) - r2;
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        // 未实现改变大小
        base.Update();
    }
}
