using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class voxelMap : MonoBehaviour
{
    editVoxel mainVoxel;
    public Vector3Int size;
    public int[,,] cubes;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private List<int> tris = new List<int>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private int index = 0;
    private int textureMatrix = 0;
    private int firstHeight;


    void Awake()
    {
        mainVoxel = GameObject.FindWithTag("Player").GetComponent<editVoxel>();

        textureMatrix = mainVoxel.textureMatix;

        size = new Vector3Int(mainVoxel.size, mainVoxel.height, mainVoxel.size);

        cubes = new int[size.x + 2, size.y, size.z + 2];

        meshFilter = GetComponent<MeshFilter>();

        meshCollider = GetComponent<MeshCollider>();

        Vector2Int posObject = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        //40=mountain height 10=place height
        firstHeight = 40 + 10 + 1;

        int mapIndex = -1;
        for (int a = 0; a < mainVoxel.mapDatas.Count; a++)
        {
            if (posObject == mainVoxel.mapDatas[a].pos)
            {
                mapIndex = a;
                firstHeight = size.y;
            }
        }

        for (int x = 0; x < size.x + 2; x++)
        {
            for (int z = 0; z < size.z + 2; z++)
            {

                float bigDetail = Mathf.PerlinNoise((x + posObject.x) * 0.005f - 9999, (z + posObject.y) * 0.005f - 9999);
                int maxHeight = Mathf.FloorToInt(Mathf.PerlinNoise((x + posObject.x) * 0.025f - 9999, (z + posObject.y) * 0.025f - 9999) * 40 * bigDetail);

                for (int y = 0; y <= maxHeight + 10; y++)
                {
                    cubes[x, y, z] = 1;
                }
            }
        }

        if (mapIndex != -1)
        {
            Vector3Int[] poses = mainVoxel.mapDatas[mapIndex].savedBlocks;
            int[] blockIndex = mainVoxel.mapDatas[mapIndex].blockIndex;
            for (int b = 0; b < blockIndex.Length; b++)
            {
                Vector3Int pos = poses[b];
                cubes[pos.x, pos.y, pos.z] = blockIndex[b];
            }
            mainVoxel.mapDatas.RemoveAt(mapIndex);
        }




        createMesh();
        firstHeight = size.y;
    }

    void createFace(Vector3 normal, int blockIndex)
    {
        int[] trisA ={
            index+0,index+1,index+2,
            index+2,index+1,index+3
        };
        Vector3[] normalsA ={
            normal,
            normal,
            normal,
            normal
        };
        float posTexture = 1f / textureMatrix;
        int line = (blockIndex - 1) / textureMatrix;

        float vertical = (textureMatrix - 1 - line) / (float)textureMatrix;
        float horizontal = ((blockIndex - 1) - line * textureMatrix) / (float)textureMatrix;

        Vector2[] uvsA ={
            new Vector2(posTexture+horizontal,vertical),
            new Vector2(posTexture+horizontal,posTexture+vertical),
            new Vector2(horizontal,vertical),
            new Vector2(horizontal,posTexture+vertical)
        };

        tris.AddRange(trisA);
        normals.AddRange(normalsA);
        uvs.AddRange(uvsA);
        index += 4;
    }
    public void createMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        List<Vector3> vertices = new List<Vector3>();

        for (int x = 1; x < size.x + 1; x++)
        {
            for (int y = 0; y < firstHeight; y++)
            {
                for (int z = 1; z < size.z + 1; z++)
                {
                    if (cubes[x, y, z] != 0)
                    {

                        //right
                        if (cubes[x + 1, y, z] == 0)
                        {
                            Vector3[] verticesA ={
                                new Vector3(x+0.5f,y+0.5f,z-0.5f),
                                new Vector3(x+0.5f,y+0.5f,z+0.5f),
                                new Vector3(x+0.5f,y-0.5f,z-0.5f),
                                new Vector3(x+0.5f,y-0.5f,z+0.5f)
                            };
                            vertices.AddRange(verticesA);
                            createFace(Vector3.right, cubes[x, y, z]);
                        }

                        //left
                        if (cubes[x - 1, y, z] == 0)
                        {
                            Vector3[] verticesA ={
                                new Vector3(x-0.5f,y+0.5f,z+0.5f),
                                new Vector3(x-0.5f,y+0.5f,z-0.5f),
                                new Vector3(x-0.5f,y-0.5f,z+0.5f),
                                new Vector3(x-0.5f,y-0.5f,z-0.5f)
                            };
                            vertices.AddRange(verticesA);
                            createFace(Vector3.left, cubes[x, y, z]);
                        }

                        //up
                        if (y != size.y - 1)
                        {
                            if (cubes[x, y + 1, z] == 0)
                            {
                                Vector3[] verticesA ={
                                    new Vector3(x+0.5f,y+0.5f,z+0.5f),
                                    new Vector3(x+0.5f,y+0.5f,z-0.5f),
                                    new Vector3(x-0.5f,y+0.5f,z+0.5f),
                                    new Vector3(x-0.5f,y+0.5f,z-0.5f)
                                };
                                vertices.AddRange(verticesA);
                                createFace(Vector3.up, cubes[x, y, z]);
                            }
                        }

                        //down
                        if (y != 0)
                        {
                            if (cubes[x, y - 1, z] == 0)
                            {

                                Vector3[] verticesA ={
                                    new Vector3(x+0.5f,y-0.5f,z-0.5f),
                                    new Vector3(x+0.5f,y-0.5f,z+0.5f),
                                    new Vector3(x-0.5f,y-0.5f,z-0.5f),
                                    new Vector3(x-0.5f,y-0.5f,z+0.5f)
                                };
                                vertices.AddRange(verticesA);
                                createFace(Vector3.down, cubes[x, y, z]);
                            }
                        }

                        //forward
                        if (cubes[x, y, z + 1] == 0)
                        {
                            Vector3[] verticesA ={
                                new Vector3(x+0.5f,y-0.5f,z+0.5f),
                                new Vector3(x+0.5f,y+0.5f,z+0.5f),
                                new Vector3(x-0.5f,y-0.5f,z+0.5f),
                                new Vector3(x-0.5f,y+0.5f,z+0.5f)
                            };
                            vertices.AddRange(verticesA);
                            createFace(Vector3.forward, cubes[x, y, z]);
                        }

                        //back
                        if (cubes[x, y, z - 1] == 0)
                        {
                            Vector3[] verticesA ={
                                new Vector3(x+0.5f,y+0.5f,z-0.5f),
                                new Vector3(x+0.5f,y-0.5f,z-0.5f),
                                new Vector3(x-0.5f,y+0.5f,z-0.5f),
                                new Vector3(x-0.5f,y-0.5f,z-0.5f)
                            };
                            vertices.AddRange(verticesA);
                            createFace(Vector3.back, cubes[x, y, z]);
                        }
                    }
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        meshFilter.mesh.RecalculateBounds();
        meshFilter.mesh.RecalculateTangents();

        index = 0;
        tris.Clear();
        normals.Clear();
        uvs.Clear();
    }
}
