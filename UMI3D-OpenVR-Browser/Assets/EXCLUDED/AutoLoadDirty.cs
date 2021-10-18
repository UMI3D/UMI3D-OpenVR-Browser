using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// I'm lazy and i'm sorry.
/// </summary>
public class AutoLoadDirty : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DoItForMe());
    }
    
    IEnumerator DoItForMe()
    {
        UnityEngine.EventSystems.EventSystem eventSys = UnityEngine.EventSystems.EventSystem.current;
        yield return new WaitForSeconds(1);
        (GameObject.Find("ConnectionPanel").transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetComponent<Button>()).OnSubmit(new UnityEngine.EventSystems.BaseEventData(eventSys));
        yield return new WaitForSeconds(.5f);
        (GameObject.Find("AdvancedConnectionButton").GetComponent<Button>()).OnSubmit(new UnityEngine.EventSystems.BaseEventData(eventSys));
        yield return new WaitForSeconds(.5f);
        (GameObject.Find("AdvanceConnectionPanel").transform.GetChild(0).GetChild(3).GetComponent<Button>()).OnSubmit(new UnityEngine.EventSystems.BaseEventData(eventSys));
        while (GameObject.Find("Password") == null)
            yield return new WaitForSeconds(1);
        GameObject.Find("Password").transform.GetChild(1).GetComponent<InputField>().text = "0000";
        (GameObject.Find("Password").transform.parent.GetChild(3).GetComponent<Button>()).OnSubmit(new UnityEngine.EventSystems.BaseEventData(eventSys));
    }
}
