using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Immersio.Utility;

public class DebugOption
{
    public string Name { get; private set; }
    public KeyCode Key { get; private set; }
    public bool IsActivated { get; private set; }
    public bool IsTrigger { get; private set; }

    public bool UnableToSetActivation { get; private set; }

    Predicate<bool> _setActive_Predicate;


    public DebugOption(KeyCode keyCode, string name, Predicate<bool> setActive_Predicate)
        : this(keyCode, name, setActive_Predicate, false, false)
    { }

    public DebugOption(KeyCode keyCode, string name, Predicate<bool> setActive_Predicate, bool isActivated)
        : this(keyCode, name, setActive_Predicate, isActivated, false)
    { }

    public DebugOption(KeyCode keyCode, string name, Predicate<bool> setActive_Predicate, bool isActivated, bool isTrigger)
    {
        this.Name = name;
        this.Key = keyCode;
        this.IsActivated = isActivated;
        this.IsTrigger = isTrigger;

        _setActive_Predicate = setActive_Predicate;
    }

    public void Activate(bool value)
    {
        IsActivated = value;
        if (_setActive_Predicate != null)
        {
            UnableToSetActivation = !_setActive_Predicate(value);
        }
    }


    public const KeyCode NullKeyCode = KeyCode.JoystickButton19;
    public static DebugOption EmptyOption()
    {
        return new DebugOption(NullKeyCode, string.Empty, null, false, true);
    }
}


public class DebugOptions : Singleton<DebugOptions>
{
    #region Options

    List<DebugOption> _options = new List<DebugOption> {

        new DebugOption(KeyCode.F1, "Show Debug Options", DebugOptionsUI_SetActive),
        DebugOption.EmptyOption(),
    };
    public List<DebugOption> Options { get { return _options; } }


    static bool DebugOptionsUI_SetActive(bool value)
    {
        if (DebugOptions_UI == null)
        {
            return false;
        }
        DebugOptions_UI.SetActive(value);
        return true;
    }

    /*static bool PlayerListingsUI_SetActive(bool value)
    {
        if (PlayerListings_UI == null)
        {
            return false;
        }

        PlayerListings_UI.SetActive(value);
        return true;
    }*/

    #endregion


    #region UI Stuff

    const string DebugOptionsUIPrefabPath = "DebugOptions UI";
    //const string PlayerListingsUIPrefabPath = "PlayerListingsUI_uNet";

    const float CanvasWidth = 1920f;
    const float CanvasHeight = 1080f;
    const float CanvasScale = 0.001f;
    const float CanvasOffsetZ = 0.64f;


    static GameObject _debugCanvas;
    static GameObject DebugCanvas { get { return _debugCanvas ?? (_debugCanvas = CreateCanvasOnVRCamera()); } }
    static GameObject _debugOptions_UI;
    static GameObject DebugOptions_UI { get { return _debugOptions_UI ?? (_debugOptions_UI = InstantiateUiPanel(DebugOptionsUIPrefabPath)); } }
    //static GameObject _playerListings_UI;
    //static GameObject PlayerListings_UI { get { return _playerListings_UI ?? (_playerListings_UI = InstantiateUiPanel(PlayerListingsUIPrefabPath)); } }


    static GameObject CreateCanvasOnVRCamera()
    {
        GameObject leftEyeAnchor = GameObject.Find("LeftEyeAnchor");
        if (leftEyeAnchor == null)
        {
            return null;
        }

        GameObject debugCanvas = new GameObject();
        debugCanvas.name = "Debug Options Canvas";
        debugCanvas.transform.parent = leftEyeAnchor.transform;

        RectTransform rt = debugCanvas.AddComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CanvasWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CanvasHeight);
        rt.localScale = CanvasScale * Vector3.one;
        rt.localPosition = new Vector3(0, 0, CanvasOffsetZ);
        rt.localEulerAngles = Vector3.zero;

        Canvas canvas = debugCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.pixelPerfect = false;

        return debugCanvas;
    }

    static GameObject InstantiateUiPanel(string prefabPath)
    {
        if (DebugCanvas == null)
        {
            return null;
        }

        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("Prefab not found in Resources: " + prefabPath);
            return null;
        }

        GameObject g = Instantiate(prefab) as GameObject;
        g.name = prefab.name;

        Transform t = g.transform;
        t.SetParent(DebugCanvas.transform);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        return g;
    }

    #endregion


    void OnLevelWasLoaded(int level)
    {
        _debugCanvas = null;
        _debugOptions_UI = null;
        //_playerListings_UI = null;
    }


    void Update()
    {
        foreach (DebugOption option in _options)
        {
            if (Input.GetKeyUp(option.Key))
            {
                option.Activate(!option.IsActivated);
            }
            if (option.UnableToSetActivation)
            {
                option.Activate(option.IsActivated);
            }
        }
    }


    public void ApplyCurrentSettings()
    {
        foreach (DebugOption option in _options)
        {
            if (!option.IsTrigger)
            {
                option.Activate(option.IsActivated);
            }
        }
    }

    public DebugOption GetOptionByName(string name)
    {
        return _options.Single(p => p.Name == name);
    }
    public void ActivateOptionByName(string name, bool value)
    {
        DebugOption option = GetOptionByName(name);
        if (option != null)
        {
            option.Activate(value);
        }
    }
}