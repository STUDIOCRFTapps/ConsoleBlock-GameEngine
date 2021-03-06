﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour {

    public Rigidbody playerRigidbody;
    public PlayerController controller;
    public bool IsUICurrentlyOpened = false;
    public bool ForcedMouvementFreeze;
    public bool ForcedCompleteMotionFreeze;

    public UIManager uiManager;
    public BuildingManager buildingManager;
    public WorldLoader loader;

    public bool SpecificTypeModeActive = false;
    public SpecificTypeModes SpecificTypeMode;
    public Image SpecificTypeModeOverlay;
    public InventoryUI inventory;

    WInteractable linksource;
    WInteractable linksourcePower;
    public ProjectileLauncherScript projectileLauncher;

    public bool EnableInfintePPSFilling = true;

    public bool IsPaused = false;
    bool WasOpenedBefore = false;
    bool BuildingModeOn = false;

    void Start () {
        buildingManager.player = this;
    }

    void Update () {
        if(InputControl.GetInputDown(InputControl.InputType.Close)) {
            IsPaused = !IsPaused;
            if(IsPaused) {
                WasOpenedBefore = IsUICurrentlyOpened;
                Time.timeScale = 0.0f;
                uiManager.PauseMenu.gameObject.SetActive(true);
                uiManager.PauseMenuDropdowns[0].value = PlayerPrefs.GetInt("KeyboardType", 0);
                uiManager.PauseMenuDropdowns[1].value = PlayerPrefs.GetInt("BuildingKeyType", 0);
            } else {
                IsUICurrentlyOpened = WasOpenedBefore;
                Time.timeScale = 1.0f;
                uiManager.PauseMenu.gameObject.SetActive(false);
            }
        }

        if(IsPaused) {
            IsUICurrentlyOpened = true;
            PlayerPrefs.SetInt("KeyboardType", uiManager.PauseMenuDropdowns[0].value);
            PlayerPrefs.SetInt("BuildingKeyType", uiManager.PauseMenuDropdowns[1].value);
        }

        if(PlayerPrefs.GetInt("BuildingKeyType", 0) == 0) {
            BuildingModeOn = InputControl.GetInput(InputControl.InputType.BuildingMode);
        } else {
            if(!IsUICurrentlyOpened && InputControl.GetInputDown(InputControl.InputType.BuildingMode)) {
                BuildingModeOn = !BuildingModeOn;
            }
        }

        if(Input.GetKeyDown(KeyCode.G)) {
            projectileLauncher.gameObject.SetActive(!projectileLauncher.gameObject.activeSelf);
        }
        if(ForcedMouvementFreeze) {
            controller.FreezeMouvement = true;
            ForcedMouvementFreeze = false;

        } else {
            controller.FreezeMouvement = false;
        }
        if(ForcedCompleteMotionFreeze) {
            playerRigidbody.isKinematic = true;
            ForcedCompleteMotionFreeze = false;

        } else {
            playerRigidbody.isKinematic = false;
        }
        if(IsUICurrentlyOpened) {
            //Hide/Show building widget/inventory ?
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            controller.FreezeMouvement = true;
            controller.FreezeCamera = true;
            return;
        } else {
            controller.FreezeCamera = false;
        }

        if(BuildingModeOn) {
            if(InputControl.GetInputDown(InputControl.InputType.BuildingChangeRotation)) {
                uiManager.EditWidgetValue("Build_BlockRotation", -uiManager.GetWidgetValue("Build_BlockRotation"));
                buildingManager.CurrentRotation = uiManager.GetWidgetValue("Build_BlockRotation");
            } else if(InputControl.GetInput(InputControl.InputType.BuildingChangeRotation)) {
                if(Input.mouseScrollDelta.y > 0.4) {
                    uiManager.EditWidgetValue("Build_BlockRotation", 1);
                    buildingManager.CurrentRotation = uiManager.GetWidgetValue("Build_BlockRotation");
                }
                if(Input.mouseScrollDelta.y < -0.4) {
                    uiManager.EditWidgetValue("Build_BlockRotation", -1);
                    buildingManager.CurrentRotation = uiManager.GetWidgetValue("Build_BlockRotation");
                }
            } else {
                if(Input.mouseScrollDelta.y > 0.4) {
                    uiManager.EditWidgetValue("Build_BlockType", 1);
                    buildingManager.BuildingBlockType = (BuildingBlock.BlockType)uiManager.GetWidgetValue("Build_BlockType");
                }
                if(Input.mouseScrollDelta.y < -0.4) {
                    uiManager.EditWidgetValue("Build_BlockType", -1);
                    buildingManager.BuildingBlockType = (BuildingBlock.BlockType)uiManager.GetWidgetValue("Build_BlockType");
                }
                if(buildingManager.BuildingBlockType == BuildingBlock.BlockType.Objects) {
                    uiManager.widget[0].Display.sprite = buildingManager.Blocks[buildingManager.CurrentBlock].Icon;
                    buildingManager.PlaceHolderObject.GetChild(0).transform.localPosition = buildingManager.Blocks[buildingManager.CurrentBlock].CustomPlaceHolderPosition;
                    buildingManager.PlaceHolderObject.GetChild(0).transform.localScale = buildingManager.Blocks[buildingManager.CurrentBlock].CustomPlaceHolderScale;
                    buildingManager.PlaceHolderObject.GetChild(0).transform.localEulerAngles = buildingManager.Blocks[buildingManager.CurrentBlock].CustomPlaceHolderRotation;
                }
            }
            if(InputControl.GetInputDown(InputControl.InputType.BuildingInventory)) {
                if(buildingManager.BuildingBlockType == BuildingBlock.BlockType.Objects) {
                    buildingManager.CurrentInventory = 0;
                } else if(buildingManager.BuildingBlockType == BuildingBlock.BlockType.Cube ||
                        buildingManager.BuildingBlockType == BuildingBlock.BlockType.Floor ||
                        buildingManager.BuildingBlockType == BuildingBlock.BlockType.Wall ||
                        buildingManager.BuildingBlockType == BuildingBlock.BlockType.Stair) {
                    buildingManager.CurrentInventory = 1;
                }
                inventory.gameObject.SetActive(!inventory.gameObject.activeSelf);
            }
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if(controller.enabled == false) {
            controller.enabled = true;
        }

        buildingManager.PlaceHolderClear();
        if(!SpecificTypeModeActive && SpecificTypeModeOverlay.enabled) {
            SpecificTypeModeOverlay.enabled = false;
        } else if(SpecificTypeModeActive && !SpecificTypeModeOverlay.enabled) {
            SpecificTypeModeOverlay.enabled = true;
        }

        if(SpecificTypeModeActive) {
            if(SpecificTypeMode == SpecificTypeModes.Link) {
                SpecificTypeModeOverlay.color = new Color(0.8f, 1f, 0.8f, 0.6f);
            }
            if(SpecificTypeMode == SpecificTypeModes.PowerComsumeLink) {
                SpecificTypeModeOverlay.color = new Color(1f, 0f, 0.55f, 0.6f);
            }
            if(SpecificTypeMode == SpecificTypeModes.PowerGenerationLink) {
                SpecificTypeModeOverlay.color = new Color(1f, 0f, 0.25f, 0.6f);
            }
        }

        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit)) {
            if(hit.collider.GetComponent<WInteractableCaller>() != null && !BuildingModeOn) {
                if(hit.collider.GetComponent<WInteractableCaller>().callType == CallType.Transmition) {
                    if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress) && SpecificTypeModeActive && SpecificTypeMode == SpecificTypeModes.Link) {
                        WInteractable interactable = hit.collider.GetComponent<WInteractableCaller>().Call();
                        if(interactable != null && interactable != linksource) {
                            if(interactable.transmitter.sources.Contains(linksource)) {
                                interactable.transmitter.sources.Remove(linksource);
                                if(linksource.transmitter.sources.Contains(interactable)) {
                                    linksource.transmitter.sources.Remove(interactable);
                                }
                            } else {
                                if(!linksource.transmitter.sources.Contains(interactable)) {
                                    linksource.transmitter.sources.Add(interactable);
                                    interactable.transmitter.sources.Add(linksource);
                                }

                            }
                        }
                        SpecificTypeModeActive = false;
                    } else if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress) && !SpecificTypeModeActive) {
                        linksource = hit.collider.GetComponent<WInteractableCaller>().Call();
                        if(linksource != null) {
                            SpecificTypeMode = SpecificTypeModes.Link;
                            SpecificTypeModeActive = true;
                        }
                    }
                } else if(hit.collider.GetComponent<WInteractableCaller>().callType == CallType.PowerInput) {
                    if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress) && SpecificTypeModeActive && SpecificTypeMode == SpecificTypeModes.PowerGenerationLink) {
                        WInteractable interactable = hit.collider.GetComponent<WInteractableCaller>().Call();
                        if(interactable.PowerSource == linksourcePower) {
                            interactable.PowerSource = null;
                        } else {
                            interactable.PowerSource = linksourcePower;
                        }
                        SpecificTypeModeActive = false;
                    } else if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress) && SpecificTypeModeActive && SpecificTypeMode == SpecificTypeModes.PowerComsumeLink) {
                        linksourcePower = null;
                        SpecificTypeModeActive = false;
                    } else if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress) && !SpecificTypeModeActive) {
                        linksourcePower = hit.collider.GetComponent<WInteractableCaller>().Call();
                        if(linksourcePower != null) {
                            SpecificTypeMode = SpecificTypeModes.PowerComsumeLink;
                            SpecificTypeModeActive = true;
                        }
                    }
                } else if(hit.collider.GetComponent<WInteractableCaller>().callType == CallType.PowerOutput) {
                    if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress) && SpecificTypeModeActive && SpecificTypeMode == SpecificTypeModes.PowerGenerationLink) {
                        linksourcePower = null;
                        SpecificTypeModeActive = false;
                        SpecificTypeModeActive = false;
                    } else if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress) && SpecificTypeModeActive && SpecificTypeMode == SpecificTypeModes.PowerComsumeLink) {
                        WInteractable interactable = hit.collider.GetComponent<WInteractableCaller>().Call();
                        if(linksourcePower.PowerSource == null) {
                            linksourcePower.PowerSource = interactable;
                        } else {
                            if(interactable != linksourcePower.PowerSource) {
                                linksourcePower.PowerSource = interactable;
                            } else {
                                linksourcePower.PowerSource = null;
                            }
                        }
                        SpecificTypeModeActive = false;
                    } else if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress) && !SpecificTypeModeActive) {
                        linksourcePower = hit.collider.GetComponent<WInteractableCaller>().Call();
                        if(linksourcePower != null) {
                            SpecificTypeMode = SpecificTypeModes.PowerGenerationLink;
                            SpecificTypeModeActive = true;
                        }
                    }
                } else if(hit.collider.GetComponent<WInteractableCaller>().callType == CallType.TactileInteraction) {
                    hit.collider.GetComponent<WInteractableCaller>().TactileCall(
                        hit.textureCoord,
                        InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress),
                        InputControl.GetInput(InputControl.InputType.MouseSecondairyPress),
                        InputControl.GetInputUp(InputControl.InputType.MouseSecondairyPress)
                    );
                } else if(hit.collider.GetComponent<WInteractableCaller>().callType == CallType.Interaction) {
                    if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress)) {
                        hit.collider.GetComponent<WInteractableCaller>().Call(this);
                    }
                } else if(hit.collider.GetComponent<WInteractableCaller>().callType == CallType.PointedAt) {
                    hit.collider.GetComponent<WInteractableCaller>().PointedAtCall(this);
                }
            } else if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress) && !BuildingModeOn) {
                if(hit.collider.tag == "Interactable") {
                    WInteractable interactable = hit.collider.GetComponent<WInteractableCaller>().Call();
                    if(interactable != null) {
                        if(hit.collider.GetComponent<WInteractableCaller>().callType == CallType.Transmition) {
                            SpecificTypeModeActive = true;
                            SpecificTypeMode = SpecificTypeModes.Link;
                            linksource = interactable;
                        }
                    }
                }
            } else if(BuildingModeOn) {
                buildingManager.PlaceHolderDisplay(hit);

                if(InputControl.GetInputDown(InputControl.InputType.MouseSecondairyPress)) {
                    if(hit.collider.tag == "BuildingBlock" && hit.collider.GetComponent<BuildingBlock>() != null) {
                        buildingManager.PlaceObject(hit);
                    }
                }
                if(InputControl.GetInputDown(InputControl.InputType.MouseMainPress)) {
                    if(hit.collider.tag == "BuildingBlock" && hit.collider.GetComponent<BuildingBlock>() != null) {
                        buildingManager.DestroyObject(hit);
                    } else if(hit.collider.tag == "Interactable" && hit.collider.GetComponent<WObject>() != null) {
                        buildingManager.DestroyObject(hit);
                    }
                }
            }
        }
    }
    
    public void OpenUI () {
        IsUICurrentlyOpened = true;
    }

    public void CloseUI () {
        IsUICurrentlyOpened = false;
    }

    public enum SpecificTypeModes {
        Link,
        PowerComsumeLink,
        PowerGenerationLink
    }
}
