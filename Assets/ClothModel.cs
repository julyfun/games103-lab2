using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothModel : MonoBehaviour
{
    [Header("Mesh Properties")]
    [Tooltip("Mesh volocity damping factor")]
    [Range(0.8f, 1.0f)]
    public float damping = 0.99f;
    [Tooltip("Friction coefficient")]
    [Range(0f, 1.0f)]
    public float restitution = 0.5f;
    [Header("Initial Mesh Size")]
    [Range(5, 40)]
    public int n_width = 21;
    [Range(5f, 40f)]
    public float width_meter = 10.0f;

    [HideInInspector]
    public float dt = 1.0f; // 从全局变量中获取的
    [HideInInspector]
    public int[] E;
    [HideInInspector]
    public float[] L;
    [HideInInspector]
    public Vector3[] V;

    protected virtual void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        //Resize the mesh.
        int n = this.n_width;
        Vector3[] X = new Vector3[n * n];
        Vector2[] UV = new Vector2[n * n];
        int[] T = new int[(n - 1) * (n - 1) * 6];
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
            {
                float w = this.width_meter;
                X[j * n + i] = new Vector3(w / 2f - w * i / (n - 1), 0, 5 - w * j / (n - 1));
                UV[j * n + i] = new Vector3(i / (n - 1.0f), j / (n - 1.0f));
            }
        int t = 0;
        for (int j = 0; j < n - 1; j++)
            for (int i = 0; i < n - 1; i++)
            {
                T[t * 6 + 0] = j * n + i;
                T[t * 6 + 1] = j * n + i + 1;
                T[t * 6 + 2] = (j + 1) * n + i + 1;
                T[t * 6 + 3] = j * n + i;
                T[t * 6 + 4] = (j + 1) * n + i + 1;
                T[t * 6 + 5] = (j + 1) * n + i;
                t++;
            }
        mesh.vertices = X;
        mesh.triangles = T;
        mesh.uv = UV;
        mesh.RecalculateNormals();

        //Construct the original edge list
        int[] _E = new int[T.Length * 2];
        for (int i = 0; i < T.Length; i += 3)
        {
            _E[i * 2 + 0] = T[i + 0];
            _E[i * 2 + 1] = T[i + 1];
            _E[i * 2 + 2] = T[i + 1];
            _E[i * 2 + 3] = T[i + 2];
            _E[i * 2 + 4] = T[i + 2];
            _E[i * 2 + 5] = T[i + 0];
        }
        //Reorder the original edge list
        for (int i = 0; i < _E.Length; i += 2)
            if (_E[i] > _E[i + 1])
                Swap(ref _E[i], ref _E[i + 1]);
        //Sort the original edge list using quicksort
        Quick_Sort(ref _E, 0, _E.Length / 2 - 1);

        int e_number = 0;
        for (int i = 0; i < _E.Length; i += 2)
            if (i == 0 || _E[i + 0] != _E[i - 2] || _E[i + 1] != _E[i - 1])
                e_number++;

        E = new int[e_number * 2];
        for (int i = 0, e = 0; i < _E.Length; i += 2)
            if (i == 0 || _E[i + 0] != _E[i - 2] || _E[i + 1] != _E[i - 1])
            {
                E[e * 2 + 0] = _E[i + 0];
                E[e * 2 + 1] = _E[i + 1];
                e++;
            }

        L = new float[E.Length / 2];
        for (int e = 0; e < E.Length / 2; e++)
        {
            int i = E[e * 2 + 0];
            int j = E[e * 2 + 1];
            L[e] = (X[i] - X[j]).magnitude;
        }

        V = new Vector3[X.Length];
        for (int i = 0; i < X.Length; i++)
            V[i] = new Vector3(0, 0, 0);
    }

    void Quick_Sort(ref int[] a, int l, int r)
    {
        int j;
        if (l < r)
        {
            j = Quick_Sort_Partition(ref a, l, r);
            Quick_Sort(ref a, l, j - 1);
            Quick_Sort(ref a, j + 1, r);
        }
    }

    int Quick_Sort_Partition(ref int[] a, int l, int r)
    {
        int pivot_0, pivot_1, i, j;
        pivot_0 = a[l * 2 + 0];
        pivot_1 = a[l * 2 + 1];
        i = l;
        j = r + 1;
        while (true)
        {
            do ++i; while (i <= r && (a[i * 2] < pivot_0 || a[i * 2] == pivot_0 && a[i * 2 + 1] <= pivot_1));
            do --j; while (a[j * 2] > pivot_0 || a[j * 2] == pivot_0 && a[j * 2 + 1] > pivot_1);
            if (i >= j) break;
            Swap(ref a[i * 2], ref a[j * 2]);
            Swap(ref a[i * 2 + 1], ref a[j * 2 + 1]);
        }
        Swap(ref a[l * 2 + 0], ref a[j * 2 + 0]);
        Swap(ref a[l * 2 + 1], ref a[j * 2 + 1]);
        return j;
    }

    void Swap(ref int a, ref int b)
    {
        int temp = a;
        a = b;
        b = temp;
    }

    public void Collision_Handling()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] X = mesh.vertices;

        SdfDraggable[] objects = GameObject.FindObjectsOfType<SdfDraggable>();
        for (int i = 0; i < objects.Length; i++)
        {
            SdfDraggable obj = objects[i];
            for (int j = 0; j < X.Length; j++)
            {
                if (j == 0 || j == this.n_width - 1)
                {
                    continue;
                }
                var sdf = obj.sdf(X[j]);
                if (sdf >= 0.3f)
                {
                    continue;
                }
                // sdf 函数为正时在外部
                // 仿照刚体碰撞计算摩擦效应，以下计算都是相对速度
                var rubber_resti = this.restitution * ((0.3f - sdf) / 0.3f);
                var hit_normal = ((SdfInterface)obj).normal(X[j]);
                var rel_v = this.V[j] - obj.mouse_move_v;
                var v_ni = Vector3.Dot(rel_v, hit_normal) * hit_normal;
                var v_ti = rel_v - v_ni;
                // 切向速度衰减系数
                var a = Mathf.Max(1 - rubber_resti * (1 + rubber_resti)
                    * Vector3.SqrMagnitude(v_ni) / Vector3.SqrMagnitude(v_ti), 0);
                var v_ni_new = -Mathf.Min(1f, rubber_resti) * v_ni;
                var v_ti_new = a * v_ti;
                this.V[j] = obj.mouse_move_v + v_ni_new + v_ti_new;
                if (sdf < 0f)
                {
                    var x_to_be = X[j] - sdf * hit_normal;
                    this.V[j] += (x_to_be - X[j]) / this.dt;
                    X[j] = x_to_be;
                }
            }
        }

        mesh.vertices = X;
    }
    // 获取风场并更新每个结点速度 
    public void update_wind_force()
    {
        GameObject wind_zone = GameObject.Find("wind-zone");
        WindZone wind_zone_comp = wind_zone.GetComponent<WindZone>();
        Vector3 wind_dir = wind_zone_comp.transform.forward;
        float wind_strength = wind_zone_comp.windMain;

        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector3 normal = normals[i];

            // 迎风面积可正可负
            float facing_area = Vector3.Dot(normal, wind_dir);
            Vector3 wind_force = wind_strength * facing_area * normal;
            this.V[i] += wind_force * this.dt;
        }
    }

    public void access_to_global_variable()
    {
        GlobalVariable global_variable = GameObject.Find("GlobalVariable").GetComponent<GlobalVariable>();
        this.dt = global_variable.dt;
    }
    protected virtual void Update()
    {
        this.access_to_global_variable();
    }
}
