using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeShatter : MonoBehaviour {

    public int numInstances = 5000;

    public Material material;
    public Mesh mesh;

    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;

    private void Start() {
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(int), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(new uint[] { (uint)mesh.GetIndexCount(0), (uint)numInstances, 0, 0, 0 });

        Vector3[] randomPositions = new Vector3[numInstances];

        for (int i = 0; i < numInstances; i++) {
            randomPositions[i] = Random.insideUnitSphere * 200;
        }

        positionBuffer = new ComputeBuffer(numInstances, sizeof(float) * 3);
        positionBuffer.SetData(randomPositions);

        material.SetBuffer("PositionBuffer", positionBuffer);
    }

    private void Update() {
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, GetBounds(), argsBuffer);
    }

    private Bounds GetBounds() => new Bounds(Vector3.zero, Vector3.one * 1000);

}