using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface SdfInterface
{
    float sdf(Vector3 pos);
    Vector3 normal(Vector3 pos)
    {
        float eps = 0.001f;
        float x = this.sdf(pos + new Vector3(eps, 0, 0)) - this.sdf(pos - new Vector3(eps, 0, 0));
        float y = this.sdf(pos + new Vector3(0, eps, 0)) - this.sdf(pos - new Vector3(0, eps, 0));
        float z = this.sdf(pos + new Vector3(0, 0, eps)) - this.sdf(pos - new Vector3(0, 0, eps));
        return new Vector3(x, y, z).normalized;
    }
    // 计算一个光线是否与我碰撞
    bool ray_march(Vector3 pos, Vector3 dir)
    {
        int step = 0;
        float d = this.sdf(pos);
        while (d > 0.001 && step < 50)
        {
            pos = pos + dir * d;
            d = this.sdf(pos);
            step++;
        }
        return step < 50;
    }
    bool mouse_ray_hit()
    {
        // 从摄像机向鼠标位置创建一个射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return this.ray_march(ray.origin, ray.direction);
    }
}
