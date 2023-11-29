using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public bool pressed = false;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.pressed = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            this.pressed = false;
        }

        if (this.pressed && SdfDraggable.selecting_cnt == 0)
        {
            float v = 2.0f * Input.GetAxis("Mouse Y");
            float h = 2.0f * Input.GetAxis("Mouse X");
            Camera.main.transform.RotateAround(new Vector3(0, 0, 0), Vector3.up, h);
            Vector3 cam_forward = Camera.main.transform.forward;
            Vector3 cam_up = Camera.main.transform.up;
            // 垂直移动鼠标时，需要绕一个平行于地面和相机平面的轴旋转
            Vector3 parallel_to_ground_and_camera = Vector3.Cross(cam_forward, cam_up).normalized;
            Camera.main.transform.RotateAround(new Vector3(0, 0, 0), parallel_to_ground_and_camera, v);
        }

        // 选中至少一个物体时，旋转物体而不是移动镜头
        if (SdfDraggable.selecting_cnt == 0)
        {
            if (Input.GetKey(KeyCode.W))
            {
                Camera.main.transform.Translate(Vector3.forward);
            }

            if (Input.GetKey(KeyCode.S))
            {
                Camera.main.transform.Translate(Vector3.back);
            }
        }
    }

}
