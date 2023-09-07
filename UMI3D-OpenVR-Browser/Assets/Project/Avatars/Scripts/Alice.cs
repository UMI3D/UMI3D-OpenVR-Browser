using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alice : MonoBehaviour
{
    public Transform Bob;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.position = Bob.position;
        this.transform.rotation = Quaternion.Euler(Bob.rotation.eulerAngles.x, 0, Bob.rotation.eulerAngles.z);
        //this.transform.localRotation = Quaternion.Euler(12, 45, 47);
    }
}
