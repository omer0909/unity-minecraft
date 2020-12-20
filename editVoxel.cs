using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(CharacterController))]

public class editVoxel : MonoBehaviour
{
    public static bool open;

    public List<mapData> mapDatas=new List<mapData>();
    public int textureMatix=3;
    public int blockIndex=0;
    public float renderDistance=100;
    public float editDistance=10;
    private List<voxelMap> mesh=new List<voxelMap>();
    private List<Vector2Int> positions =new List<Vector2Int>();
    public Material material;
    private GameObject map;
    bool add=true;
    public int size= 20;
    public int height= 100;
    private float charachterHeight;
    private float charachterRadius;
    private Camera cameraT;

    void Awake(){
        cameraT=Camera.main;
        CharacterController charachter=GetComponent<CharacterController>();
        charachterRadius=charachter.radius;
        charachterHeight=charachter.height-charachterRadius*2;
        if(open){
            load();
            open=false;
        }

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

        int addIndex=(add)?blockIndex+1:0;

        if(pos.y!=height-1 && pos.y!=0){
            if(!collisionControl(pos+new Vector3Int(1,0,1)) || !add){
                meshMain.cubes[indexpos.x+1,indexpos.y,indexpos.z+1]=addIndex;
                meshMain.createMesh();

                if(indexpos.x==size-1){
                    voxelMap meshlook= mesh[positions.IndexOf(index+Vector2Int.right)];
                    meshlook.cubes[0,indexpos.y,indexpos.z+1]=addIndex;
                    meshlook.createMesh();
                }else if(indexpos.x==0){
                    voxelMap meshlook= mesh[positions.IndexOf(index+Vector2Int.left)];
                    meshlook.cubes[size+1,indexpos.y,indexpos.z+1]=addIndex;
                    meshlook.createMesh();
                }
                if(indexpos.z==size-1){
                    voxelMap meshlook= mesh[positions.IndexOf(index+Vector2Int.up)];
                    meshlook.cubes[indexpos.x+1,indexpos.y,0]=addIndex;
                    meshlook.createMesh();
                }else if(indexpos.z==0){
                    voxelMap meshlook= mesh[positions.IndexOf(index+Vector2Int.down)];
                    meshlook.cubes[indexpos.x+1,indexpos.y,size+1]=addIndex;
                    meshlook.createMesh();
                }

            }
        }
    }

