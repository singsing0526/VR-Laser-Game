using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

public class BuildModeManagement : MonoBehaviour
{
    public GameObject modeButton;
    private TMPro.TextMeshProUGUI modeButtonText, buildObjectName, buildObjectDetails;
    private bool isBuildObjectCreated, isBuildObjectAlreadyCreated;
    private GameObject buildObjectUI, currentHoldingBuildObject;
    public BuildObjectList buildObjectList;
    private int currentSelection;
    public List<SerializedClasses.SerializedBuildObject> builtObjects;
    [HideInInspector] public List<GameObject> sceneBuildObjects;
    private string savedLevel, path, currentLevel;
    [HideInInspector] public GameObject buildModeUI;
    [SerializeField] private Transform playerPosition;
    private PlayerController playerController;

    public static bool isBuildMode;
    public delegate void RequireExtraObjectInformationShown();
    public delegate void RequireExtraObjectInformationHidden();
    public event RequireExtraObjectInformationShown ShowInformationButtons;
    public event RequireExtraObjectInformationHidden HideInformationButtons;
    public GameObject extraInformationButtonPrefab, extraInformationUIPrefab, inputField, testPositionUIPrefab;
    public static bool isExtraInformationUIShowing;
    public AudioLibrary audioLibrary;

    private void Start()
    {
        currentLevel = PlayerPrefs.GetString("CurrentLevel");
        currentLevel = "1";
        path = Application.dataPath + "/StreamingAssets/LevelData/";
        Camera.main.transform.parent.position = new Vector3(0, 20, 0);
        Camera.main.transform.parent.rotation = Quaternion.Euler(0, 0, 0);
        isBuildMode = false;
        isBuildObjectCreated = false;
        //modeButtonText = modeButton.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        //buildModeUI = GameObject.FindGameObjectWithTag("BuildModeUI");
        //buildObjectName = buildModeUI.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>();
        //buildObjectUI = GameObject.FindGameObjectWithTag("BuildObjectUI");
        //buildObjectDetails = buildObjectUI.transform.GetChild(9).GetComponent<TMPro.TextMeshProUGUI>();
        playerController = Camera.main.transform.parent.parent.GetComponent<PlayerController>();
        SwitchModeOutlook();

        Debug.Log(currentLevel);
        LoadLevel(currentLevel);
    }

    IEnumerator LoadVRDevice(string newDevice, bool enable)
    {
        XRSettings.LoadDeviceByName(newDevice);
        yield return null;
        XRSettings.enabled = enable;
    }

    void EnableVR()
    {
        StartCoroutine(LoadVRDevice("OpenVR", true));
    }

    void DisableVR()
    {
        StartCoroutine(LoadVRDevice("", false));
    }

