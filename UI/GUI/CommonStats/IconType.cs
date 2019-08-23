using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IconType : MonoBehaviour {

    protected Image fImage = null;
    protected ObjectType_Script fObjectType = null;


    public ObjectType_Script ObjectType
    {
        get
        {
            return fObjectType;
        }

        set
        {
            fObjectType = value;
            UpdateUI();
        }
    }

    protected virtual void UpdateUI()
    {
        if (fObjectType != null)
            fImage.sprite = fObjectType.TypeInfo.TypeData.TypeIcon;
        else
            fImage.sprite = null;
    }

	// Use this for initialization
	void Start () {
        fImage = this.GetComponent<Image>();
	}

    private void OnValidate()
    {
        fImage = this.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
