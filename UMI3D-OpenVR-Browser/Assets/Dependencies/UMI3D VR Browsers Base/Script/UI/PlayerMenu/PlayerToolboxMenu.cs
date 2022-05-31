using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.menu;
using umi3dVRBrowsersBase.interactions.input;
using UnityEngine;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public partial class PlayerToolboxMenu
    {
        /// <summary>
        /// Toggles the display of the menu with all pinned items.
        /// </summary>
        public void ToggleDisplayMenu()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        /// <summary>
        /// Navigates to the [menu].
        /// </summary>
        /// <param name="menu"></param>
        public void NavigateTo(AbstractMenu menu)
            => menuDisplayManager.Navigate(menu);
    }

    public partial class PlayerToolboxMenu : AbstractMenuManager
    {
        private void Start()
        {
            Close();
        }

        /// <summary>
        /// Open the toolboxes player menu.
        /// </summary>
        [ContextMenu("Open player menu")]
        public override void Open()
        {
            base.Open();

            PlayerMenuManager.Instance.CtrlToolMenu.Hide();

            menuDisplayManager.Display(true);
        }

        /// <summary>
        /// Closes the toolboxes player menu.
        /// </summary>
        public override void Close()
        {
            base.Close();
            menuDisplayManager.Hide(false);
        }
    }
}
