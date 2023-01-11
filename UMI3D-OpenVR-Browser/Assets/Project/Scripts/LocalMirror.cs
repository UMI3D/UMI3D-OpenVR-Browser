using UnityEditor;
using UnityEngine;


public class LocalMirror : MonoBehaviour
{
    private GameObject mirrorAvatar;
    private Animator animator;

    private Vector3 prevPos;
    private float prevTime;
    private float maxSpeed = 0.5f;

    public void Start()
    {
        prevPos = transform.position;
        prevTime = Time.time;
        
    }

    public void Update()
    {
        var speed = (transform.position - prevPos).magnitude / (Time.time - prevTime);
        prevPos = transform.position;
        prevTime = Time.time;

        if (mirrorAvatar == null)
            mirrorAvatar = GameObject.Find("Player mirror");
        if (mirrorAvatar == null)
            return;
        if (animator == null)
            animator = mirrorAvatar.GetComponentInChildren<Animator>();
        if (animator == null)
            return;

        Debug.Log($"Speed: {speed}");
        if (speed < maxSpeed)
            animator.SetFloat("Speed", speed / maxSpeed);
        else
            animator.SetFloat("Speed", 1);

    }
}
