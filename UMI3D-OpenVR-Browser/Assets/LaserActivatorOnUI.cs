using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class LaserActivatorOnUI : MonoBehaviour
{
    Canvas[] canvas;
    ActivateDeactivateMonobehaviour adm;
    bool hasSurvey;

    public GameObject LaserLeft;
    public GameObject LaserRight;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FoundUI());
    }

    IEnumerator FoundUI()
    {
        hasSurvey = false;
        while (!hasSurvey)
        {
            yield return null;

            canvas = GetComponentsInChildren<Canvas>(true);
    
            if (canvas.Length >= 2)
            {
                if (canvas[1].gameObject.name.Contains("SurveyCanvas"))
                {
                    Debug.Log("Yes " + canvas[1].gameObject.name);

                    LaserLeft.SetActive(false);
                    LaserRight.SetActive(false);

                    adm = canvas[1].gameObject.AddComponent<ActivateDeactivateMonobehaviour>();
                    adm.activated += value =>
                    {
                        LaserLeft.SetActive(value);
                        LaserRight.SetActive(value);
                    };
                    hasSurvey = true;
                }
            }
            else
            {
                Debug.Log("Un seul");
            }
            
        }
        yield break;
    }
}
