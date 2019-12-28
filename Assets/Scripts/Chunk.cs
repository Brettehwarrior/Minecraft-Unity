using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Chunk : MonoBehaviour
{
    public TerrainGenerator generator;

    public BlockType[ , , ] blocks = new BlockType[8,32,8];
    public TerrainGenerator.BlockTextureCoords[] blockTextureCoords;

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

    public void GenerateTerrain(int x, int z) {
        for (int i = 0; i < blocks.GetLength(0); i++) {
            for (int j = 0; j < blocks.GetLength(2); j++) {
                for (int k = 0; k < blocks.GetLength(1); k++)
                {
                    /// Generate Terrain
                    int maxHeight = (int)(Mathf.PerlinNoise((x*8+i)*0.03f, (z*8+j)*0.03f) * blocks.GetLength(1));
                    BlockType type = BlockType.air;

                    if (k == maxHeight) {
                        type = BlockType.grass;
                    } else if (k < maxHeight) {
                        type = BlockType.dirt;
                    } else if (k < maxHeight - 6) {
                        type = BlockType.stone;
                    }

                    blocks[i,k,j] = type;
                }
            }
        }
    }

    public Mesh BuildMesh() {
        List<Vector2> uv = new List<Vector2>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();


        int voxelIndex = 0;
        for (int i = 0; i < blocks.GetLength(0); i++)  {
            for (int j = 0; j < blocks.GetLength(2); j++) {
                for (int k = 0; k < blocks.GetLength(1); k++)
                {
                    if (blocks[i, k, j] != BlockType.air) {
                        AddVerticies(ref vertices, i, k, j);
                        SetUVs(ref uv, blocks[i, k, j]);
                        // Check Y+
                        try {
                            if (blocks[i, k+1, j] == BlockType.air) {
                                AddFace(ref triangles, Faces.y, voxelIndex);
                            }
                        } catch { }
                        // Check Y-
                        try {
                            if (blocks[i, k-1, j] == BlockType.air) {
                                AddFace(ref triangles, Faces.yn, voxelIndex);
                            }
                        } catch { }
                        // Check X+
                        try {
                            if (blocks[i+1, k, j] == BlockType.air) {
                                AddFace(ref triangles, Faces.x, voxelIndex);
                            }
                        } catch {
                            if (NeighborHasBlockAtPosition(Faces.x, new Vector3Int(i, k, j))) {
                                AddFace(ref triangles, Faces.x, voxelIndex);
                            }
                        }
                        // Check X-
                        try {
                            if (blocks[i-1, k, j] == BlockType.air) {
                                AddFace(ref triangles, Faces.xn, voxelIndex);
                            }
                        } catch {
                            if (NeighborHasBlockAtPosition(Faces.xn, new Vector3Int(i, k, j))) {
                                AddFace(ref triangles, Faces.xn, voxelIndex);
                            }
                        }
                        // Check Z+
                        try {
                            if (blocks[i, k, j+1] == BlockType.air) {
                                AddFace(ref triangles, Faces.z, voxelIndex);
                            }
                        } catch {
                            if (NeighborHasBlockAtPosition(Faces.z, new Vector3Int(i, k, j))) {
                                AddFace(ref triangles, Faces.z, voxelIndex);
                            }
                        }
                        // Check Z-
                        try {
                            if (blocks[i, k, j-1] == BlockType.air) {
                                AddFace(ref triangles, Faces.zn, voxelIndex);
                            }
                        } catch {
                            if (NeighborHasBlockAtPosition(Faces.zn, new Vector3Int(i, k, j))) {
                                AddFace(ref triangles, Faces.zn, voxelIndex);
                            }
                        }

                        voxelIndex ++;
                    }
                }
            }
        }
        

        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.uv = uv.ToArray();
        mesh.triangles = triangles.ToArray();

        return mesh;
    }

    public bool NeighborHasBlockAtPosition(Faces dir, Vector3Int pos) {
        bool found = false;

        for (var i = 0; i < generator.chunks.Count; i++) {
            GameObject c = generator.chunks[i];
            Chunk chunkScript = c.GetComponent<Chunk>();
            switch(dir) {
                case Faces.x:
                    if (c.transform.position.x == transform.position.x + blocks.GetLength(0) && c.transform.position.z == transform.position.z) {
                        if (chunkScript.blocks[0, pos.y, pos.z] != BlockType.air)
                            found = true;
                    }
                    break;
                case Faces.xn:
                    if (c.transform.position.x == transform.position.x - blocks.GetLength(0) && c.transform.position.z == transform.position.z) {
                        if (chunkScript.blocks[blocks.GetLength(0)-1, pos.y, pos.z] != BlockType.air)
                            found = true;
                    }
                    break;
                case Faces.z:
                    if (c.transform.position.z == transform.position.z + blocks.GetLength(2) && c.transform.position.x == transform.position.x) {
                        if (chunkScript.blocks[pos.x, pos.y, 0] != BlockType.air)
                            found = true;
                    }
                    break;
                case Faces.zn:
                    if (c.transform.position.z == transform.position.z - blocks.GetLength(2) && c.transform.position.x == transform.position.x) {
                        if (chunkScript.blocks[pos.x, pos.y, blocks.GetLength(2)-1] != BlockType.air)
                            found = true;
                    }
                    break;
            }

            if (found)
                break;
        }

        return found;
    }

    public void AddVerticies(ref List<Vector3> vertices, float x, float y, float z) {
        // Y+
        vertices.Add(new Vector3(x-.5f, y+.5f, z+.5f)); //0
        vertices.Add(new Vector3(x+.5f, y+.5f, z+.5f)); //1
        vertices.Add(new Vector3(x+.5f, y+.5f, z-.5f)); //2
        vertices.Add(new Vector3(x-.5f, y+.5f, z-.5f)); //3
        // Y-
        vertices.Add(new Vector3(x-.5f, y-.5f, z+.5f)); //4
        vertices.Add(new Vector3(x+.5f, y-.5f, z+.5f)); //5
        vertices.Add(new Vector3(x+.5f, y-.5f, z-.5f)); //6
        vertices.Add(new Vector3(x-.5f, y-.5f, z-.5f)); //7
        // X+
        vertices.Add(new Vector3(x+.5f, y+.5f, z-.5f)); //8
        vertices.Add(new Vector3(x+.5f, y+.5f, z+.5f)); //9
        vertices.Add(new Vector3(x+.5f, y-.5f, z+.5f)); //10
        vertices.Add(new Vector3(x+.5f, y-.5f, z-.5f)); //11
        // X-
        vertices.Add(new Vector3(x-.5f, y+.5f, z+.5f)); //12
        vertices.Add(new Vector3(x-.5f, y+.5f, z-.5f)); //13
        vertices.Add(new Vector3(x-.5f, y-.5f, z-.5f)); //14
        vertices.Add(new Vector3(x-.5f, y-.5f, z+.5f)); //15
        // Z+
        vertices.Add(new Vector3(x+.5f, y+.5f, z+.5f)); //16
        vertices.Add(new Vector3(x-.5f, y+.5f, z+.5f)); //17
        vertices.Add(new Vector3(x-.5f, y-.5f, z+.5f)); //18
        vertices.Add(new Vector3(x+.5f, y-.5f, z+.5f)); //19
        // Z-
        vertices.Add(new Vector3(x-.5f, y+.5f, z-.5f)); //20
        vertices.Add(new Vector3(x+.5f, y+.5f, z-.5f)); //21
        vertices.Add(new Vector3(x-.5f, y-.5f, z-.5f)); //22
        vertices.Add(new Vector3(x+.5f, y-.5f, z-.5f)); //23
    }

    public void SetUVs(ref List<Vector2> uv, BlockType blockType) {
        float tileSize = 16;
        float tileOffset = 1 / tileSize;

        float tileX;
        float tileY;

        float uMin;
        float uMax;
        float vMin;
        float vMax;

        // Y+
        resetUVCoords(0);
        uv.Add(new Vector2(uMin, vMax));
        uv.Add(new Vector2(uMax, vMax));
        uv.Add(new Vector2(uMax, vMin));
        uv.Add(new Vector2(uMin, vMin));


        // Y-
        resetUVCoords(2);
        uv.Add(new Vector2(uMin, vMax));
        uv.Add(new Vector2(uMax, vMax));
        uv.Add(new Vector2(uMax, vMin));
        uv.Add(new Vector2(uMin, vMin));


        // X+
        resetUVCoords(1);
        uv.Add(new Vector2(uMin, vMax));
        uv.Add(new Vector2(uMax, vMax));
        uv.Add(new Vector2(uMax, vMin));
        uv.Add(new Vector2(uMin, vMin));
        // X-
        uv.Add(new Vector2(uMin, vMax));
        uv.Add(new Vector2(uMax, vMax));
        uv.Add(new Vector2(uMax, vMin));
        uv.Add(new Vector2(uMin, vMin));
        // Z+
        uv.Add(new Vector2(uMin, vMax));
        uv.Add(new Vector2(uMax, vMax));
        uv.Add(new Vector2(uMax, vMin));
        uv.Add(new Vector2(uMin, vMin));
        // Z-
        uv.Add(new Vector2(uMin, vMax));
        uv.Add(new Vector2(uMax, vMax));
        uv.Add(new Vector2(uMin, vMin));
        uv.Add(new Vector2(uMax, vMin));

        void resetUVCoords(int faceIndex) {
            tileX = blockTextureCoords[(int)blockType].coordinates[faceIndex].x;
            tileY = blockTextureCoords[(int)blockType].coordinates[faceIndex].y;

            uMin = tileOffset*tileX;
            uMax = tileOffset*(tileX+1);
            vMin = tileOffset*tileY;
            vMax = tileOffset*(tileY+1);
        }
    }

    void AddFace(ref List<int> triangles, Faces faceID, int index) {
        int offset = index * 24;
        switch (faceID) {
            case Faces.y:
                // Y+
                triangles.Add(3+offset);
                triangles.Add(0+offset);
                triangles.Add(1+offset);
                triangles.Add(3+offset);
                triangles.Add(1+offset);
                triangles.Add(2+offset);
                break;
            case Faces.yn:
                // Y-
                triangles.Add(4+offset);
                triangles.Add(7+offset);
                triangles.Add(6+offset);
                triangles.Add(4+offset);
                triangles.Add(6+offset);
                triangles.Add(5+offset);
                break;
            case Faces.x:
                // X+
                triangles.Add(11+offset);
                triangles.Add(8+offset);
                triangles.Add(9+offset);
                triangles.Add(11+offset);
                triangles.Add(9+offset);
                triangles.Add(10+offset);
                break;
            case Faces.xn:
                // X-
                triangles.Add(15+offset);
                triangles.Add(12+offset);
                triangles.Add(13+offset);
                triangles.Add(15+offset);
                triangles.Add(13+offset);
                triangles.Add(14+offset);
                break;
            case Faces.z:
                // Z+
                triangles.Add(19+offset);
                triangles.Add(16+offset);
                triangles.Add(17+offset);
                triangles.Add(19+offset);
                triangles.Add(17+offset);
                triangles.Add(18+offset);
                break;
            case Faces.zn:
                // Z-
                triangles.Add(22+offset);
                triangles.Add(20+offset);
                triangles.Add(21+offset);
                triangles.Add(22+offset);
                triangles.Add(21+offset);
                triangles.Add(23+offset);
                break;
        }
    }
}
