using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum CurveType { 
    Start,
    Wall,
    Posts,
    Ramp,
    RampCurve,
    RampUp
}

public class ProceduralLevelManager : MonoBehaviour {

    [SerializeField]

    private Transform playerTransform;

    [Header("Procedural Generation Code")]

    [SerializeField]
    private float segmentSizes;

    [SerializeField]
    private GameObject startingPlatformPrefab;

    [SerializeField]
    private GameObject wallPlatformPrefab;

    [SerializeField]
    private GameObject wallPostsPrefab;

    [SerializeField]
    private GameObject verticalCylinderPrefab;

    [SerializeField]
    private GameObject rampHorizontalPrefab;

    [SerializeField]
    private GameObject rampCurveHorizontalPrefab;

    [SerializeField]
    private GameObject rampCurveVerticalPrefab;

    private Dictionary<CurveType, GameObject> prefabs = new Dictionary<CurveType, GameObject> ();

    private Vector3 mapPosition;
    private Vector3 mapDirection;
    private CurveType currentType = CurveType.Start;

    private CurveType[] nonStartTypes = { CurveType.Wall, CurveType.Posts };

    private List<GameObject> addedPlatforms = new List<GameObject>();

    private void Start() {

        prefabs.Add(CurveType.Start, startingPlatformPrefab);
        prefabs.Add(CurveType.Wall, wallPlatformPrefab);
        prefabs.Add(CurveType.Posts, wallPostsPrefab);
        // prefabs.Add(CurveType.Ramp, rampHorizontalPrefab);
        // prefabs.Add(CurveType.RampCurve, rampCurveHorizontalPrefab);
        // prefabs.Add(CurveType.RampUp, rampCurveVerticalPrefab);

        mapPosition = Vector3.zero;

        mapDirection = new Vector3(1, 0, 0);

        // Place first five steps
        for (int i = 0; i < 5; i++) {
            PlacePlatformAndStep();        
        }
    }

    private void PlacePlatformAndStep() {
        float platformRotation = 0;

        if (mapDirection == Vector3.right) {
            platformRotation = 90;
        }

        // TODO: Change map direction
        if (Random.value < 0.1) {
            if (mapDirection.x > mapDirection.z) {
                mapDirection = new Vector3(0, 0, 1);

                platformRotation -= 45;
            }
            else {
                mapDirection = new Vector3(1, 0, 0);

                platformRotation += 45;
            }
        }

        
        // platform.transform.localScale = Vector3.one * segmentSizes * 0.5f;
        GameObject platform = Instantiate(prefabs[currentType], transform);
        
        platform.transform.position = mapPosition;

        addedPlatforms.Add(platform);


        platform.transform.rotation = Quaternion.Euler(0, platformRotation, 0);

        // Switch type
        currentType = nonStartTypes[Random.Range(0, nonStartTypes.Length)];


        // Step position
        mapPosition += mapDirection * segmentSizes;

    }

    private void Update() {
        // Check if player is near position

        if (Vector3.Distance(playerTransform.position, mapPosition) < segmentSizes * 5.0) {
            PlacePlatformAndStep();
        }
    }
}