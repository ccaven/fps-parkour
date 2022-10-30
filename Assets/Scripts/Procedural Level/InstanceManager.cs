using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InstanceManager : MonoBehaviour {

    public int numInstances;
    
    public Material material;
    
    public Mesh mesh;

    public ComputeShader propertyEditShader;

    private ComputeBuffer propertiesBuffer;
    private ComputeBuffer argsBuffer;

    private Bounds bounds;

    private struct MeshProperties {

        public Vector3 position;
        public Matrix4x4 rotation;
        public Vector3 scale;

        public Vector4 color;

        public MeshProperties(Vector3 p, Quaternion q, Vector3 s, Vector3 c) {
            position = p;
            rotation = Matrix4x4.Rotate(q);
            scale = s;
            color = c;
        }

        public static int Size() {
            return sizeof(float) * 3 + sizeof(float) * 4 * 4 + sizeof(float) * 3 + sizeof(float) * 4;
        }
    }

    private void Awake() {
        bounds = new Bounds(transform.position, Vector3.one * 1000);

        uint[] args = { 
            mesh.GetIndexCount(0),
            (uint)numInstances,
            mesh.GetIndexStart(0),
            mesh.GetBaseVertex(0),
            0
        };

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(int), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        MeshProperties[] properties = new MeshProperties[numInstances];

        for (int i = 0; i < numInstances; i++) {

            Vector3 p = Random.insideUnitSphere * 500f;

            Quaternion q = Random.rotation;
            Vector3 s = Vector3.one * Random.Range(0.5f, 2f);

            Vector4 c = new Vector4(Random.value, Random.value, Random.value, 1f);

            properties[i] = new MeshProperties(p, q, s, c);
        }

        propertiesBuffer = new ComputeBuffer(numInstances, MeshProperties.Size());
        propertiesBuffer.SetData(properties);

        material.SetBuffer("_Properties", propertiesBuffer);
    }

    private void Update() {
        RunComputeShader();

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    private void OnDisable() {
        // Release memory

        if (propertiesBuffer != null) {
            propertiesBuffer.Release();
        }

        propertiesBuffer = null;

        if (argsBuffer != null) { 
            argsBuffer.Release();
        }

        argsBuffer = null;
    }

    private void RunComputeShader() {
        propertyEditShader.SetFloat("_Time", Time.time);

        propertyEditShader.SetBuffer(0, "_MeshProperties", propertiesBuffer);

        int numThreads = Mathf.CeilToInt(numInstances / 64);

        propertyEditShader.Dispatch(0, numThreads, 1, 1);
    }
}