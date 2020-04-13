// <copyright file="Test.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>04/12/2020</date>
// <summary>
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.UIDock;

public class Test : MonoBehaviour
{
    public Root root;

    public RectTransform rtMain;

    Window mainWin;
    
    void Start()
    {
        this.mainWin = root.WrapIntoWindow(this.rtMain, "Test Titlebar");
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Add Window") == true)
        { 
            GameObject go = new GameObject("TestWin");
            go.transform.SetParent(root.transform);

            UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = 
                new Color(
                    Random.Range(0.0f, 1.0f), 
                    Random.Range(0.0f, 1.0f),
                    Random.Range(0.0f, 1.0f));

            Window.PrepareChild(img.rectTransform);
            img.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
            img.rectTransform.sizeDelta = new Vector2(200.0f, 200.0f);

            root.WrapIntoWindow(img.rectTransform, "Thing!");
        }
    }
}
