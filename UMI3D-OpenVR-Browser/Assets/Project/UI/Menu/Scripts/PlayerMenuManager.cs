/*
Copyright 2019 - 2021 Inetum
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
    http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using umi3d.cdk.menu.interaction;
using umi3d.cdk.interaction;

using QuestBrowser.Interactions;
using umi3d.cdk.collaboration;
using Valve.VR;

public class PlayerMenuManager : MonoBehaviour
{
    #region Fields

    public static List<PlayerMenuManager> instances { get; protected set; } = new List<PlayerMenuManager>();

    public static PlayerMenuManager FindInstanceAssociatedToController(SteamVR_Input_Sources controllerType)
    {
        foreach(var i in instances)
        {
            if (i.controllerType == controllerType)
                return i;
        }
        return null;
    }

    public static PlayerMenuManager FindInstanceAssociatedToController(AbstractController controller)
    {
        foreach (var i in instances)
        {
            if (i.controller == controller)
                return i;
        }
        return null;
    }

    [Header("Controller")]
    public OpenVRController controller;
    public SteamVR_Input_Sources controllerType;

    [Header("Toolbox and Tool Menu")]
    public MenuAsset playerMainMenu;
    public MenuDisplayManager toolboxesAndToolsMenuDisplay;
    public HemicircularPathConstraint hemicircularPathConstraintToolboxMenu;

    [Header("Tool Parameter Menu")]
    public MenuDisplayManager toolParametersMenuDisplay;
    public HemicircularPathConstraint hemicircularPathConstraintToolParametersMenu;
    
    [Header("Buttons Renderers")]
    public MeshRenderer backButtonRenderer;
    public MeshRenderer menuButtonRenderer;
    public MeshRenderer quitReleaseButtonRenderer;
    public MeshRenderer navigateLeftRenderer;
    public MeshRenderer navigateRightRenderer;

    [Header("Settings buttons renderers")]
    public MeshRenderer avatarButtonRenderer;
    public MeshRenderer microButtonRenderer;
    public MeshRenderer screenShareRenderer;
    public MeshRenderer soundButtonRenderer;

    [Header("Panels")]
    public GameObject navigationPanel;
    public GameObject parameterPanel;
    public GameObject toolPanel;
    public GameObject settingsPanel;
    public GameObject appInfosPanel;


    [System.Serializable]
    public class MaterialGroup
    {
        public int materialIndex = 0;
        public Material disabled;
        public Material enabled;
        public Material hoverred;
        public Material pressed;
    }
    [Header("Materials")]
    [SerializeField] public MaterialGroup backButtonMaterials;
    [SerializeField] public MaterialGroup menuButtonMaterials;
    [SerializeField] public MaterialGroup releaseButtonMaterials;
    [SerializeField] public MaterialGroup quitButtonMaterials;
    [SerializeField] public MaterialGroup navigateLeftMaterials;
    [SerializeField] public MaterialGroup navigateRightMaterials;
    [SerializeField] public MaterialGroup avatarBtnOffMaterials;
    [SerializeField] public MaterialGroup avatarBtnOnMaterials;
    [SerializeField] public MaterialGroup microBtnOnMaterials;
    [SerializeField] public MaterialGroup microBtnOffMaterials;
    [SerializeField] public MaterialGroup screenShareOnBtnMaterials;
    [SerializeField] public MaterialGroup screenShareOffBtnMaterials;
    [SerializeField] public MaterialGroup soundOnBtnMaterials;
    [SerializeField] public MaterialGroup soundOffBtnMaterials;

    [Header("Others")]
    [SerializeField]
    private UnityEngine.UI.Text toolAndToolboxesLabel;
    public Material baseMaterial;

    public GameObject releaseButtonCollider;
    public GameObject leaveButtonCollider;

    public ScreenInputDisplayer screenDisplayer;

    public ToolMenuItem currentToolMenu;

    private enum MenuState { ROOT, SETTINGS, TOOL, TOOLBOX, PARAMETERS}

    private enum ButtonState { DISABLE, ENABLE, HOVER, PRESS}

    private MenuState _menuState;
    private MenuState menuState {
        get => _menuState;
        set
        {
            previousMenuState = menuState;
            _menuState = value;
        }
    }
    private MenuState previousMenuState;

    private bool isAvatarShared = true;
    private bool isScreenShared = true;
    private bool isSound = true;

    /// <summary>
    /// They are not enable while users are not in the environement
    /// </summary>
    private bool areSettingButtonsEnabled;

    public int toolAndToolBoxDisplayDepth = 0; // 0 means root is currentlyDisplayed.
    #endregion

    #region Button's callback

    #region Navigation left/right buttons

    /// <summary>
    /// Sets the right material for navigation button depending on if users can navigate right or left.
    /// </summary>
    public  void SetNavigationButtonMaterials()
    {
        Material[] leftMats = navigateLeftRenderer.materials;
        leftMats[navigateLeftMaterials.materialIndex] = CanNavigateLeft() ? navigateLeftMaterials.enabled : navigateLeftMaterials.disabled;
        navigateLeftRenderer.materials = leftMats;

        //If a user navigates left, he can navigates right
        Material[] rightMats = navigateRightRenderer.materials;
        rightMats[navigateRightMaterials.materialIndex] = CanNavigateRight() ? navigateRightMaterials.enabled : navigateRightMaterials.disabled;
        navigateRightRenderer.materials = rightMats;
    }

    public void NavigateLeftHoverEnter()
    {
        if (CanNavigateLeft())
        {
            if (navigateLeftMaterials.materialIndex == 0)
                navigateLeftRenderer.material = navigateLeftMaterials.hoverred;
            else
            {
                Material[] mats = navigateLeftRenderer.materials;
                mats[navigateLeftMaterials.materialIndex] = navigateLeftMaterials.hoverred;
                navigateLeftRenderer.materials = mats;
            }

        }
    }
    public void NavigateLeftHoverExit()
    {
        if (CanNavigateLeft())
        {
            if (navigateLeftMaterials.materialIndex == 0)
                navigateLeftRenderer.material = navigateLeftMaterials.enabled;
            else
            {
                Material[] mats = navigateLeftRenderer.materials;
                mats[navigateLeftMaterials.materialIndex] = navigateLeftMaterials.enabled;
                navigateLeftRenderer.materials = mats;
            }
        }
    }
    public void NavigateLeft()
    {
        if (CanNavigateLeft())
        {
            if (toolParametersMenuDisplay.isDisplaying)
                hemicircularPathConstraintToolParametersMenu.Cursor -= 1;
            else
                hemicircularPathConstraintToolboxMenu.Cursor -= 1;

            SetNavigationButtonMaterials();
        }
    }

    /// <summary>
    /// Checks if navigating left is allowed (depending in the menu displayed).
    /// </summary>
    /// <returns></returns>
    private bool CanNavigateLeft()
    {
        if (toolParametersMenuDisplay.isDisplaying)
            return hemicircularPathConstraintToolParametersMenu.Cursor > hemicircularPathConstraintToolParametersMenu.numberOfItemsInCircle - 1;
        else
            return hemicircularPathConstraintToolboxMenu.Cursor > hemicircularPathConstraintToolParametersMenu.numberOfItemsInCircle - 1;
    }

    public void NavigateRightHoverEnter()
    {
        if (CanNavigateRight())
        {
            if (navigateRightMaterials.materialIndex == 0)
                navigateRightRenderer.material = navigateRightMaterials.hoverred;
            else
            {
                Material[] mats = navigateRightRenderer.materials;
                mats[navigateRightMaterials.materialIndex] = navigateRightMaterials.hoverred;
                navigateRightRenderer.materials = mats;
            }

        }
    }
    public void NavigateRightHoverExit()
    {
        if (CanNavigateRight())
        {
            if (navigateRightMaterials.materialIndex == 0)
                navigateRightRenderer.material = navigateRightMaterials.enabled;
            else
            {
                Material[] mats = navigateRightRenderer.materials;
                mats[navigateRightMaterials.materialIndex] = navigateRightMaterials.enabled;
                navigateRightRenderer.materials = mats;
            }
        }
    }
    public void NavigateRight()
    {
        if (CanNavigateRight())
        {
            if (toolParametersMenuDisplay.isDisplaying)
                hemicircularPathConstraintToolParametersMenu.Cursor += 1;
            else
                hemicircularPathConstraintToolboxMenu.Cursor += 1;

            SetNavigationButtonMaterials();
        }
    }

    /// <summary>
    /// Checks if navigating right is allowed (depending in the menu displayed).
    /// </summary>
    /// <returns></returns>
    private bool CanNavigateRight()
    {
        if (toolParametersMenuDisplay.isDisplaying)
            return hemicircularPathConstraintToolParametersMenu.Cursor < hemicircularPathConstraintToolParametersMenu.constraintedObjects.Count - 1;
        else 
            return hemicircularPathConstraintToolboxMenu.Cursor < hemicircularPathConstraintToolboxMenu.constraintedObjects.Count - 1;
    }

    public void OnNavigateInsideToolMenu()
    {
        Debug.Log("Callback !");
        switch (menuState)
        {
            case MenuState.ROOT:
                break;
            case MenuState.SETTINGS:
                break;
            case MenuState.TOOL:
                break;
            case MenuState.TOOLBOX:
                if (settingsPanel)
                {
                    DisplayToolboxesPanel();
                }
                break;
            case MenuState.PARAMETERS:
                break;
            default:
                throw new System.Exception("missing case !");

        }
        menuState = MenuState.TOOL;
    }

    #endregion

    #region Back button

    /// <summary>
    /// Changes back button maeterial to show users if they can use it or not.
    /// </summary>
    /// <param name="enable"></param>
    public void EnableBackButton(bool enable)
    {
        if (enable)
            backButtonRenderer.material = backButtonMaterials.enabled;
        else
            backButtonRenderer.material = backButtonMaterials.disabled;
    }

    public void BackHoverEnter()
    {
        switch (menuState)
        {
            case MenuState.SETTINGS:
                backButtonRenderer.material = backButtonMaterials.hoverred;
                break;
            case MenuState.TOOL:
            case MenuState.TOOLBOX:
                if (toolAndToolBoxDisplayDepth < 0)
                {
                    backButtonRenderer.material = backButtonMaterials.hoverred;
                }
                break;
            case MenuState.PARAMETERS:
            case MenuState.ROOT:
                break;
            default:
                throw new System.Exception("Case missing !");
        }
    }
    public void BackHoverExit()
    {
        switch (menuState)
        {
            case MenuState.SETTINGS:
                EnableBackButton(true);
                break;
            case MenuState.TOOL:
            case MenuState.TOOLBOX:
                if (toolAndToolBoxDisplayDepth == 0)
                    EnableBackButton(false);
                else
                    EnableBackButton(true);
                break;

            case MenuState.PARAMETERS:
            case MenuState.ROOT:
                break;
            default:
                throw new System.Exception("Case missing !");
        }
    }
    public void Back()
    {
        switch (menuState)
        {
            case MenuState.SETTINGS:
                Display();
                break;
            case MenuState.TOOLBOX:
                toolAndToolBoxDisplayDepth++;
                toolboxesAndToolsMenuDisplay.Back();
                if (toolAndToolBoxDisplayDepth == 0)
                {
                    DisplayToolboxesPanel();
                    EnableBackButton(false);
                }
                else
                {
                    EnableBackButton(true);
                    DisplayToolsPanel();
                }
                   
                SetToolboxAndToolLabel(toolboxesAndToolsMenuDisplay.lastMenuContainerUnderNavigation.menu.Name);
                SetNavigationButtonMaterials();
                break;
            case MenuState.TOOL:
                Debug.Log("<color=red>TODO : Back when menu state == TOOL</color>");
                toolboxesAndToolsMenuDisplay.Back();
                break;

            case MenuState.PARAMETERS:
                Debug.Log("<color=orange>TODO : check if it's not the toolParameterMenu </color>");
                break;
            case MenuState.ROOT:
                break;
            default:
                throw new System.Exception("Case missing !");
        }

    }

    #endregion

    #region App settings button

    public void AppSettingsButtonHoverEnter()
    {
        switch (menuState)
        {
            case MenuState.ROOT:
            case MenuState.TOOL:
            case MenuState.TOOLBOX:
                menuButtonRenderer.material = menuButtonMaterials.hoverred;
                break;

            case MenuState.SETTINGS:
            case MenuState.PARAMETERS:
                break;
            default:
                throw new System.Exception("Case missing !");
        }
    }
    public void AppSettingsButtonHoverExit()
    {
        switch (menuState)
        {
            case MenuState.ROOT:
            case MenuState.TOOL:
            case MenuState.TOOLBOX:
                menuButtonRenderer.material = menuButtonMaterials.enabled;
                break;

            case MenuState.SETTINGS:
            case MenuState.PARAMETERS:
                break;
            default:
                throw new System.Exception("Case missing !");
        }
    }
    public void AppSettingsDisplay()
    {
        switch (menuState)
        {
            case MenuState.ROOT:
            case MenuState.TOOL:
                //TODO
                break;
            case MenuState.TOOLBOX:
                DisplayToolboxesAndToolsMenu(false);
                break;
            case MenuState.SETTINGS:
                return;
            case MenuState.PARAMETERS:
                DisplayToolParamatersMenu(false);
                break;
            default:
                throw new System.Exception("Case missing !");
        }
        DisplaySettingsMenu(true);
    }

    #endregion

    #region Release button

    public void ReleaseButtonHoverEnter()
    {
        switch (menuState)
        {
            case MenuState.ROOT:
            case MenuState.TOOL:
            case MenuState.TOOLBOX:
            case MenuState.SETTINGS:
            case MenuState.PARAMETERS:
                if (controller.tool != null)
                    quitReleaseButtonRenderer.material = releaseButtonMaterials.hoverred;
                break;
            default:
                throw new System.Exception("Case missing !");
        }
    }
    public void ReleaseButtonHoverExit()
    {
        switch (menuState)
        {
            case MenuState.ROOT:
            case MenuState.TOOL:
            case MenuState.TOOLBOX:
            case MenuState.SETTINGS:
            case MenuState.PARAMETERS:
                if (controller.tool != null)
                    quitReleaseButtonRenderer.material = releaseButtonMaterials.enabled;
                break;
            default:
                throw new System.Exception("Case missing !");
        }
    }
    public void ReleaseButtonSelect()
    {
        if (controller.tool != null)
        {
            controller.ReleaseCurrentTool();
            Display();
        }
            
    }

    #endregion

    #region Quit button

    public void QuitButtonHoverEnter()
    {
        quitReleaseButtonRenderer.material = quitButtonMaterials.hoverred;
    }
    public void QuitButtonHoverExit()
    {
        quitReleaseButtonRenderer.material = quitButtonMaterials.enabled;
    }
    public void Quit()
    {
        System.Action<bool> leaveCallback = (b) =>
        {
            if (b)
            {
                if (areSettingButtonsEnabled)
                    Connecting.Instance.Leave();
                else
                    Application.Quit();
            }
        };

        string title = areSettingButtonsEnabled ? "Go back to main menu" : "Close application";
        DialogBox.Instance.Display(title, "Are you sure you want to leave ?", "Yes", "No", leaveCallback);
    }

    #endregion

    #region Settings buttons

    #region Avatar
    public void AvatarButtonHoverEnter()
    {
        if (!areSettingButtonsEnabled)
            return;
        MaterialGroup mats = isAvatarShared ? avatarBtnOnMaterials : avatarBtnOffMaterials;
        avatarButtonRenderer.material = mats.hoverred;
    }

    public void AvatarButtonHoverExit()
    {
        if (!areSettingButtonsEnabled)
            return;
        SetAvatarBtnMat(isAvatarShared, ButtonState.ENABLE);
    }

    public void AvatarButtonClick()
    {
        if (!areSettingButtonsEnabled)
            return;
        Debug.Log("<color=red>TODO AvatarButton onClick </color>");
        isAvatarShared = !isAvatarShared;
        SetAvatarBtnMat(isAvatarShared, ButtonState.HOVER);
    }

    private static void SetAvatarBtnMat(bool isEnabled, ButtonState state)
    {
        foreach (var instance in instances)
        {
            MaterialGroup mats = isEnabled ? instance.avatarBtnOnMaterials : instance.avatarBtnOffMaterials;
            Material mat;
            switch (state)
            {
                case ButtonState.DISABLE:
                    mat = mats.disabled;
                    break;
                case ButtonState.ENABLE:
                    mat = mats.enabled;
                    break;
                case ButtonState.HOVER:
                    mat = mats.hoverred;
                    break;
                case ButtonState.PRESS:
                    mat = mats.pressed;
                    break;
                default:
                    mat = null;
                    break;
            }
            instance.avatarButtonRenderer.material = mat;
        }
    }


    #endregion

    #region Microphone
    public void MicrophoneButtonHoverEnter()
    {
        if (!areSettingButtonsEnabled)
            return;
        MaterialGroup mats = !MicrophoneListener.IsMute ? microBtnOnMaterials : microBtnOffMaterials;
        microButtonRenderer.material = mats.hoverred;
    }

    public void MicrophoneButtonHoverExit()
    {
        if (!areSettingButtonsEnabled)
            return;
        SetMicrophoneMat(!MicrophoneListener.IsMute, ButtonState.ENABLE);
    }

    public void MicrophoneButtonClick()
    {
        if (!areSettingButtonsEnabled)
            return;
        if (ActivateDesactivateMicrophone.Exists)
        {
            ActivateDesactivateMicrophone.Instance.EnableMicrophone(MicrophoneListener.IsMute);
            SetMicrophoneMat(!MicrophoneListener.IsMute, ButtonState.HOVER);
        }
    }

    private static void SetMicrophoneMat(bool isEnabled, ButtonState state)
    {
        foreach (var instance in instances)
        {
            MaterialGroup mats = isEnabled ? instance.microBtnOnMaterials : instance.microBtnOffMaterials;
            Material mat;
            switch (state)
            {
                case ButtonState.DISABLE:
                    mat = mats.disabled;
                    break;
                case ButtonState.ENABLE:
                    mat = mats.enabled;
                    break;
                case ButtonState.HOVER:
                    mat = mats.hoverred;
                    break;
                case ButtonState.PRESS:
                    mat = mats.pressed;
                    break;
                default:
                    mat = null;
                    break;
            }
            instance.microButtonRenderer.material = mat;
        }
    }

    #endregion

    #region ScreenShare
    public void ScreenSharedButtonHoverEnter()
    {
        if (!areSettingButtonsEnabled)
            return;
        MaterialGroup mats = isScreenShared ? screenShareOnBtnMaterials : screenShareOffBtnMaterials;
        screenShareRenderer.material = mats.hoverred;
    }

    public void ScreenSharedButtonHoverExit()
    {
        if (!areSettingButtonsEnabled)
            return;
        MaterialGroup mats = isScreenShared ? screenShareOnBtnMaterials : screenShareOffBtnMaterials;
        SetScreenShareBtnMat(isScreenShared, ButtonState.ENABLE);
    }

    public void ScreenSharedButtonClick()
    {
        if (!areSettingButtonsEnabled)
            return;
        Debug.Log("<color=red>TODO ScreenSharedButton onClick </color>");
        isScreenShared = !isScreenShared;
        SetScreenShareBtnMat(isScreenShared, ButtonState.HOVER);
    }

    private static void SetScreenShareBtnMat(bool isEnabled, ButtonState state)
    {
        foreach (var instance in instances)
        {
            MaterialGroup mats = isEnabled ? instance.screenShareOnBtnMaterials : instance.screenShareOffBtnMaterials;
            Material mat;
            switch (state)
            {
                case ButtonState.DISABLE:
                    mat = mats.disabled;
                    break;
                case ButtonState.ENABLE:
                    mat = mats.enabled;
                    break;
                case ButtonState.HOVER:
                    mat = mats.hoverred;
                    break;
                case ButtonState.PRESS:
                    mat = mats.pressed;
                    break;
                default:
                    mat = null;
                    break;
            }
            instance.screenShareRenderer.material = mat;
        }
    }

    #endregion

    #region Sound

    public void SoundButtonHoverEnter()
    {
        if (!areSettingButtonsEnabled)
            return;
        MaterialGroup mats = isSound ? soundOnBtnMaterials : soundOffBtnMaterials;
        soundButtonRenderer.material = mats.hoverred;
    }

    public void SoundButtonHoverExit()
    {
        if (!areSettingButtonsEnabled)
            return;
        SetSoundBtnMat(isSound, ButtonState.ENABLE);
    }

    public void SoundButtonClick()
    {
        if (!areSettingButtonsEnabled)
            return;
        Debug.Log("<color=red>TODO SoundButton onClick </color>");
        isSound = !isSound;
        SetSoundBtnMat(isSound, ButtonState.HOVER);
    }

    private static void SetSoundBtnMat(bool isEnabled, ButtonState state)
    {
        foreach (var instance in instances)
        {
            MaterialGroup mats = isEnabled ? instance.soundOnBtnMaterials : instance.soundOffBtnMaterials;
            Material mat;
            switch (state)
            {
                case ButtonState.DISABLE:
                    mat = mats.disabled;
                    break;
                case ButtonState.ENABLE:
                    mat = mats.enabled;
                    break;
                case ButtonState.HOVER:
                    mat = mats.hoverred;
                    break;
                case ButtonState.PRESS:
                    mat = mats.pressed;
                    break;
                default:
                    mat = null;
                    break;
            }
            instance.soundButtonRenderer.material = mat;
        }
    }

    #endregion

    #endregion

    #endregion

    #region Actions

    /// <summary>
    /// Displays or hides (depending on param val) the app settings menu.
    /// </summary>
    /// <param name="val"></param>
    private void DisplaySettingsMenu(bool val)
    {
        appInfosPanel.SetActive(val);
        settingsPanel.SetActive(val);
        navigationPanel.SetActive(!val);

        if (val)
        {
            menuButtonRenderer.material = menuButtonMaterials.pressed;
            quitReleaseButtonRenderer.material = quitButtonMaterials.enabled;
            EnableBackButton(true);

            menuState = MenuState.SETTINGS;
        } else
        {
            menuButtonRenderer.material = menuButtonMaterials.enabled;
            quitReleaseButtonRenderer.material = quitButtonMaterials.enabled;
            EnableBackButton(false);

            menuState = MenuState.ROOT;
        }

    }

    /// <summary>
    /// Checks the interactionMapper to fill the toolboxesAndToolMenu.
    /// </summary>
    protected void UpdateToolboxesAndToolsMenu()
    {
        toolboxesAndToolsMenuDisplay.Hide(true);
        playerMainMenu.menu.RemoveAll();

        foreach (Menu toolboxMenu in InteractionMapper.Instance.toolboxMenu.GetItems())
        {
            if (toolboxMenu is ToolboxSubMenu)
            {
                playerMainMenu.menu.Add(toolboxMenu);
            }
            else
            {
                throw new System.Exception("Internal error : this submenu is not a toolbox menu but should be.");
            }
        }

        List<Tool> tools = (InteractionMapper.Instance as OpenVRInteractionMapper).GetToolsWithoutToolbox();

        foreach (ToolMenuItem toolMenu in tools.ConvertAll(t => (t as Tool).Menu as ToolMenuItem))
        {
            playerMainMenu.menu.Add(toolMenu);
            toolMenu.Subscribe(() =>
            {
                currentToolMenu = toolMenu as ToolMenuItem;
                OnNavigateInsideToolMenu();
            });
        }
    }

    /// <summary>
    /// Displays or hides the toolboxes and tools menu.
    /// </summary>
    /// <param name="display"></param>
    private void DisplayToolboxesAndToolsMenu(bool display)
    {
        if (display)
        {
            toolAndToolBoxDisplayDepth = 0;

            toolboxesAndToolsMenuDisplay.Display(true);
            menuState = MenuState.TOOLBOX;
            backButtonRenderer.material = backButtonMaterials.disabled; //because displaying this menu displays its root first
            hemicircularPathConstraintToolboxMenu.Cursor = 3;
        }
        else
        {
            if (toolboxesAndToolsMenuDisplay.isDisplaying)
                toolboxesAndToolsMenuDisplay.Hide();
        }

        DisplayToolboxesPanel();
        toolAndToolboxesLabel.gameObject.SetActive(display);
        navigationPanel.SetActive(display);
    }

    /// <summary>
    /// Displays or hides the tool parameters menu.
    /// </summary>
    /// <param name="display"></param>
    public void DisplayToolParamatersMenu(bool display)
    {
        if (display)
        {
            toolParametersMenuDisplay.Display(true);
            menuState = MenuState.PARAMETERS;
            hemicircularPathConstraintToolParametersMenu.Cursor = 3;
            DisplayParametersPanel();
        }
        else
        {
            if (menuState == MenuState.PARAMETERS)
                menuState = MenuState.ROOT;
        }
        navigationPanel.SetActive(display);
        backButtonRenderer.material = backButtonMaterials.disabled;
    }

    bool hasToolParametersMenuChanged = false;
    
    /// <summary>
    /// Adds a menu item to the tool parameters menu. BindingMenuItem will be displayed first.
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void AddMenuItemToParamatersMenu(AbstractMenuItem item)
    {
        toolParametersMenuDisplay.menu.Add(item);
        List<AbstractMenuItem> sortedItems = toolParametersMenuDisplay.menu.GetMenuItems().OrderBy(i => !(i is BindingMenuItem)).ToList();
        toolParametersMenuDisplay.menu.RemoveAll();

        foreach(var i in sortedItems)
            toolParametersMenuDisplay.menu.Add(i);
    }
    /// <summary>
    /// Removes an item to the tool parameters menu.
    /// </summary>
    public void RemoveItemFromParametersMenu(AbstractMenuItem item)
    {
        if (toolParametersMenuDisplay.menu.Contains(item))
            toolParametersMenuDisplay.menu.Remove(item);
    }

    /// <summary>
    /// Removes all menu items from paramater menu.
    /// </summary>
    public void ClearParameterMenu()
    {
        toolParametersMenuDisplay.menu.RemoveAll();
    }

    /// <summary>
    /// Display a panel when parameters (of interactable or tools) are displayed.
    /// </summary>
    public void DisplayParametersPanel()
    {
        toolPanel.SetActive(false);
        parameterPanel.SetActive(true);
    }

    /// <summary>
    /// Display a panel for when tools (of interactable or tools) are displayed.
    /// </summary>
    public void DisplayToolsPanel()
    {
        toolPanel.SetActive(true);
        parameterPanel.SetActive(false);
    }

    /// <summary>
    /// Display a panel for when toolboxes (of interactable or tools) are displayed.
    /// </summary>
    private void DisplayToolboxesPanel()
    {
        toolPanel.SetActive(false);
        parameterPanel.SetActive(false);
    }

    /// <summary>
    /// Sets the label text of the toolbox and tool label. WARNING : If the root of the toolboxMenu is displayed, the label will be overriden
    /// to be empty.
    /// </summary>
    /// <param name="label"></param>
    public void SetToolboxAndToolLabel(string label)
    {
        if (toolAndToolBoxDisplayDepth >= 0)
        {
            toolAndToolboxesLabel.text = string.Empty;
        }  
        else{
            toolAndToolboxesLabel.text = label;
        }       
    }

    #endregion

    #region Status 

    /// <summary>
    /// Returns true if the menu if the menu is displayed.
    /// </summary>
    public bool IsDisplaying { get; private set; }

    /// <summary>
    /// Returns true if the menu if the tool parameter menu is displayed.
    /// </summary>
    public bool IsDisplayingToolParameters
    {
        get => IsDisplaying && toolParametersMenuDisplay.menu.Count > 0;
    }

    #endregion

    /// <summary>
    /// Displays the joystick menu. Depending on the data, the menu displayed will be different :
    ///     - if toolParametersMenuDisplay contains elements (actions or paramaters), il will be displayed
    ///     - else if toolboxesAndToolsMenuDisplay contains elements (toolboxes or tools without toolbox), it will be displayed
    ///     - else nothing is displayed (settings menu is always hidden).
    /// </summary>
    [ContextMenu("Display")]
    public void Display()
    {
        IsDisplaying = true;
        this.gameObject.SetActive(true);

        UpdateToolboxesAndToolsMenu();

        DisplaySettingsMenu(false);

        if (toolParametersMenuDisplay.menu.Count > 0)
        {
            DisplayToolboxesAndToolsMenu(false);
            DisplayToolParamatersMenu(true);

        } else if (toolboxesAndToolsMenuDisplay.menu.Count > 0)
        {
            DisplayToolParamatersMenu(false);
            DisplayToolboxesAndToolsMenu(true);

        } else
        {
            DisplayToolboxesAndToolsMenu(false);
            DisplayToolParamatersMenu(false);
            menuState = MenuState.ROOT;
        }

        var hasAToolProjected = (controller.tool != null);

        if (hasAToolProjected)
        {
            bool canBeReleased = (InteractionMapper.Instance as OpenVRInteractionMapper).IsToolReleasable(controller.tool.id);
            releaseButtonCollider.SetActive(canBeReleased);
            leaveButtonCollider.SetActive(!canBeReleased);
            quitReleaseButtonRenderer.material = canBeReleased ? releaseButtonMaterials.enabled : quitButtonMaterials.enabled;
        } else
        {
            releaseButtonCollider.SetActive(hasAToolProjected);
            leaveButtonCollider.SetActive(!hasAToolProjected);
            quitReleaseButtonRenderer.material = hasAToolProjected ? releaseButtonMaterials.enabled : quitButtonMaterials.enabled;
        }

        SetNavigationButtonMaterials();
    }

    public bool WasHiddenLastFrame { get; private set; } = false;

    /// <summary>
    /// Hides all circular menu.
    /// </summary>
    public void Hide()
    {
        WasHiddenLastFrame = true;
        // If users were editing a parameter cancels the operation.
        if (screenDisplayer.IsDisplayed)
            screenDisplayer.HideScreen(false);

        toolboxesAndToolsMenuDisplay.Hide(true);

        IsDisplaying = false;
        this.gameObject.SetActive(false);

        MainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(ResetWasHiddenLastFrame());
    }

    
    System.Collections.IEnumerator ResetWasHiddenLastFrame()
    {
        yield return null;
        WasHiddenLastFrame = false;
    }

    public void OpenCurrentToolMenu()
    {
        toolboxesAndToolsMenuDisplay.Display(true);
        toolboxesAndToolsMenuDisplay.Navigate(currentToolMenu);
    }

    protected virtual void Awake()
    {
        instances.Add(this);
    }   

    void Start()
    {
        toolboxesAndToolsMenuDisplay.onDisplay.AddListener(() => SetNavigationButtonMaterials());
        toolParametersMenuDisplay.onDisplay.AddListener(() => SetNavigationButtonMaterials());
    }

    protected virtual void OnDestroy()
    {
        instances.Remove(this);
    }

    /// <summary>
    /// Enables settings menu for all PlayerMenuManager (there is one for each Touch Controller).
    /// </summary>
    public static void EnableSettingMenu()
    {
        foreach(var instance in instances)
        {
            instance._EnableSettingMenu();
        }
    }

    /// <summary>
    /// Enables settings button by changing their looks and allowing them to be triggered.
    /// </summary>
    void _EnableSettingMenu()
    {
        areSettingButtonsEnabled = true;
        avatarButtonRenderer.material = avatarBtnOnMaterials.enabled;
        screenShareRenderer.material = screenShareOffBtnMaterials.enabled;
        soundButtonRenderer.material = soundOnBtnMaterials.enabled;
        microButtonRenderer.material = MicrophoneListener.IsMute ? microBtnOffMaterials.enabled : microBtnOnMaterials.enabled;
    }

    void Update()
    {
        if (hasToolParametersMenuChanged)
        {
            hasToolParametersMenuChanged = false;
            if (menuState == MenuState.TOOLBOX || menuState == MenuState.TOOL)
                return;
            else
                DisplayToolParamatersMenu(true); 
        }
    } 

    void LateUpdate()
    {
        if (toolParametersMenuDisplay.isDisplaying && toolParametersMenuDisplay.menu.Count == 0)
        {
            DisplayToolParamatersMenu(false);
        }
    }
}