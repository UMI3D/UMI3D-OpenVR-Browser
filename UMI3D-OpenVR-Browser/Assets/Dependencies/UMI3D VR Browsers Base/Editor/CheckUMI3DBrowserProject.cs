/*
Copyright 2019 - 2022 Inetum

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

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.interaction;
using umi3d.cdk.userCapture;
using umi3dVRBrowsersBase.connection;
using umi3d.cdk.collaboration;
using umi3dVRBrowsersBase.interactions.input;
using umi3dVRBrowsersBase.navigation;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.interactions.selection.selector;

namespace umi3dVRBrowsersBase.editor
{
    /// <summary>
    /// Provides an editor helper to help developers make sure their project and player prefab are correctly configured.
    /// </summary>
    public class CheckUMI3DBrowserProject : EditorWindow
    {
        #region Fields

        /// <summary>
        /// Where all project errors are displayed.
        /// </summary>
        VisualElement projectErrorContainer;

        /// <summary>
        /// Where all player errors are displayed.
        /// </summary>
        VisualElement playerErrorContainer;

        #endregion

        #region Methods

        [MenuItem("UMI3D/Check Project")]
        public static void Display()
        {
            CheckUMI3DBrowserProject wnd = GetWindow<CheckUMI3DBrowserProject>();
            wnd.titleContent = new GUIContent("UMI3D Check Project");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 5;
            root.style.paddingRight= 5;
            root.style.paddingTop = 5;
            root.style.paddingBottom = 5;

            VisualElement title = new Label("UMI3D Project checker");
            title.style.fontSize = 20;
            title.style.marginBottom = 5;
            title.style.marginTop = 5;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;

            VisualElement subtitle = new Label("Helper to maker sure your project is correctly configured and your player prefab correctly set.");
            subtitle.style.fontSize = 12;

            Button checkProject = new Button { text = "Check project" };
            checkProject.style.width = 200;
            checkProject.clickable.clicked += CheckProject;

            projectErrorContainer = new VisualElement();

            ObjectField playerPrefabField = new ObjectField("Player prefab ");
            playerPrefabField.objectType = typeof(GameObject);

            Button checkPlayer = new Button { text = "Check PlayerPrefab" };
            checkPlayer.style.width = 200;
            checkPlayer.clickable.clicked += () => CheckPlayerPrefab(playerPrefabField.value as GameObject);

            playerErrorContainer = new VisualElement();

            root.Add(title);
            root.Add(subtitle);

            root.Add(CreateSeparator());

            root.Add(checkProject);
            root.Add(projectErrorContainer);

            root.Add(CreateSeparator());

            root.Add(playerPrefabField);
            root.Add(checkPlayer);
            root.Add(playerErrorContainer);
        }

        /// <summary>
        /// Returns a Visualement similar to <hr>.
        /// </summary>
        /// <returns></returns>
        private VisualElement CreateSeparator()
        {
            return new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(.5f, .5f, .5f),
                    height = 1,
                    marginTop = 10,
                    marginBottom = 10,
                    marginLeft = StyleKeyword.Auto,
                    marginRight = StyleKeyword.Auto,
                    width = new StyleLength(new Length(95, LengthUnit.Percent))
                }
            };
        }

        /// <summary>
        /// Checks if the projet is correctly configured.
        /// </summary>
        private void CheckProject()
        {
            projectErrorContainer.Clear();
            List<string> errors = new List<string>();

            if (EditorBuildSettings.scenes.Length < 4)
            {
                errors.Add("Your project has less than 4 scenes whereas it should have at least StartScene, ConnectionScene, MainImmersive and TutorialScene");
            }

            if (errors.Count == 0)
            {
                projectErrorContainer.Add(new Label { text = "You project seems correctly configured ! ", style = { color = Color.green } });
            }
            else
            {
                foreach (var e in errors)
                {
                    projectErrorContainer.Add(new Label { text = e, style = { color = Color.red } });
                }
            }
        }

        /// <summary>
        /// Checks if player prefabs has everything required.
        /// </summary>
        /// <param name="player"></param>
        private void CheckPlayerPrefab(GameObject player)
        {
            if (player == null || player == default)
            {
                return;
            }

            playerErrorContainer.Clear();

            List<string> errors = new List<string>();

            //Check if player has a camera
            if (player.GetComponentInChildren<Camera>() == null)
                errors.Add("Prefab without camera.");

            //Check UMI3DNavigation
            if (player.GetComponentInChildren<umi3d.cdk.UMI3DNavigation>() == null)
                errors.Add("Prefab without UMI3DNavigation.");

            //Check InteractionMapper
            if (player.GetComponentInChildren<InteractionMapper>() == null)
                errors.Add("Not mandatory on this prefab but make sur you have an InteractionMapper in your scene.");

            //Check User Tracking
            if (player.GetComponentInChildren<UMI3DClientUserTracking>() == null)
                errors.Add("Prefab without UMI3DClientUserTracking.");

            //Check User DialogBox
            if (player.GetComponentInChildren<DialogBox>() == null)
                errors.Add("Prefab without DialogBox.");

            //Check AudioManager
            if (player.GetComponentInChildren<AudioManager>() == null)
                errors.Add("Prefab without AudioManager.");

            //Check MicrophoneListener
            if (player.GetComponentInChildren<MicrophoneListener>() == null)
                errors.Add("Prefab without MicrophoneListener.");

            //Check Controllers.
            int nbControllers = player.GetComponentsInChildren<AbstractController>().Length;
            if (nbControllers != 2)
                errors.Add("Prefab with " + nbControllers + "AbstractController, generally 2 are required for VR players.");

            //Check Boolean Input
            if (player.GetComponentsInChildren<BooleanInput>().Length == 0)
                errors.Add("Warning : Prefab without any BooleanInput");

            //Check ManipulationInput
            if (player.GetComponentsInChildren<ManipulationInput>().Length == 0)
                errors.Add("Warning : Prefab without any ManipulationInput");

            //Check selectors
            if (player.GetComponentsInChildren<InteractableVRSelector>().Length == 0)
                errors.Add("Prefab without any InteractableVRSelector, you can add UMI3D VR Browser Base/Prefab/ControllerSelectors as a children of each of you controller");
            if (player.GetComponentsInChildren<SelectableVRSelector>().Length == 0)
                errors.Add("Prefab without any SelectableVRSelector, you can add UMI3D VR Browser Base/Prefab/ControllerSelectors as a children of each of you controller");
            if (player.GetComponentsInChildren<ElementVRSelector>().Length == 0)
                errors.Add("Prefab without any ElementVRSelector, you can add UMI3D VR Browser Base/Prefab/ControllerSelectors as a children of each of you controller");

            //Check TeleportationArc
            if (player.GetComponentsInChildren<TeleportArc>().Length == 0)
                errors.Add("Warning : Prefab without TeleportArc");

            //Check AbstractControllerInputManager
            if (player.GetComponentsInChildren<AbstractControllerInputManager>().Length == 0)
                errors.Add("Warning : Prefab without AbstractControllerInputManager");

            if (errors.Count == 0)
            {
                playerErrorContainer.Add(new Label { text = "You player prefab seems correct ! ", style = { color = Color.green } });
            } else
            {
                foreach(var e in errors)
                {
                    playerErrorContainer.Add(new Label { text = e, style = { color = Color.red } });
                }
            }
        }

        #endregion
    }
}
