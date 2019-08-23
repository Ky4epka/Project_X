using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectComponentsCollection_Script))]
[RequireComponent(typeof(EnduranceObject_Script))]
[RequireComponent(typeof(ObjectAttaching_Script))]
public class EnduranceIndicatorShell_Script : MonoBehaviour {

    public float Spacing = 1f;
    public float WidthScalling = 1f;

    private ObjectComponentsCollection_Script fComps = null;
    private EnduranceIndicator_Script fIndicator = null;
    private bool fInit = true;


    public EnduranceIndicator_Script Indicator
    {
        get
        {
            return fIndicator;
        }
    }

    public void Initialize()
    {
        fIndicator = EnduranceIndicator_Script.Instantiate(GlobalCollector.Instance.Default_EnduranceIndicator_Sample);
        fComps.SetEnduranceIndicator(fIndicator.gameObject);
        fIndicator.gameObject.SetActive(true);
        fIndicator.EnduranceObject = fComps.EnduranceObject_Comp;
        fIndicator.AttachingObject = fComps.AttachingManager_Comp;
        fIndicator.Default_Spacing = Spacing;
        fIndicator.Default_WidthScalling = WidthScalling;
        fIndicator.name = fComps.name + "_Indicator";
        fIndicator.Initialize();
    }

    // Use this for initialization
    void Start() {
        fComps = this.GetComponent<ObjectComponentsCollection_Script>();
    }

    // Update is called once per frame
    void Update() {
        if (fInit)
        {
            fInit = false;
            Initialize();
        }
	}
}
