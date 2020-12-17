using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController))]

public class editVoxel : MonoBehaviour
{
    public float renderDistance=100;
    public float editDistance=10;
    private List<voxelMap> mesh=new List<voxelMap>();
    private List<Vector2Int> positions =new List<Vector2Int>();
    public Material material;
    private GameObject map;
    bool add=true;
    public static int size= 20;
    public static int height= 100;
    private float charachterHeight;
    private float charachterRadius;

    void Awake(){
        CharacterController charachter=GetComponent<CharacterController>();
        charachterRadius=charachter.radius;
        charachterHeight=charachter.height-charachterRadius*2;

        map=new GameObject("map");
        
        map.AddComponent<voxelMap>();
        map.GetComponent<MeshRenderer>().material=material;
        positions.Add(Vector2Int.zero);
        mesh.Add(map.GetComponent<voxelMap>());
        
    }

    bool collisionControl(Vector3Int pos){
        bool contact=false;
        Vector3 posCharachter=transform.position;

        for(int i=0;i<2;i++){
            Vector3 spherePos= new Vector3(posCharachter.x,(posCharachter.y+charachterHeight*0.5f)-charachterHeight*i,posCharachter.z);
            
            //right
            if(spherePos.y<pos.y+0.5f+charachterRadius && spherePos.y>pos.y-0.5f-charachterRadius && spherePos.z>pos.z-0.5f-charachterRadius && spherePos.z<pos.z+0.5f+charachterRadius){
                if(spherePos.x<pos.x+0.5f+charachterRadius && spherePos.x>pos.x-0.5f-charachterRadius){
                    contact= true;
                }
            }
            //up
            if(spherePos.x<pos.x+0.5f+charachterRadius && spherePos.x>pos.x-0.5f-charachterRadius && spherePos.z>pos.z-0.5f-charachterRadius && spherePos.z<pos.z+0.5f+charachterRadius){
                if(spherePos.y<pos.y+0.5f+charachterRadius && spherePos.y>pos.y-0.5f-charachterRadius){
                    contact= true;
                }
            }
            //forward
            if(spherePos.x<pos.x+0.5f+charachterRadius && spherePos.x>pos.x-0.5f-charachterRadius && spherePos.y>pos.y-0.5f-charachterRadius && spherePos.y<pos.y+0.5f+charachterRadius){
                if(spherePos.z<pos.z+0.5f+charachterRadius && spherePos.z>pos.z-0.5f-charachterRadius){
                    contact= true;
                }
            }
        }
        return contact;
    }

    void mapControl(){
        Vector2 pos=new Vector2(transform.position.x,transform.position.z);
        Vector2Int posInt=new Vector2Int(Mathf.FloorToInt((transform.position.x/(float)size)),Mathf.FloorToInt((transform.position.z/(float)size)));
        int distanceInt=(int)(renderDistance/size);


        for(int x=0;x<distanceInt*2+1;x++){
            for(int y=0;y<distanceInt*2+1;y++){
                Vector2Int posCreated = new Vector2Int(x-distanceInt,y-distanceInt)+posInt;
                
                if(!positions.Contains(posCreated) && Vector2.Distance(pos-new Vector2(size*0.5f,size*0.5f),posCreated*size)<renderDistance){
                    GameObject created=Instantiate(map,new Vector3(posCreated.x,0,posCreated.y)*size,Quaternion.identity);
                    positions.Add(posCreated);
                    mesh.Add(created.GetComponent<voxelMap>());
                }
                
            }
        }

        for(int i=0;i<positions.Count;i++){
            float distance=Vector2.Distance(pos-new Vector2(size*0.5f,size*0.5f),positions[i]*size);
            if(distance<renderDistance){
                mesh[i].gameObject.SetActive(true);
                mesh[i].gameObject.GetComponent<MeshCollider>().enabled=distance<editDistance+size*0.5;

            }else{
                mesh[i].gameObject.SetActive(false);
            }
        }
    }