    void Update()
    {
        example();

        mapControl();

        if(Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)){
            add=Input.GetMouseButtonDown(1);

            RaycastHit hit;
            Ray ray=cameraT.ScreenPointToRay(new Vector2(Screen.width*0.5f,Screen.height*0.5f));
            if(Physics.Raycast(ray,out hit,editDistance)){
                if(add){
                    hit.normal=hit.normal*-1;
                }
                //right
                if(hit.normal==Vector3.right){
                    edit(new Vector3Int(Mathf.FloorToInt(hit.point.x-1),Mathf.RoundToInt(hit.point.y),Mathf.RoundToInt(hit.point.z-1)));
                }
                //left
                if(hit.normal==Vector3.left){
                    edit(new Vector3Int(Mathf.CeilToInt(hit.point.x-1),Mathf.RoundToInt(hit.point.y),Mathf.RoundToInt(hit.point.z-1)));
                }
                //up
                if(hit.normal==Vector3.up){
                    edit(new Vector3Int(Mathf.RoundToInt(hit.point.x-1),Mathf.FloorToInt(hit.point.y),Mathf.RoundToInt(hit.point.z-1)));
                }
                //down
                if(hit.normal==Vector3.down){
                    edit(new Vector3Int(Mathf.RoundToInt(hit.point.x-1),Mathf.CeilToInt(hit.point.y),Mathf.RoundToInt(hit.point.z-1)));
                }
                //forward
                if(hit.normal==Vector3.forward){
                    edit(new Vector3Int(Mathf.RoundToInt(hit.point.x-1),Mathf.RoundToInt(hit.point.y),Mathf.FloorToInt(hit.point.z-1)));
                }
                //back
                if(hit.normal==Vector3.back){
                    edit(new Vector3Int(Mathf.RoundToInt(hit.point.x-1),Mathf.RoundToInt(hit.point.y),Mathf.CeilToInt(hit.point.z-1)));
                }
            }
        }
    }
    
    void load(){
        gameData gameDataV=JsonUtility.FromJson<gameData>(File.ReadAllText(Application.persistentDataPath+"/saved_file.json"));
        int index=0;
        
        for(int a=0;a<gameDataV.objects.Length;a++){
            mapDatas.Add(new mapData());
            mapDatas[mapDatas.Count-1].pos=new Vector2Int(gameDataV.objects[a].x, gameDataV.objects[a].y);
            List<Vector3Int> posBlocks=new List<Vector3Int>();
            List<int> blockIndex=new List<int>();

            for(int b=0;b<gameDataV.objects[a].z;b++){
                posBlocks.Add(gameDataV.pos[index]);
                blockIndex.Add(gameDataV.blockIndex[index]);
                
                index++;
            }
            mapDatas[mapDatas.Count-1].blockIndex=blockIndex.ToArray();
            mapDatas[mapDatas.Count-1].savedBlocks=posBlocks.ToArray();
        }
        cameraT.transform.rotation=gameDataV.camera;
        transform.position=gameDataV.character;
    }
    
    void save(){
        List<int> blockIndex=new List<int>();
        List<Vector3Int> posBlocks=new List<Vector3Int>();
        List<Vector3Int> objectIndex=new List<Vector3Int>();

        gameData data=new gameData();
        data.character=transform.position;
        data.camera=cameraT.transform.rotation;

        for(int i=0;i<mesh.Count;i++){

            Vector2Int posObject=new Vector2Int(Mathf.RoundToInt(mesh[i].transform.position.x),Mathf.RoundToInt(mesh[i].transform.position.z));
            int legenth=0;
            for (int x=0;x<size+2;x++){
                for (int z=0;z<size+2;z++){

                    float bigDetail=Mathf.PerlinNoise((x+posObject.x)*0.005f-9999,(z+posObject.y)*0.005f-9999);
                    int maxHeight=Mathf.FloorToInt(Mathf.PerlinNoise((x+posObject.x)*0.025f-9999,(z+posObject.y)*0.025f-9999)*40*bigDetail);

                    for (int y=0;y<height;y++){
                        if(!(y<=maxHeight+10 && mesh[i].cubes[x,y,z]==1) && !(y>maxHeight+10 && mesh[i].cubes[x,y,z]==0)){
                            posBlocks.Add(new Vector3Int(x,y,z));
                            blockIndex.Add(mesh[i].cubes[x,y,z]);
                            legenth++;
                        }
                    }
                }
            }
            if(legenth!=0){
                objectIndex.Add(new Vector3Int(posObject.x,posObject.y,legenth));
            }
        }
        data.blockIndex=blockIndex.ToArray();
        data.pos=posBlocks.ToArray();
        data.objects=objectIndex.ToArray();

        string jsonData=JsonUtility.ToJson(data,true);
        File.WriteAllText(Application.persistentDataPath+"/saved_file.json",jsonData);
    }
    public class gameData
    {
        //xpos,zpos,legenth
        public Vector3Int[] objects;
        public Vector3Int[] pos;
        public int[] blockIndex;
        public Vector3 character;
        public Quaternion camera;
    }
    public class mapData
    {
        public Vector2Int pos;
        public Vector3Int[] savedBlocks;
        public int[] blockIndex; 
    }
    void example(){

        if(Input.GetKey(KeyCode.LeftControl)){
            if(Input.GetKeyDown(KeyCode.U)){
                save();
            }
            if(Input.GetKeyDown(KeyCode.J)){
                open=true;
                SceneManager.LoadScene(0);
            }
            if(Input.GetKeyDown(KeyCode.H)){
                SceneManager.LoadScene(0);
            }
        }
        

        if(Input.GetKeyDown(KeyCode.Keypad1)){
            blockIndex=0;
        }
        if(Input.GetKeyDown(KeyCode.Keypad2)){
            blockIndex=1;
        }
        if(Input.GetKeyDown(KeyCode.Keypad3)){
            blockIndex=2;
        }
        if(Input.GetKeyDown(KeyCode.Keypad4)){
            blockIndex=3;
        }
        if(Input.GetKeyDown(KeyCode.Keypad5)){
            blockIndex=4;
        }
        if(Input.GetKeyDown(KeyCode.Keypad6)){
            blockIndex=5;
        }
        if(Input.GetKeyDown(KeyCode.Keypad7)){
            blockIndex=6;
        }
        if(Input.GetKeyDown(KeyCode.Keypad8)){
            blockIndex=7;
        }
        if(Input.GetKeyDown(KeyCode.Keypad9)){
            blockIndex=8;
        }
    }
}

