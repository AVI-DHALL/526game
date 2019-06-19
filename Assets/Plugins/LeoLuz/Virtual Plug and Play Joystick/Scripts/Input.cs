﻿//info: Fiz esse codigo pra servir como um atravessador da input, assim eu poderia adaptar qualquer jogo para mobile,
//sem fazer nada, ele ele atravessa na Input.GetButton e por enquanto funciona bem. Trabalha em cooperação com o script
//ButtonToDefaultInput.cs que é colocado em qualquer botão da UI e funciona perfeitamente até agora.
#region Using
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

public class Input : MonoBehaviour
{
    public enum Mode { pc, touch, both }
    public static Mode mode = Mode.both;
    public Mode _mode = Mode.both;
    #region INPUT API OVERRIDING
    public static AccelerationEvent GetAccelerationEvent(int index)
    {
       return UnityEngine.Input.GetAccelerationEvent(index);
    }
    public static string[] GetJoystickNames()
    {

        return UnityEngine.Input.GetJoystickNames();
    }

    public static void ResetInputAxes()
    {
        
        UnityEngine.Input.ResetInputAxes();
    }

    public static bool touchSupported { get { return UnityEngine.Input.touchSupported;  } }
    public static bool touchPressureSupported { get { return UnityEngine.Input.touchPressureSupported; } }
    public static bool stylusTouchSupported { get { return UnityEngine.Input.stylusTouchSupported; } }
    public static bool multiTouchEnabled { get { return UnityEngine.Input.multiTouchEnabled; } }
    public static bool mousePresent { get { return UnityEngine.Input.mousePresent; } }
    public static Vector2 mousePosition { get { return UnityEngine.Input.mousePosition; } }
    public static LocationService location { get { return UnityEngine.Input.location; } }
    public static string inputString { get { return UnityEngine.Input.inputString; } }
    public static bool imeIsSelected { get { return UnityEngine.Input.imeIsSelected; } }
    public static IMECompositionMode imeCompositionMode { get { return UnityEngine.Input.imeCompositionMode; } }
    public static DeviceOrientation deviceOrientation { get { return UnityEngine.Input.deviceOrientation; } }
    public static Gyroscope gyro { get { return UnityEngine.Input.gyro; } }
    public static string compositionString { get { return UnityEngine.Input.compositionString; } }
    public static Vector2 compositionCursorPos { get { return UnityEngine.Input.compositionCursorPos; } }
    public static bool compensateSensors { get { return UnityEngine.Input.compensateSensors; } }
    public static Compass compass { get { return UnityEngine.Input.compass; } }
    public static bool backButtonLeavesApp { get { return UnityEngine.Input.backButtonLeavesApp; } }
    public static bool anyKeyDown { get { return UnityEngine.Input.anyKeyDown; } }
    public static bool anyKey { get { return UnityEngine.Input.anyKey; } }
    public static AccelerationEvent[] accelerationEvents { get { return UnityEngine.Input.accelerationEvents; } }
    public static int accelerationEventCount { get { return UnityEngine.Input.accelerationEventCount; } }
    public static Vector3 acceleration { get { return UnityEngine.Input.acceleration; } }

