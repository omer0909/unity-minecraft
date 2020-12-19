using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class voxelMap : MonoBehaviour
{
    public Vector3Int size;
    public int[,,] cubes;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private List<int> tris=new List<int>();
    private List<Vector3> normals=new List<Vector3>();
    private List<Vector2> uvs=new List<Vector2>();
    public voxelMap rightM,leftM,forwardM,backM;
    private int index=0;
    private float bigDetailMultiplay=0.005f;
    private float detailMultiplay=0.025f;
    private float detailHeightMultiplay=40;
    private int mapHeight=10;
    private int textureMatrix=0;


    void Awake()
    {
        textureMatrix= editVoxel.textureMatix;

        size=new Vector3Int(editVoxel.size,editVoxel.height,editVoxel.size);

        cubes=new int[size.x,size.y,size.z];

        meshFilter = GetComponent<MeshFilter>();

        meshCollider = GetComponent<MeshCollider>();

        for (int x=0;x<size.x;x++){
            for (int z=0;z<size.z;z++){

                int maxHeight=calculateHeight(x,z);

                for (int y=0;y<=maxHeight;y++){
                    cubes[x,y,z]=1;
                }
            }
        }
        createMesh();
    }
    int calculateHeight(int x,int y){
        Vector2Int posObject=new Vector2Int(Mathf.RoundToInt(transform.position.x),Mathf.RoundToInt(transform.position.z));

        float bigDetail=Mathf.PerlinNoise((x+posObject.x)*bigDetailMultiplay-999,(y+posObject.y)*bigDetailMultiplay-999);
        int maxHeight=Mathf.FloorToInt(Mathf.PerlinNoise((x+posObject.x)*detailMultiplay-999,(y+posObject.y)*detailMultiplay-999)*detailHeightMultiplay*bigDetail);
        return maxHeight+mapHeight;
    }
    
    void createFace(Vector3 normal,int blockIndex){
        int[] trisA={
            index+0,index+1,index+2,
            index+2,index+1,index+3
        };
        Vector3[] normalsA={
            normal,
            normal,
            normal,
            normal
        };
        float posTexture=1f/textureMatrix;
        int line=(blockIndex-1)/textureMatrix;

        float vertical=(textureMatrix-1-line)/(float)textureMatrix;
        float horizontal=((blockIndex-1)-line*textureMatrix)/(float)textureMatrix;

        Vector2[] uvsA={
            new Vector2(posTexture+horizontal,vertical),
            new Vector2(posTexture+horizontal,posTexture+vertical),
            new Vector2(horizontal,vertical),
            new Vector2(horizontal,posTexture+vertical)
        };
                            
        tris.AddRange(trisA);
        normals.AddRange(normalsA);
        uvs.AddRange(uvsA);
        index+=4;
    }
    public void createMesh(){
        Mesh mesh=new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        List<Vector3> vertices=new List<Vector3>();

        for (int x=0;x<size.x;x++){
            for (int y=0;y<size.y;y++){
                for (int z=0;z<size.z;z++){
                    if(cubes[x,y,z]!=0){
                        
                        //right
                        bool right=false;
                        if(x!=size.x-1){
                            if(cubes[x+1,y,z]==0){
                                right=true;
                            }
                        }else if(rightM!=null){
                                right=rightM.cubes[0,y,z]==0;

                        }else{
                            
                            int maxHeight=calculateHeight(x+1,z);
                            right=y>maxHeight;

                        }
                        

                        //left
                        bool left=false;
                        if(x!=0){
                            if(cubes[x-1,y,z]==0){
                                left=true;
                            }
                        }else if(leftM!=null){
                            left=leftM.cubes[size.x-1,y,z]==0;

                        }else{
                            int maxHeight=calculateHeight(x-1,z);
                            left=y>maxHeight;
                        }

                        //up
                        bool up=false;
                        if(y!=size.y-1){
                            if(cubes[x,y+1,z]==0){
                                up=true;
                            }
                        }else{
                            up=true;
                        }
                        
                        //down
                        bool down=false;
                        if(y!=0){
                            if(cubes[x,y-1,z]==0){
                                down=true;
                            }
                        }

                        //forward
                        bool forward=false;
                        if(z!=size.z-1){
                            if(cubes[x,y,z+1]==0){
                                forward=true;
                            }
                        }else if(forwardM!=null){
                            forward=forwardM.cubes[x,y,0]==0;

                        }else{
                            int maxHeight=calculateHeight(x,z+1);
                            forward=y>maxHeight;
                        }

                        //back
                        bool back=false;
                        if(z!=0){
                            if(cubes[x,y,z-1]==0){
                                back=true;
                            }
                        }else if(backM!=null){
                            back=backM.cubes[x,y,size.z-1]==0;

                        }else{
                            int maxHeight=calculateHeight(x,z-1);
                            back=y>maxHeight;
                        }


                        if(right){
                            Vector3[] verticesA={
                                new Vector3(x+0.5f,y+0.5f,z-0.5f),
                                new Vector3(x+0.5f,y+0.5f,z+0.5f),
                                new Vector3(x+0.5f,y-0.5f,z-0.5f),
                                new Vector3(x+0.5f,y-0.5f,z+0.5f)
                            };
                            vertices.AddRange(verticesA);
                            createFace(Vector3.right,cubes[x,y,z]);
                        }
                        
                        if(left){
                            Vector3[] verticesA={
                                new Vector3(x-0.5f,y+0.5f,z+0.5f),
                                new Vector3(x-0.5f,y+0.5f,z-0.5f),
                                new Vector3(x-0.5f,y-0.5f,z+0.5f),
                                new Vector3(x-0.5f,y-0.5f,z-0.5f)
                            };
                            vertices.AddRange(verticesA);
                            createFace(Vector3.left,cubes[x,y,z]);
                        }
                        
                        if(up){
                            Vector3[] verticesA={
                                new Vector3(x+0.5f,y+0.5f,z+0.5f),
                                new Vector3(x+0.5f,y+0.5f,z-0.5f),
                                new Vector3(x-0.5f,y+0.5f,z+0.5f),
                                new Vector3(x-0.5f,y+0.5f,z-0.5f)
                            };
                            vertices.AddRange(verticesA);
                            createFace(Vector3.up,cubes[x,y,z]);
                        }
                        
                        if(down){
                                Vector3[] verticesA={
                                new Vector3(x+0.5f,y-0.5f,z-0.5f),
                                new Vector3(x+0.5f,y-0.5f,z+0.5f),
                                new Vector3(x-0.5f,y-0.5f,z-0.5f),
                                new Vector3(x-0.5f,y-0.5f,z+0.5f)
                            };
                            vertices.AddRange(verticesA);
                            createFace(Vector3.down,cubes[x,y,z]);
                        }
                        
                        if(forward){
                            Vector3[] verticesA={
                                new Vector3(x+0.5f,y-0.5f,z+0.5f),
                                new Vector3(x+0.5f,y+0.5f,z+0.5f),
                                new Vector3(x-0.5f,y-0.5f,z+0.5f),
                                new Vector3(x-0.5f,y+0.5f,z+0.5f)
                            };
                            vertices.AddRange(verticesA);
                            createFace(Vector3.forward,cubes[x,y,z]);
                        }
                        
                        
                        if(back){
                            Vector3[] verticesA={
                                new Vector3(x+0.5f,y+0.5f,z-0.5f),
                                new Vector3(x+0.5f,y-0.5f,z-0.5f),
                                new Vector3(x-0.5f,y+0.5f,z-0.5f),
                                new Vector3(x-0.5f,y-0.5f,z-0.5f)
                            };
                            vertices.AddRange(verticesA);
                            createFace(Vector3.forward,cubes[x,y,z]);
                        }
                    }
                }
            }
        }
        
        mesh.vertices=vertices.ToArray();
        mesh.normals=normals.ToArray();
        mesh.uv=uvs.ToArray();
        mesh.triangles=tris.ToArray();
        meshFilter.mesh=mesh;
        meshCollider.sharedMesh=mesh;

        index=0;
        tris.Clear();
        normals.Clear();
        uvs.Clear();
    }
}
