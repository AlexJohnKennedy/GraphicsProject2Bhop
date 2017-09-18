using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Will build a straight line arena FROM the parent gameobject, out in the Z direction, of width in X direction.
 * Dimensions and spacings will be based on passed parameters.
 * Materials used for each type of object will be based on passed parameters.
 */
public class LinearCourse : MonoBehaviour {

    // --- PARAMTERS FOR GENERATION ------------------------------------------------------------------------------------------------------------------------------------------------

    //Random seed
    public int seed;

    //Dimensions and spacing parameters.
    public float courseLength;      //How far the obstacle course will extend onwards.
    public float courseWidth;       //How wide the course will be.
    public float jumpHeight;        //Determines how high off of the ground a block should be such that the player can jump over or onto it.
    public float averageSpacing;    //Determines how close together the obstacles should be on average. Larger number means more spaced out objects!
    public float spacingVariation;  //How much we allowed to deviated from the average spacing randomly when spacing objects!
    public float angledChance;      //How often a generated obstacle should have some randomly applied YAW rotation. (0-1)
    public float angled45DegChance; //How often a generated obstacle, GIVEN it is being rotated, should be rotated exactly 45 degrees yaw in some direction. (0-1)
    public float angleVariation;

    //Weightings for how likely each type of obstacle. (adjusting these will change the nature of the course.)
    public float floorRaiseChance;      //Object with must be jumped onto and raises the the overall 'height' of the floor, since it forms floor.
    public float fullWallChance;        //Object which cannt be jumped over, and must be dodged (will not span full width)
    public float thinJumpBlockChance;   //Object which can be jumped over from floor level, and has small depth. (won't be likely to be stood on)
    public float fatJumpBlockChance;    //Object which can be jumped over from floor level, and has squarish depth. Can be jumped on to boost off of!
    public float tallJumpBlockChance;   //Object which is doubley high as a normal jump block; Can not be jumped over from ground level but CAN if you boost off of something else.
    public float duckBlockChance;       //Object which must be ducked under, OR jumped onto if you manage to boost of off other blocks!

    //Materials
    public Material floorMaterial;
    public Material floorMaterial2; //Alternate to facilitate swapping
    public Material boundaryWallMaterial;   //Material for the OUTER EDGE walls (NOT the obstacle walls!)
    public Material obstacleWallMaterial;
    public Material jumpBlockMaterial;
    public Material tallJumpBlockMaterial;
    public Material duckBlockMaterial;

    //Textures
    public Texture stonyGround;
    public Texture metalGrate;

    //Shader for obstacles
    public Shader sharedShader;

    // --- PRIVATE FIELDS ----------------------------------------------------------------------------------------------------------------------------------------------------------

    //Tracking current floor height
    private float currentFloorHeight;   //starts at zero, and increases by jump height when we generate a 'floor raise'. Lowers when a floor raise ends!
    private Stack<float> floorEndCoordStack;
    private float[] obstacleChanceBrackets;

    private static int FLOOR_RAISE_INDEX = 0;
    private static int FULL_WALL_INDEX = 1;
    private static int THIN_JUMP_BLOCK_INDEX = 2;
    private static int FAT_JUMP_BLOCK_INDEX = 3;
    private static int TALL_JUMP_BLOCK_INDEX = 4;
    private static int DUCK_BLOCK_INDEX = 5;

    private List<GameObject> children;

    // Use this for initialization
    void Start () {
        Random.InitState(seed);
        children = new List<GameObject>();  //each obstacle will have it's own child game object, with its own mesh and material!
        floorEndCoordStack = new Stack<float>();

        //First, normalize the chance waitings, so that they all add up to 1, and build up array or boundary values for deciding which random ranges go to which
        //choice.
        obstacleChanceBrackets = normalizeObstacleChances();

        //Okay, now create a list of values indcating the origin positions of each obstacle..
        List<float> obstacleSpacing = generateRandomObstacleSpacing();

        //Nice. We now have an origin Z axis position (Z axis refers to the length direction of the course) for each obstacle
        children.Add(buildFloorMesh());   //Build the floor mesh..

        //Loop through the spacing list. For each item, we should generate an obstacle!
        //We should also check if the current spacing exceeds the current floor end point, and pop it if it does!
        foreach (float originLength in obstacleSpacing) {
            while (floorEndCoordStack.Count > 0 && originLength > floorEndCoordStack.Peek()) {
                //The top value has been exceeded! This means some upper floor has ended.
                //we should pop and lower the floor height.
                floorEndCoordStack.Pop();
                currentFloorHeight -= jumpHeight;
            }

            //Okay, now that we have aquired the correct floor height, we should randomly generate an obstacle!
            children.Add(generateObstacle(originLength));
        }
    }

