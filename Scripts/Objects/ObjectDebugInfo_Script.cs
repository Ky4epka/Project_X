using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectDebugInfo_Script : MonoBehaviour {

        public TextMeshProUGUI TextLayer = null;

        private Dictionary<string, string> fLines = new Dictionary<string, string>();


    public void DebugValue(string val_name, string val_str)
    {
        if (!fLines.ContainsKey(val_name))
            fLines.Add(val_name, val_str);
        else
            fLines[val_name] = val_str;

        UpdateLayer();
    }

    public void DeleteValue(string val_name)
    {
        fLines.Remove(val_name);
        UpdateLayer();
    }

    void UpdateLayer()
    {
        string str = "";

        foreach (KeyValuePair<string, string> pair in fLines)
        {
            str += pair.Key + ": " + pair.Value + ";\n";
        }

        TextLayer.text = str;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
