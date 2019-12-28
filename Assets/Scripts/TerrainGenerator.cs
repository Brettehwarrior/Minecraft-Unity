using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Material material;
    public Transform parent;

    public BlockTextureCoords[] blockTextureCoords;

    public List<GameObject> chunks = new List<GameObject>();

    void Start() {
        for (int i = 0; i < 12; i++) {
            for (int j = 0; j < 12; j++) {
                GameObject chunk = new GameObject("Chunk", typeof(MeshFilter), typeof(MeshRenderer));
                Chunk chunkScript = chunk.AddComponent<Chunk>();
                chunkScript.blockTextureCoords = blockTextureCoords;
                chunkScript.generator = this;

                chunkScript.GenerateTerrain(i, j);
                Mesh mesh = chunkScript.BuildMesh();
                mesh.RecalculateNormals();

                chunk.transform.Translate(new Vector3(i*chunkScript.blocks.GetLength(0), 0, j*chunkScript.blocks.GetLength(2)));
                chunk.GetComponent<MeshFilter>().mesh = mesh;
                chunk.GetComponent<MeshRenderer>().material = material;
                chunk.transform.parent = parent;

                chunks.Add(chunk);
            }
        }

        foreach (GameObject chunk in chunks) {

        }
    }

    

    public enum BlockType {
        air,
        dirt,
        grass,
        stone
    }

    public enum Faces {
        x,
        xn,
        y,
        yn,
        z,
        zn
    }

    [System.Serializable]
    public struct BlockTextureCoords {
        public BlockType type;
        public Vector2[] coordinates;
    }
}
