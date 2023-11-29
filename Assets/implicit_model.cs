using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

// 从 ClothModel 继承边的初始化和碰撞处理、风场处理等
public class implicit_model : ClothModel
{
    [Header("Implicit Model Properties")]
    [Tooltip("Mass of each mesh")]
    public float mass = 1;
    [Tooltip("Spring constant")]
    [Range(3000f, 20000f)]
    public float spring_k = 8000f;
    [Header("Calculation")]
    [Tooltip("Newton's Method Iteration Times")]
    [Range(1, 64)]
    public int num_iteraions = 32;

    float rho = 0.995f;

    void Get_Gradient(Vector3[] X, Vector3[] X_hat, float t, ref Vector3[] G)
    {
        for (int i = 0; i < X.Length; i++)
            G[i] = -(new Vector3(0, -9.8f * this.mass, 0));
        // 这里的 x_hat 已经不是初始猜测的意思了，而是方程中必须有的 x0 + v0 * dt 这一项
        for (int i = 0; i < X.Length; i++)
        {
            G[i] += (X[i] - X_hat[i]) * this.mass / t / t;
        }
        for (int e = 0; e < this.E.Length / 2; e++)
        {
            int v0 = this.E[e * 2 + 0];
            int v1 = this.E[e * 2 + 1];
            Vector3 d = X[v0] - X[v1];
            float l = d.magnitude;
            d /= l;
            G[v0] += d * (l - L[e]) * this.spring_k;
            G[v1] -= d * (l - L[e]) * this.spring_k;
        }
    }

    void update_force()
    {
        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        Vector3[] X = mesh.vertices;
        Vector3[] last_X = new Vector3[X.Length];
        // 预计位置
        Vector3[] X_hat = new Vector3[X.Length];
        Vector3[] G = new Vector3[X.Length];

        for (int i = 0; i < X.Length; i++)
        {
            this.V[i] *= this.damping;
            X_hat[i] = X[i] + this.dt * this.V[i];
        }

        // 以下是牛顿迭代法
        var acc_w = 0f;
        for (int k = 0; k < this.num_iteraions; k++)
        {
            // gradient 就是 ppt p18 等式右半边
            Get_Gradient(X, X_hat, this.dt, ref G);
            // 切比雪夫加速收敛，统计总残差
            var max_rest = 0f;
            acc_w = k == 0 ? 1 : (k == 1 ? 2 / (2 - this.rho * this.rho) : 4 / (4 - this.rho * this.rho * acc_w));
            for (int i = 0; i < X.Length; i++)
            {
                if (i == 0 || i == this.n_width - 1)
                {
                    continue;
                }
                var old_x = X[i];
                var delta_x = -G[i] / (1 / this.dt / this.dt * this.mass + 4.0f * this.spring_k);
                max_rest = Mathf.Max(max_rest, delta_x.magnitude);
                X[i] += delta_x;
                X[i] = acc_w * X[i] + (1 - acc_w) * last_X[i];
                last_X[i] = old_x;
            }
            if (max_rest < 1e-6)
            {
                break;
            }
        }
        for (int i = 0; i < X.Length; i++)
        {
            if (i == 0 || i == this.n_width - 1)
            {
                continue;
            }
            this.V[i] += (X[i] - X_hat[i]) / this.dt;
        }

        mesh.vertices = X;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        this.update_force();
        base.update_wind_force();
        base.Collision_Handling();
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.RecalculateNormals();
    }
}
