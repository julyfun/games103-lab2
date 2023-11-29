using UnityEngine;
using System.Collections;
using System.Net.Mail;

public class PBD_model : ClothModel
{
    [Header("PBD Calculation")]
    [Tooltip("Strain Limiting Iteration Times")]
    [Range(1, 64)]
    public int num_iteraions = 32;

    public void update_elastic()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] X = mesh.vertices;
        for (int i = 0; i < X.Length; i++)
        {
            if (i == 0 || i == this.n_width - 1) continue;
            this.V[i] *= this.damping;
            this.V[i] += (new Vector3(0f, -9.8f, 0f)) * this.dt;
            X[i] += this.dt * this.V[i];
        }
        mesh.vertices = X;

        for (int l = 0; l < this.num_iteraions; l++)
        {
            this.Strain_Limiting();
        }
    }

    void Strain_Limiting()
    {
        // Strain Limiting 让弹簧的长度保持在一定范围内
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] X = mesh.vertices;

        var sum_x = new Vector3[X.Length];
        var sum_n = new int[X.Length];
        for (int e = 0; e < this.E.Length / 2; e++)
        {
            // 第 e 条边的两个结点
            int i = this.E[e * 2 + 0];
            int j = this.E[e * 2 + 1];
            var dir = (X[i] - X[j]).normalized;
            sum_x[i] += 0.5f * (X[i] + X[j] + L[e] * dir);
            sum_n[i]++;
            sum_x[j] += 0.5f * (X[i] + X[j] - L[e] * dir);
            sum_n[j]++;
        }
        for (int i = 0; i < X.Length; i++)
        {
            if (i == 0 || i == this.n_width - 1) continue;
            this.V[i] += 1f / this.dt * (
                (0.2f * X[i] + sum_x[i]) / (0.2f + sum_n[i]) - X[i]);
            X[i] = (0.2f * X[i] + sum_x[i]) / (0.2f + sum_n[i]);
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
        this.update_elastic();
        base.update_wind_force();
        base.Collision_Handling();
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.RecalculateNormals();
    }
}