    private void Update() // Only Active In Build Mode
    {
        if (Input.GetMouseButtonDown(0) && currentHoldingBuildObject == null && isBuildObjectCreated == false)
        {
            if (IsPointerOverUIObject() == false)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                int index = -1;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.parent != null)
                    {
                        currentHoldingBuildObject = hit.transform.parent.gameObject;
                        index = SerializedClasses.SerializedBuildObject.FindIndexByInstanceID(currentHoldingBuildObject, ref sceneBuildObjects);
                    }
                    else
                    {
                        currentHoldingBuildObject = hit.transform.gameObject;
                        index = SerializedClasses.SerializedBuildObject.FindIndexByInstanceID(hit.transform.gameObject, ref sceneBuildObjects);
                    }
                    if (index != -1)
                    {
                        sceneBuildObjects.Remove(currentHoldingBuildObject);
                        currentSelection = SerializedClasses.SerializedBuildObject.GetBuildObjectID(index, ref builtObjects);
                        builtObjects.RemoveAt(index);
                        isBuildObjectAlreadyCreated = true;
                        enabled = false;
                        buildModeUI.SetActive(false);
                        buildObjectUI.SetActive(true);
                        isBuildObjectCreated = true;
                        modeButtonText.text = "Cancel";
                        SetBuildObjectText();
                        CallEvent("HideInformationButtons");
                    }
                }
            }
        }
    }

    public GameObject FindGameObjectWithSceneTag(string sceneTag)
    {
        for (int i = 0; i < builtObjects.Count; i++)
        {
            if (builtObjects[i].tag == sceneTag)
            {
                if (sceneBuildObjects[i].GetComponent<SceneObjectTag>().sceneTag == sceneTag)
                {
                    return sceneBuildObjects[i];
                }
            }
        }
        return null;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        Transform canvas = GameObject.Find("Canvas").transform;
        foreach (RaycastResult r in results)
        {
            bool isUIClick = r.gameObject.transform.IsChildOf(canvas);
            if (isUIClick)
            {
                return true;
            }
        }
        return false;
    }

    public void CallEvent(string name)
    {
        if (name == "ShowInformationButtons")
        {
            ShowInformationButtons?.Invoke();
        }
        else if (name == "HideInformationButtons")
        {
            HideInformationButtons?.Invoke();
        }
    }

    public void SwitchMode()
    {
        if (isExtraInformationUIShowing == false)
        {
            if (isBuildObjectCreated == false)
            {
                isBuildMode = !isBuildMode;
                SwitchModeOutlook();
            }
            else // Reselect From Cancel
            {
                enabled = true;
                if (currentSelection == 2)
                {
                    LaserInteraction laserInteraction = GameObject.FindGameObjectWithTag("LaserInteractionManager").GetComponent<LaserInteraction>();
                    laserInteraction.mainLaserData.Remove(currentHoldingBuildObject.GetComponent<LaserSender>().laserData);
                }

                sceneBuildObjects.Remove(currentHoldingBuildObject);
                isBuildObjectAlreadyCreated = false;
                Destroy(currentHoldingBuildObject);
                currentHoldingBuildObject = null;
                isBuildObjectCreated = false;
                modeButtonText.text = "Enter Play Mode";
                buildModeUI.SetActive(true);
                buildObjectUI.SetActive(false);
                CallEvent("ShowInformationButtons");
            }
        }
    }

    private void SwitchModeOutlook()
    {
        if (isBuildMode == true)
        {
            //DisableVR();
            enabled = true;
            buildModeUI.SetActive(true);
            currentSelection = 0;
            buildObjectName.text = buildObjectList.buildObjectList[currentSelection].name;
            buildObjectUI.SetActive(false);
            Camera.main.transform.parent.rotation = Quaternion.Euler(60, 0, 0);
            modeButtonText.text = "Enter Play Mode";
            ResetLevel();
            CallEvent("ShowInformationButtons");
            playerController.DisablePlayer();
            if (playerPosition != null)
            {
                playerPosition.gameObject.SetActive(true);
            }
        }
        else
        {
            //EnableVR();
            enabled = false;
            //buildModeUI.SetActive(false);
            //buildObjectName.text = null;
            //buildObjectUI.SetActive(false);
            //modeButtonText.text = "Enter Build Mode";
            CallEvent("HideInformationButtons");
            Camera.main.transform.parent.rotation = Quaternion.Euler(0, 0, 0);
            LaserInteraction.hasSceneUpdate = true;
            playerController.EnablePlayer();
            if (playerPosition != null)
            {
                Camera.main.transform.parent.position = playerPosition.position + Vector3.down * 0.8f;
                playerPosition.gameObject.SetActive(false);
            }
        }
    }

    public void PreviousBuildObject()
    {
        if (currentSelection - 1 >= 0)
        {
            currentSelection--;
            buildObjectName.text = buildObjectList.buildObjectList[currentSelection].name;
        }
    }

    public void NextBuildObject()
    {
        if (currentSelection + 1 < buildObjectList.buildObjectList.Count)
        {
            currentSelection++;
            buildObjectName.text = buildObjectList.buildObjectList[currentSelection].name;
        }
    }

    public void CreateBuildObject()
    {
        enabled = false;
        buildModeUI.SetActive(false);
        buildObjectUI.SetActive(true);
        isBuildObjectCreated = true;
        modeButtonText.text = "Cancel";
        currentHoldingBuildObject = Instantiate(buildObjectList.buildObjectList[currentSelection].model, Vector3.zero, Quaternion.Euler(0, 0, 0));
        SetBuildObjectText();
        CallEvent("HideInformationButtons");
    }

    private void SetBuildObjectText()
    {
        string tempDetails = "Object Details: \nPosition: ";
        tempDetails += currentHoldingBuildObject.transform.position.ToString();
        tempDetails += "\nRotation: ";
        tempDetails += currentHoldingBuildObject.transform.localEulerAngles.ToString();
        buildObjectDetails.text = tempDetails;
    }

    public void CameraUp()
    {
        Camera.main.transform.parent.position += Vector3.up * 0.5f;
    }
    public void CameraDown()
    {
        Camera.main.transform.parent.position += Vector3.down * 0.5f;
    }
    public void CameraLeft()
    {
        Camera.main.transform.parent.position += Vector3.left * 0.5f;
    }
    public void CameraRight()
    {
        Camera.main.transform.parent.position += Vector3.right * 0.5f;
    }
    public void CameraFront()
    {
        Camera.main.transform.parent.position += Vector3.forward * 0.5f;
    }
    public void CameraBack()
    {
        Camera.main.transform.parent.position += Vector3.back * 0.5f;
    }

    public void ObjectUp()
    {
        currentHoldingBuildObject.transform.position += Vector3.up * 0.1f;
        SetBuildObjectText();
    }
    public void ObjectDown()
    {
        currentHoldingBuildObject.transform.position += Vector3.down * 0.1f;
        SetBuildObjectText();
    }
    public void ObjectLeft()
    {
        currentHoldingBuildObject.transform.position += Vector3.left * 0.1f;
        SetBuildObjectText();
    }
    public void ObjectRight()
    {
        currentHoldingBuildObject.transform.position += Vector3.right * 0.1f;
        SetBuildObjectText();
    }
    public void ObjectFront()
    {
        currentHoldingBuildObject.transform.position += Vector3.forward * 0.1f;
        SetBuildObjectText();
    }
    public void ObjectBack()
    {
        currentHoldingBuildObject.transform.position += Vector3.back * 0.1f;
        SetBuildObjectText();
    }
    public void ObjectRotateX()
    {
        currentHoldingBuildObject.transform.Rotate(45, 0, 0);
        SetBuildObjectText();
    }
    public void ObjectRotateY()
    {
        currentHoldingBuildObject.transform.Rotate(0, 45, 0);
        SetBuildObjectText();
    }
    public void ObjectRotateZ()
    {
        currentHoldingBuildObject.transform.Rotate(0, 0, 45);
        SetBuildObjectText();
    }
    public void ObjectConfirm()
    {
        enabled = true;
        if (isBuildObjectAlreadyCreated == false)
        {
            builtObjects.Add(new SerializedClasses.SerializedBuildObject(new SerializedClasses.SerializedVector3(currentHoldingBuildObject.transform.position), new SerializedClasses.SerializedVector3(currentHoldingBuildObject.transform.localEulerAngles), buildObjectList.buildObjectList[currentSelection].ID, buildObjectList.buildObjectList[currentSelection].defaultInformation));
            currentHoldingBuildObject.name = buildObjectList.buildObjectList[currentSelection].name;
            sceneBuildObjects.Add(currentHoldingBuildObject);
            if (currentHoldingBuildObject.name == "VRPlayer")
            {
                playerPosition = currentHoldingBuildObject.transform;
            }
        }
        else
        {
            List<string> extraSettings = new List<string>();
            string tag = null;
            if (currentHoldingBuildObject.GetComponent<SceneObjectTag>() != null)
            {
                tag = currentHoldingBuildObject.GetComponent<SceneObjectTag>().sceneTag;
            }
            if (currentHoldingBuildObject.GetComponent<SubscribeExtraInformation>() != null)
            {
                extraSettings = currentHoldingBuildObject.GetComponent<SubscribeExtraInformation>().extraSettings;
            }
            builtObjects.Add(new SerializedClasses.SerializedBuildObject(new SerializedClasses.SerializedVector3(currentHoldingBuildObject.transform.position), new SerializedClasses.SerializedVector3(currentHoldingBuildObject.transform.localEulerAngles), currentSelection, extraSettings, tag));
            sceneBuildObjects.Add(currentHoldingBuildObject);
        }

        isBuildObjectAlreadyCreated = false;
        currentHoldingBuildObject = null;
        isBuildObjectCreated = false;
        modeButtonText.text = "Enter Play Mode";
        buildModeUI.SetActive(true);
        buildObjectUI.SetActive(false);
        CallEvent("ShowInformationButtons");
    }
    public void GoLevel()
    {
        LoadLevel(buildModeUI.transform.GetChild(5).GetComponent<TMPro.TMP_InputField>().text);
    }
    public void SaveLevel()
    {
        savedLevel = null;
        for (int i = 0; i < builtObjects.Count; i++)
        {
            savedLevel += JsonUtility.ToJson(builtObjects[i]) + "\n";
        }
        File.WriteAllText(path + currentLevel + ".txt", savedLevel);
    }

    public void ToNextLevel(string level)
    {
        AudioSource playerAudio = Camera.main.GetComponent<AudioSource>();
        playerAudio.clip = audioLibrary.audioClips[1];
        playerAudio.Play();
        if (File.Exists(path + level + ".txt")) {
            PlayerPrefs.SetString("CurrentLevel", level);
            LoadLevel(PlayerPrefs.GetString("CurrentLevel"));
        }
    }
    public void DeleteLasers(bool needDetrigger, bool needLaserDataDeleted)
    {
        LaserInteraction laserInteraction = GameObject.FindGameObjectWithTag("LaserInteractionManager").GetComponent<LaserInteraction>();
        for (int i = 0; i < laserInteraction.lasers.Count; i++)
        {
            Destroy(laserInteraction.lasers[i].gameObject);
        }
        if (needDetrigger)
        {
            laserInteraction.DetriggerAllLasers();
        }
        if (needLaserDataDeleted)
        {
            laserInteraction.mainLaserData.Clear();
        }
        laserInteraction.lasers.Clear();
        laserInteraction.triggeredObjects.Clear();
    }
    public void ResetLevel()
    {
        if (isBuildMode == true)
        {
            Vector3 spawnPos = playerPosition.position + Vector3.down * 0.2f;
            Camera.main.transform.parent.parent.position = spawnPos;
            Camera.main.transform.parent.position = spawnPos + Vector3.down * 1f;
        }
        DeleteLasers(true, false);
        Transform extraInformationButton = GameObject.FindGameObjectWithTag("ExtraInformation").transform;
        for (int i = 0; i < extraInformationButton.transform.childCount; i++)
        {
            Destroy(extraInformationButton.GetChild(i).gameObject);
        }
        if (sceneBuildObjects.Count > 0)
        {
            for (int i = 0; i < sceneBuildObjects.Count; i++)
            {
                sceneBuildObjects[i].transform.position = builtObjects[i].position.ToVector3();
                sceneBuildObjects[i].transform.rotation = Quaternion.Euler(builtObjects[i].rotation.ToVector3());
            }
        }
    }
    public void LoadLevel(string level)
    {
        DeleteLasers(false, true);
        builtObjects.Clear();
        for (int i = 0; i < sceneBuildObjects.Count; i++)
        {
            Destroy(sceneBuildObjects[i]);
        }
        sceneBuildObjects.Clear();
        playerPosition = null;

        if (File.Exists(path + level + ".txt"))
        {
            PlayerPrefs.SetString("CurrentLevel", level);
            string[] levelData = File.ReadAllLines(path + level + ".txt");

            if (levelData.Length > 0)
            {
                for (int i = 0; i < levelData.Length; i++)
                {
                    builtObjects.Add(JsonUtility.FromJson<SerializedClasses.SerializedBuildObject>(levelData[i]));
                    GameObject temp = Instantiate(buildObjectList.buildObjectList[builtObjects[i].ID].model, builtObjects[i].position.ToVector3(), Quaternion.Euler(builtObjects[i].rotation.ToVector3()));
                    if (builtObjects[i].stringList.Count > 0)
                    {
                        temp.GetComponent<SubscribeExtraInformation>().extraSettings = builtObjects[i].stringList;
                    }
                    if (builtObjects[i].tag != "" && builtObjects[i].tag != null)
                    {
                        temp.GetComponent<SceneObjectTag>().sceneTag = builtObjects[i - 1].tag;
                    }
                    if (playerPosition == null && buildObjectList.buildObjectList[builtObjects[i].ID].name == "VRPlayer")
                    {
                        playerPosition = temp.transform;
                    }
                    temp.name = buildObjectList.buildObjectList[builtObjects[i].ID].name;
                    sceneBuildObjects.Add(temp);
                }
            }
            if (isBuildMode == true)
                CallEvent("ShowInformationButtons");
        }
        else
        {
            Debug.Log("Level Not Found");
        }
        if (isBuildMode == false && playerPosition != null)
        {
            Vector3 spawnPos = playerPosition.position + Vector3.down * 0.2f;
            Camera.main.transform.parent.parent.position = spawnPos;
            Camera.main.transform.parent.position = spawnPos + Vector3.down * 1f;
            playerPosition.gameObject.SetActive(false);
        }
        else
        {
            Camera.main.transform.parent.parent.position = new Vector3(0, 20, 0);
            Camera.main.transform.parent.position = new Vector3(0, 20, 0);
        }
        currentLevel = level;
        LaserInteraction.hasSceneUpdate = true;
        Debug.Log("To Level " + currentLevel);
    }
}