    public static bool GetButtonDown(string Button)
    {
        switch (mode)
        {
            case Mode.pc:
                return UnityEngine.Input.GetButtonDown(Button);
            case Mode.touch:
                return GetButtonDownMobile(Button);
            case Mode.both:
                return UnityEngine.Input.GetButtonDown(Button) || GetButtonDownMobile(Button);
            default:
                return false;
        }
    }
    public static bool GetButton(string Button)
    {
        switch (mode)
        {
            case Mode.pc:
                return UnityEngine.Input.GetButton(Button);
            case Mode.touch:
                return GetButtonMobile(Button);
            case Mode.both:
                return UnityEngine.Input.GetButton(Button) || GetButtonMobile(Button);
            default:
                return false;
        }
    }
    public static bool GetButtonUp(string Button)
    {
        switch (mode)
        {
            case Mode.pc:
                return UnityEngine.Input.GetButtonUp(Button);
            case Mode.touch:
                return GetButtonUpMobile(Button);
            case Mode.both:
                return UnityEngine.Input.GetButtonUp(Button) || GetButtonUpMobile(Button);
            default:
                return false;
        }
    }
    public static bool GetMouseButton(int button)
    {
        return UnityEngine.Input.GetMouseButton(button);
    }
    public static bool GetMouseButtonDown(int button)
    {
        return UnityEngine.Input.GetMouseButtonDown(button);
    }
    public static bool GetMouseButtonUp(int button)
    {
        return UnityEngine.Input.GetMouseButtonUp(button);
    }
    public static bool GetKey(string key)
    {
        return UnityEngine.Input.GetKey(key);
    }
    public static bool GetKeyDown(string key)
    {
        return UnityEngine.Input.GetKeyDown(key);
    }
    public static bool GetKeyUp(string key)
    {
        return UnityEngine.Input.GetKeyUp(key);
    }
    public static bool GetKey(KeyCode key)
    {
        return UnityEngine.Input.GetKeyUp(key);
    }
    public static bool GetKeyDown(KeyCode key)
    {
        return UnityEngine.Input.GetKeyDown(key);
    }
    public static bool GetKeyUp(KeyCode key)
    {
        return UnityEngine.Input.GetKeyUp(key);
    }
    public static float GetAxis(string axis)
    {
        // print(axis+" : "+ UnityEngine.Input.GetAxis(axis) + GetAxisMobile(axis));
        return UnityEngine.Input.GetAxis(axis) + GetAxisMobile(axis);
    }
    public static float GetAxisRaw(string axis)
    {
        return UnityEngine.Input.GetAxisRaw(axis) + GetAxisMobile(axis);
    }
    public static int touchCount
    {
        get
        {
            return UnityEngine.Input.touchCount;
        }
    }
    public static Touch GetTouch(int index)
    {
        return UnityEngine.Input.GetTouch(index);
    }
    public static Touch[] touches
    {
        get
        {
            return UnityEngine.Input.touches;
        }
    }
    #endregion
    #region OLD
    private static List<Button> CurrentButtons; //The button checks works like a button list that were pressed
    private static Button[] ButtonsPool;
    public static float time;
    public static int CurrentButtonPoolIndex;
    public static List<Axis> Axes; //The axis is created as needed. If this list not contains a axis,will be created.
    public static int NumberOfSimultaneousButtons = 32;

    public void Start()
    {
        Initialize();
    }

    void FixedUpdate()
    {

        time = Time.time;


    }

    void LateUpdate()
    {
        ClearUpsAndEvolveDownToPressedOld();
    }

    public void Initialize()
    {
        //inicializei as inputs pelo scriptable objects aqui
        ButtonsPool = new Button[NumberOfSimultaneousButtons]; //pooling of buttons to not instantiate objects unessessaryly
        CurrentButtons = new List<Button>();
        Axes = new List<Axis>();
        mode = _mode;

        for (int i = 0; i < ButtonsPool.Length; i++)
        {
            ButtonsPool[i] = new Button();
        }
    }
    //this method autoconfigure this component in a object in the scene 
    public static void Autoconfigure()
    {
        GameObject obj = GameObject.Find("GenericFunctionsInstance");
        if (obj == null)
            obj = GameObject.Find("InputManager");
        if (obj == null)
        {
            obj = new GameObject();
            obj.name = "InputManager";
            obj.AddComponent<Input>();

        }
        else
        {
            if (!obj.GetComponent<Input>())
            {
                obj.AddComponent<Input>();

            }
        }

    }

    //Find if button specified is in wished state
    public static bool GetButtonMobileOld(string name, Button.Status status)
    {
        for (int i = 0; i < CurrentButtons.Count; i++)
        {
            if (CurrentButtons[i].name == name)
            {
                if (CurrentButtons[i].status == status)
                    return true;
                else
                    return false;
            }
        }
        return false;
    }
    //add pressed button on list that will be verified in input.getbutton
    public static void PressButtonMobileOld(string name, Button.Status status)
    {
        if (status == Button.Status.down)
        {
            ButtonsPool[CurrentButtonPoolIndex].name = name;
            ButtonsPool[CurrentButtonPoolIndex].status = status;
            ButtonsPool[CurrentButtonPoolIndex].Time = time;
            CurrentButtons.Add(ButtonsPool[CurrentButtonPoolIndex]);
            CurrentButtonPoolIndex++;
            if (CurrentButtonPoolIndex >= NumberOfSimultaneousButtons)
                CurrentButtonPoolIndex = 0;
        }
        else if (status == Button.Status.up)
        {
            for (int i = 0; i < CurrentButtons.Count; i++)
            {
                if (CurrentButtons[i].name == name)
                {
                    CurrentButtons[i].status = status;
                    CurrentButtons[i].Time = time;
                }
            }
        }
    }
    //add pressed button on list that will be verified in input.getbutton
    public static void AxisUpdateMobileOld(string name, float value)
    {
        Axes.Add(new Axis(name, value, Time.time));
    }
    //Clear all spired buttons from list
    public static void ClearUpsAndEvolveDownToPressedOld() //!+ Also detect Presseds based in button down expiration
    {
        for (int i = 0; i < CurrentButtons.Count; i++)
        {
            if (CurrentButtons[i].Time < time)
            {
                if (CurrentButtons[i].status == Button.Status.up)
                {
                    CurrentButtons.RemoveAt(i);
                }
                else if (CurrentButtons[i].status == Button.Status.down)
                {
                    CurrentButtons[i].status = Button.Status.pressed;
                }
            }
        }

        for (int i = 0; i < Axes.Count; i++)
        {
            if (Axes[i].Time < time)
            {
                Axes.RemoveAt(i);
            }
        }
    }
    //!++ Autoconfigure script order and presence in hierarchy
#if UNITY_EDITOR
    bool OrderOfScriptChanged;
#endif
    public void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
            return;

