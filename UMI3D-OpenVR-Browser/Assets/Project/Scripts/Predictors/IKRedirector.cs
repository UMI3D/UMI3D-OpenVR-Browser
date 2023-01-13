using UnityEditor;
using UnityEngine;


public class IKRedirector : MonoBehaviour
{
    public LocalMirror mirrorhandler;

    private void OnAnimatorIK(int layerIndex)
    {
        mirrorhandler.MoveLimbs();
    }
}