    void edit(Vector3Int pos){
        Vector2Int index=new Vector2Int(Mathf.FloorToInt((pos.x/(float)size)),Mathf.FloorToInt((pos.z/(float)size)));
        Vector3Int indexpos=new Vector3Int(pos.x-index.x*size,pos.y,pos.z-index.y*size);
        
        voxelMap meshMain=mesh[positions.IndexOf(index)];

        if(add&&pos.y!=height-1){
            if(!collisionControl(pos)){
                meshMain.cubes[indexpos.x,indexpos.y,indexpos.z]=1;
            }
        }else if(pos.y!=0){
            meshMain.cubes[indexpos.x,indexpos.y,indexpos.z]=0;
        }


        if(indexpos.x==size-1 && positions.Contains(index+Vector2Int.right)){

            voxelMap meshlook= mesh[positions.IndexOf(index+Vector2Int.right)];
            meshlook.leftM= meshMain;
            meshlook.createMesh();
            meshMain.rightM=meshlook;


        }else if(indexpos.x==0 && positions.Contains(index+Vector2Int.left)){

            voxelMap meshlook= mesh[positions.IndexOf(index+Vector2Int.left)];
            meshlook.rightM= meshMain;
            meshlook.createMesh();
            meshMain.leftM=meshlook;

        }

        if(indexpos.z==size-1 && positions.Contains(index+Vector2Int.up)){

            voxelMap meshlook= mesh[positions.IndexOf(index+Vector2Int.up)];
            meshlook.backM= meshMain;
            meshlook.createMesh();
            meshMain.forwardM=meshlook;

        }else if(indexpos.z==0 && positions.Contains(index+Vector2Int.down)){

            voxelMap meshlook= mesh[positions.IndexOf(index+Vector2Int.down)];
            meshlook.forwardM= meshMain;
            meshlook.createMesh();
            meshMain.backM=meshlook;

        }

        meshMain.createMesh();
    }
    
    void Update()
    {
        mapControl();

        if(Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)){
            add=Input.GetMouseButtonDown(1);

            RaycastHit hit;
            Ray ray=Camera.main.ScreenPointToRay(new Vector2(Screen.width*0.5f,Screen.height*0.5f));
            if(Physics.Raycast(ray,out hit,editDistance)){
                if(add){
                    hit.normal=hit.normal*-1;
                }
                //right
                if(hit.normal==Vector3.right){
                    edit(new Vector3Int(Mathf.FloorToInt(hit.point.x),Mathf.RoundToInt(hit.point.y),Mathf.RoundToInt(hit.point.z)));
                }
                //left
                if(hit.normal==Vector3.left){
                    edit(new Vector3Int(Mathf.CeilToInt(hit.point.x),Mathf.RoundToInt(hit.point.y),Mathf.RoundToInt(hit.point.z)));
                }
                //up
                if(hit.normal==Vector3.up){
                    edit(new Vector3Int(Mathf.RoundToInt(hit.point.x),Mathf.FloorToInt(hit.point.y),Mathf.RoundToInt(hit.point.z)));
                }
                //down
                if(hit.normal==Vector3.down){
                    edit(new Vector3Int(Mathf.RoundToInt(hit.point.x),Mathf.CeilToInt(hit.point.y),Mathf.RoundToInt(hit.point.z)));
                }
                //forward
                if(hit.normal==Vector3.forward){
                    edit(new Vector3Int(Mathf.RoundToInt(hit.point.x),Mathf.RoundToInt(hit.point.y),Mathf.FloorToInt(hit.point.z)));
                }
                //back
                if(hit.normal==Vector3.back){
                    edit(new Vector3Int(Mathf.RoundToInt(hit.point.x),Mathf.RoundToInt(hit.point.y),Mathf.CeilToInt(hit.point.z)));
                }
            }
        }
    }
}