        Input.Autoconfigure();
#if UNITY_EDITOR
        if (!OrderOfScriptChanged)
        {
            // Get the name of the script we want to change it's execution order
            string scriptName = typeof(Input).Name;

            // Iterate through all scripts (Might be a better way to do this?)
            foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
            {
                // If found our script
                if (monoScript.name == scriptName)
                {
                    MonoImporter.SetExecutionOrder(monoScript, -9999);
                }
            }
            OrderOfScriptChanged = true;
        }
#endif
    }

    [System.Serializable]
    public class Button
    {
        public string name;
        public enum Status { down, pressed, up, released }
        public Status status;
        public float Time;
    }
    [System.Serializable]
    public class Axis
    {
        public string name;
        public float Value;
        public float Time;

        public Axis(string name, float value, float time)
        {
            this.name = name;
            this.Value = value;
            this.Time = time;
        }
    }
    #endregion
    private static List<string> ButtonsDown = new List<string>();
    private static List<string> ButtonsHold = new List<string>();
    private static List<string> ButtonsUp = new List<string>();
    private static int lastFrameUpdated;
    public static bool GetButtonDownMobile(string name)
    {
        if (ButtonsDown.Contains(name))
        {
            if (Time.frameCount != lastFrameUpdated)
            {
                ButtonsDown.Clear();
                ButtonsUp.Clear();
                ButtonsHold.Clear();
            }
            else
            {
                return true;
            }
        }
        return false;
    }
    public static bool GetButtonMobile(string name)
    {
        if (ButtonsHold.Contains(name))
        {
            if (Time.frameCount != lastFrameUpdated)
            {
                ButtonsDown.Clear();
                ButtonsUp.Clear();
                ButtonsHold.Clear();
            }
            else
            {
                return true;
            }
        }
        return false;
    }
    public static bool GetButtonUpMobile(string name)
    {
        if (ButtonsUp.Contains(name))
        {
            if (Time.frameCount != lastFrameUpdated)
            {
                ButtonsDown.Clear();
                ButtonsUp.Clear();
                ButtonsHold.Clear();
            }
            else
            {
                return true;
            }
        }
        return false;
    }
    public static float GetAxisMobileOld(string axis)
    {
        for (int i = 0; i < Axes.Count; i++)
        {
            if (Axes[i].name == axis)
            {
                return Axes[i].Value;
            }
        }
        return 0f;
    }
    public static void PressButtonDownMobile(string buttonName)
    {
        if (Time.frameCount != lastFrameUpdated)
        {
            ButtonsDown.Clear();
            ButtonsUp.Clear();
            ButtonsHold.Clear();
        }

        lastFrameUpdated = Time.frameCount;
        ButtonsDown.Add(buttonName);
    }
    public static void PressButtonMobile(string buttonName)
    {
        if (Time.frameCount != lastFrameUpdated)
        {
            ButtonsDown.Clear();
            ButtonsUp.Clear();
            ButtonsHold.Clear();
        }

        lastFrameUpdated = Time.frameCount;
        ButtonsHold.Add(buttonName);
    }
    public static void PressButtonUpMobile(string buttonName)
    {
        if (Time.frameCount != lastFrameUpdated)
        {
            ButtonsDown.Clear();
            ButtonsUp.Clear();
            ButtonsHold.Clear();
        }

        lastFrameUpdated = Time.frameCount;
        ButtonsUp.Add(buttonName);
    }
    public static float GetAxisMobile(string axisName)
    {
        if (MobileAxes == null || !MobileAxes.ContainsKey(axisName))
            return 0f;

        return MobileAxes[axisName];
    }
    private static IDictionary<string, float> MobileAxes;
    public static void SetAxisMobile(string Name, float value)
    {
        #if UNITY_EDITOR
        if (MobileAxes==null || !MobileAxes.ContainsKey(Name)){
            Debug.LogError("Register the axis before: Input.RegisterAxisMobile([Axis Name]);");
            return;
        }
        #endif
        MobileAxes[Name] = value;
    }
    public static void RegisterAxisMobile(string Name)
    {
        if (MobileAxes == null)
            MobileAxes = new Dictionary<string, float>();

        if (MobileAxes.ContainsKey(Name))
            return;

        MobileAxes.Add(Name, 0f);
    }

}