    public GameObject generateObstacle(float lenOffset) {
        //Randomly decide which type of obstacle to create here.
        float rand = Random.Range(0f, 1f);

        if (rand < obstacleChanceBrackets[FLOOR_RAISE_INDEX]) {
            return generateFloorRaise(lenOffset);
        } 
        else if (rand < obstacleChanceBrackets[FULL_WALL_INDEX]) {
            //return generateWall(lenOffset);
        }
        else if (rand < obstacleChanceBrackets[THIN_JUMP_BLOCK_INDEX]) {
            //return generateThinJump(lenOffset);
        }
        else if (rand < obstacleChanceBrackets[FAT_JUMP_BLOCK_INDEX]) {
            //return generateFatJump(lenOffset);
        }
        else if (rand < obstacleChanceBrackets[TALL_JUMP_BLOCK_INDEX]) {
            //return generateTallJump(lenOffset);
        }
        else {
            //return generateDuckBlock(lenOffset);
        }
        return null;    //To avoid annoying error warnings during development..
    }

    private float getRandomAngle() {
        bool angled = (Random.Range(0f, 1f) < angledChance);    //Figure out if should have an angled front face.

        float angleDeg = 0;
        if (angled) {
            bool angled45 = (Random.Range(0f, 1f) < angled45DegChance);
            float ran = Random.Range(-1f, 1f);
            if (angled45) {
                if (ran >= 0) {
                    angleDeg = 45;
                }
                else {
                    angleDeg = -45;
                }
            }
            else {
                angleDeg = angleVariation * ran;
            }
        }
        return angleDeg * Mathf.PI / 180f;
    }

    private GameObject generateFloorRaise(float lenOffset) {
        GameObject gameObj = new GameObject("Floor raise obstacle");     //Will be the newly created game object just for this section!
        
        //Setup new gameobject!
        gameObj.transform.SetParent(this.gameObject.transform);
        gameObj.layer = this.gameObject.layer;      //Set child gameObject to have the same layer as the parent gameobject (should be 'ground')
        MeshFilter filter = gameObj.AddComponent<MeshFilter>();
        MeshCollider collider = gameObj.AddComponent<MeshCollider>();
        MeshRenderer renderer = gameObj.AddComponent<MeshRenderer>();

        //Generate a random 'distance' over which the floor raise will last
        float span = Random.Range(1f, 20f) * averageSpacing;
        float endingLen = span + lenOffset;
        if (floorEndCoordStack.Count != 0 && endingLen > floorEndCoordStack.Peek()) {
            //We over stepped the floor below.. we should clamp this span.
            endingLen = floorEndCoordStack.Peek();
        }
        else if (endingLen > courseLength - 0.5 * averageSpacing) {
            //Oops, this is spanning too far!
            endingLen = (float)(courseLength - 0.5 * averageSpacing);
        }

        //Create the mesh and assign it to the gameobjects meshFilter and mesh collider
        Mesh mesh = createFloorRaiseMesh(lenOffset, endingLen);
        filter.mesh = mesh;
        collider.sharedMesh = mesh;

        //In order to keep track of the ending points of all the raised floors, we should push the floor change..
        floorEndCoordStack.Push(endingLen);
        this.currentFloorHeight += jumpHeight;

        //Setup the renderer
        renderer.material = this.floorMaterial;
        renderer.material.SetTexture("stone ground", stonyGround);

        //SWAP TEXTURES FOR NEXT GUY TO GET DIFFERENT MATERIAL
        Material tmp = floorMaterial;
        floorMaterial = floorMaterial2;
        floorMaterial2 = tmp;

        return gameObj;
    }
    private Mesh createFloorRaiseMesh(float lenOffset, float endingLen) {

        //Floor raises fill entire width.
        float angleRad = getRandomAngle();

        Mesh mesh = new Mesh();
        mesh.name = "floorRaise" + lenOffset;

        Vector3[] verts = new Vector3[8];   //Floor raise requires 8 verticies
        int[] trigs = new int[18];          //6 triangles => 18 indexing positions

        //Front face
        verts[0] = new Vector3(courseWidth / 2, currentFloorHeight, lenOffset - (courseWidth / 2) * Mathf.Tan(angleRad));
        verts[1] = new Vector3(-courseWidth / 2, currentFloorHeight, lenOffset + (courseWidth / 2) * Mathf.Tan(angleRad));
        verts[2] = new Vector3(-courseWidth / 2, currentFloorHeight + jumpHeight, lenOffset + (courseWidth / 2) * Mathf.Tan(angleRad));
        verts[3] = new Vector3(courseWidth / 2, currentFloorHeight + jumpHeight, lenOffset - (courseWidth / 2) * Mathf.Tan(angleRad));

        trigs[0] = 0;
        trigs[1] = 1;
        trigs[2] = 3;
        trigs[3] = 1;
        trigs[4] = 2;
        trigs[5] = 3;

        //Back face
        verts[4] = new Vector3(courseWidth / 2, currentFloorHeight, endingLen);
        verts[5] = new Vector3(-courseWidth / 2, currentFloorHeight, endingLen);
        verts[6] = new Vector3(-courseWidth / 2, currentFloorHeight + jumpHeight, endingLen);
        verts[7] = new Vector3(courseWidth / 2, currentFloorHeight + jumpHeight, endingLen);

        trigs[6] = 4;
        trigs[7] = 7;
        trigs[8] = 5;
        trigs[9] = 5;
        trigs[10] = 7;
        trigs[11] = 6;

        //Top Face triangle assignment
        trigs[12] = 3;
        trigs[13] = 2;
        trigs[14] = 7;
        trigs[15] = 2;
        trigs[16] = 6;
        trigs[17] = 7;

        mesh.vertices = verts;
        mesh.triangles = trigs;

        return mesh;
    }

