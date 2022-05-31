using inetum.unityUtils;
using umi3d.cdk;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;

namespace umi3dVRBrowsersBase.connection
{
    public class LoadingScreenDisplayer : PersistentSingleBehaviour<LoadingScreenDisplayer>
    {
        /// <summary>
        /// Sphere used to make the screen black;
        /// </summary>
        [SerializeField]
        private MeshRenderer loadingSphere;

        private Camera cam;

        [SerializeField]
        private LayerMask loadingCullingMask;

        private int defaultCullingMask;

        [SerializeField]
        GameObject loadingLabel;

        private void Start()
        {
            cam = Camera.main;
            Debug.Assert(cam != null, "Impossible to find a camera");

            defaultCullingMask = cam.cullingMask;

            Hide();
            DontDestroyOnLoad(loadingSphere.gameObject);

            UMI3DEnvironmentLoader.Instance.onProgressChange.AddListener(OnProgressChange);
        }

        /// <summary>
        /// Displays loading screen
        /// </summary>
        public void Display()
        {
            if (ConnectionMenuManager.instance != null)
            {
                if (PlayerMenuManager.Exists)
                    loadingSphere.transform.position = PlayerMenuManager.Instance.PlayerCameraTransform.position;

                loadingSphere.enabled = true;
            }
            else
            {
                loadingLabel.SetActive(true);

                cam.cullingMask = loadingCullingMask.value;
                cam.clearFlags = CameraClearFlags.SolidColor;

                if (PlayerMenuManager.Exists)
                {
                    loadingLabel.transform.position = PlayerMenuManager.Instance.PlayerCameraTransform.position + PlayerMenuManager.Instance.PlayerCameraTransform.forward * 1f;
                    loadingLabel.transform.LookAt(PlayerMenuManager.Instance.PlayerCameraTransform.position);
                    loadingLabel.transform.Rotate(0, 180, 0);
                }

            }
        }

        /// <summary>
        /// Hides loading screen
        /// </summary>
        public void Hide()
        {
            loadingSphere.enabled = false;
            cam.cullingMask = defaultCullingMask;
            cam.clearFlags = CameraClearFlags.Skybox;
            loadingLabel.SetActive(false);
        }

        private void OnProgressChange(float val)
        {
            if (val >= 0 && val <= 1)
            {
                Display();
            }
        }
    }
}