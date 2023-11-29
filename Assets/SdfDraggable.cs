using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SdfDraggable : MonoBehaviour, SdfInterface
{
    public static int selecting_cnt = 0;

    public float rot_speed = 120f; // 旋转速度
    public bool selecting_me = false;
    public Vector3 mouse_move_v = new Vector3(0, 0, 0);
    public bool pressed = false;
    public Vector3 offset;

    bool highlighted = false;
    Material normal_mat;
    Material highlight_mat;
    float dt = 0;
    Vector3 last_x = new Vector3(0, 0, 0);

    public abstract float sdf(Vector3 pos);
    protected virtual void Start()
    {
        this.last_x = this.transform.position;
        this.normal_mat = Resources.Load<Material>("mat-1");
        this.highlight_mat = Resources.Load<Material>("mat-2");
    }
    void access_to_global_variable()
    {
        GlobalVariable global_variable = GameObject.Find("GlobalVariable").GetComponent<GlobalVariable>();
        this.dt = global_variable.dt;
    }
    protected virtual void Update()
    {
        this.access_to_global_variable();
        // 检测鼠标左键按下（按下瞬间）
        if (Input.GetMouseButtonDown(0))
        {
            this.pressed = true;
            // 从摄像机向鼠标位置创建一个射线
            if (((SdfInterface)this).mouse_ray_hit())
            {
                if (!this.selecting_me)
                {
                    SdfDraggable.selecting_cnt += 1;
                    this.selecting_me = true;
                }
            }
            else
            {
                if (this.selecting_me)
                {
                    SdfDraggable.selecting_cnt -= 1;
                    this.selecting_me = false;
                }
            }
            // 计算鼠标位置和球体中心的偏差
            this.offset = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        }

        if (Input.GetMouseButtonUp(0))
        {
            this.pressed = false;
        }

        if (this.selecting_me)
        {
            if (!this.highlighted && this.highlight_mat != null)
            {
                this.GetComponent<Renderer>().material = this.highlight_mat;
                this.highlighted = true;
            }
            float rot_amount = this.rot_speed * Time.deltaTime;

            // 绕 x, y, z 轴旋转
            if (Input.GetKey(KeyCode.W))
            {
                transform.Rotate(Vector3.right * rot_amount);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.Rotate(Vector3.left * rot_amount);
            }

            if (Input.GetKey(KeyCode.A))
            {
                transform.Rotate(Vector3.down * rot_amount);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(Vector3.up * rot_amount);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(Vector3.forward * rot_amount);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(Vector3.back * rot_amount);
            }
        }
        else
        {
            if (this.highlighted)
            {
                this.GetComponent<Renderer>().material = this.normal_mat;
                this.highlighted = false;
            }
        }

        if (this.pressed && this.selecting_me)
        {
            // 图像坐标系的
            Vector3 mouse = Input.mousePosition;
            // 这一步以后有：mouse = input_now - (input_down - ball_center_pixel_down)
            // = ball_center_pixel_down + input_now - input_down
            mouse -= offset;
            // 球体的深度不变
            mouse.z = Camera.main.WorldToScreenPoint(transform.position).z;
            // 屏幕坐标转世界坐标
            this.last_x = this.transform.position;
            this.transform.position = Camera.main.ScreenToWorldPoint(mouse);
            this.mouse_move_v = (this.transform.position - this.last_x) / this.dt;
        }
    }
}