    private GameObject buildFloorMesh() {
        //Simply look at the desired dimensions of the linear course and create a simple plane mesh at
        //height zero, originating at position zero and stretching to courseLength. It is the base floor!

        GameObject gameObj = new GameObject("Linear Floor");  //Will become a child gameobject for the floor mesh
        gameObj.transform.SetParent(this.gameObject.transform); //Set transform parent of this gameobject to be the one running this script.
        gameObj.layer = this.gameObject.layer;      //Set child gameObject to have the same layer as the parent gameobject (should be 'ground')

        MeshFilter filter = gameObj.AddComponent<MeshFilter>();
        MeshCollider collider = gameObj.AddComponent<MeshCollider>();
        MeshRenderer renderer = gameObj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.name = "bottomFloor";

        Vector3[] verts = new Vector3[4];
        verts[0] = new Vector3(courseWidth / 2, 0, 0);
        verts[1] = new Vector3(-courseWidth / 2, 0, 0);
        verts[2] = new Vector3(-courseWidth / 2, 0, courseLength);
        verts[3] = new Vector3(courseWidth / 2, 0, courseLength);
        mesh.vertices = verts;

        int[] triangles = { 0, 1, 2, 2, 3, 0 };
        mesh.triangles = triangles;

        //Create the mesh and assign it to the gameobjects meshFilter and mesh collider
        filter.mesh = mesh;
        collider.sharedMesh = mesh;

        //Setup the renderer
        renderer.material = this.floorMaterial;
        renderer.material.SetTexture("stone ground", stonyGround);

        //SWAP TEXTURES FOR NEXT GUY TO GET DIFFERENT MATERIAL
        Material tmp = floorMaterial;
        floorMaterial = floorMaterial2;
        floorMaterial2 = tmp;

        return gameObj;
    }
	
    private List<float> generateRandomObstacleSpacing() {
        //Each number generated represents a single obstacle of one of the above types STARTING at the indicated point!
        float currPos = averageSpacing;     //First item always goes the 'average distance' from the start, to ensure there is starting room.
        List<float> list = new List<float>();

        while (currPos + averageSpacing < courseLength) {
            //Okay, we have room for this position! Add it to the list!
            list.Add(currPos);

            //Generate the next position..
            currPos += averageSpacing + (Random.Range(-1f, 1f) * spacingVariation);
        }

        return list;
    }

    private float[] normalizeObstacleChances() {
        float total = floorRaiseChance + fullWallChance + thinJumpBlockChance + fatJumpBlockChance + tallJumpBlockChance + duckBlockChance;

        //Divide all of these values by the total, so that their sum adds up to 1.
        floorRaiseChance /= total;
        fullWallChance /= total;
        thinJumpBlockChance /= total;
        fatJumpBlockChance /= total;
        tallJumpBlockChance /= total;
        duckBlockChance /= total;

        float[] toRet = new float[6];

        total = floorRaiseChance;
        toRet[FLOOR_RAISE_INDEX] = total;
        total += fullWallChance;
        toRet[FULL_WALL_INDEX] = total;
        total += thinJumpBlockChance;
        toRet[THIN_JUMP_BLOCK_INDEX] = total;
        total += fatJumpBlockChance;
        toRet[FAT_JUMP_BLOCK_INDEX] = total;
        total += tallJumpBlockChance;
        toRet[THIN_JUMP_BLOCK_INDEX] = total;
        total += duckBlockChance;
        toRet[DUCK_BLOCK_INDEX] = total;

        return toRet;
    }

	// Update is called once per frame
	void Update () {
		
	}
}
