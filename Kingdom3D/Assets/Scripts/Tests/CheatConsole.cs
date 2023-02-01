using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Security.Cryptography;
using UnityEngine.EventSystems;
using UnityEngine.Windows;

public class CheatConsole : MonoBehaviour
{
    [SerializeField] TMP_InputField console;
    [SerializeField] TextMeshProUGUI logBox, suggestion;
    [SerializeField] Transform suggestionParent;
    string[] methods;
    string[][] splitMethods;
    MethodInfo[] methodInfos;
    string[] pastLogs;
    float paddingOffset;
    bool justRestarted = false;
    static readonly System.Type[] types = new System.Type[] { typeof(string), typeof(float), typeof(double), typeof(bool), typeof(int) };
    static readonly string[] typeNames = new string[] { "string", "float", "double", "bool", "int" };
    List<string> lastInputs = new List<string>();
    int inputIndex = 0;

    private void Awake()
    {
        static bool checkType(MethodInfo m)
        {
            var infos = m.GetParameters();
            if (infos.Length == 0 || infos[0].ParameterType != System.Type.GetType("System.String&") ||
            !infos[0].ParameterType.IsByRef || infos[0].IsOut)
            {
                return false;
            }

            for (int i = 1; i < infos.Length; i++)
                if (!types.Contains(infos[i].ParameterType))
                    return false;

            return true;
        }

        methodInfos = typeof(ConsoleCommands)
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => m.ReturnType == typeof(bool))
            .Where(m => checkType(m))
            .ToArray();
        methods = methodInfos.Select(m => m.Name).ToArray();
        splitMethods = methods.Select(m => m.Split("_")).ToArray();
        for (int i = 0; i < methodInfos.Length; i++)
        {
            var parameters = methodInfos[i].GetParameters();
            List<string> args = new();
            for (int o = 1; o < parameters.Length; o++)
            {
                args.Add(string.Format("[{0}:{1}]", parameters[o].Name, typeNames[System.Array.IndexOf(types, parameters[o].ParameterType)]));
            }
            splitMethods[i] = splitMethods[i].Concat(args).ToArray();
        }
        pastLogs = new string[6];
        GetComponent<PlayerInput>().DeactivateInput();
        paddingOffset = suggestionParent.GetComponent<VerticalLayoutGroup>().padding.left;
        console.ActivateInputField();
        lastInputs.Add("");
    }

    public void ToggleConsole(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            gameObject.SetActive(true);
            foreach (var input in PlayerInput.all)
            {
                if (input.defaultActionMap == "Player")
                    input.DeactivateInput();
            }
            console.Select();
            GetComponent<PlayerInput>().ActivateInput();
        }
    }

    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            foreach (var input in PlayerInput.all)
            {
                if (input.defaultActionMap == "Player")
                    input.ActivateInput();
            }
            HideConsole();
        }
    }

    public void HideConsole()
    {
        gameObject.SetActive(false);
        console.text = "";
        suggestionParent.gameObject.SetActive(false);
        foreach(Transform sugTransform in suggestionParent)
        {
            Destroy(sugTransform.gameObject);
        }
        GetComponent<PlayerInput>().DeactivateInput();
    }

    public void ValueChanged()
    {
        static int Sum(int[] ints)
        {
            int sum = 0;
            foreach (int i in ints)
                sum += i;
            return sum;
        }

        static bool MethodIsSuitable(string[] splitMethod, string[] splitInput)
        {
            if (splitMethod.Length < splitInput.Length)
                return false;

            for (int i = 0; i < splitInput.Length - 1; i++)
            {
                if (!(splitMethod[i] == splitInput[i] || splitMethod[i].StartsWith("[")))
                    return false;
            }

            if (!(splitMethod[splitInput.Length - 1].StartsWith(splitInput[^1]) || splitMethod[splitInput.Length - 1].StartsWith("[")))
                return false;

            return true;
        }

        if (justRestarted)
        {
            justRestarted = false;
            return;
        }
        string input = console.text;

        string[] splitInput = input.Split(" ");
        string[] options = splitMethods.Where(s => MethodIsSuitable(s, splitInput)).Select(s => s[splitInput.Length - 1]).Distinct().ToArray();
        int suggestionCount = suggestionParent.childCount;

        if (suggestionCount > options.Length)
        {
            for (int i = 0; i < suggestionCount; i++)
            {
                if (i >= options.Length)
                    Destroy(suggestionParent.GetChild(i).gameObject);
                else
                    suggestionParent.GetChild(i).GetComponent<TextMeshProUGUI>().text = options[i];
            }
        }
        else if (suggestionCount < options.Length)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (i > suggestionCount - 1)
                    Instantiate(suggestion, suggestionParent).GetComponent<TextMeshProUGUI>().text = options[i];
                else
                    suggestionParent.GetChild(i).GetComponent<TextMeshProUGUI>().text = options[i];
            }
        }
        else
        {
            for (int i = 0; i < options.Length; i++)
            {
                suggestionParent.GetChild(i).GetComponent<TextMeshProUGUI>().text = options[i];
            }
        }

        if (options.Length == 0)
        {
            suggestionParent.gameObject.SetActive(false);
            return;
        }
        else if (!suggestionParent.gameObject.activeSelf)
            suggestionParent.gameObject.SetActive(true);


        var info = console.textComponent.textInfo.characterInfo;
        Vector3 localPos;
        if (splitInput.Length == 1)
        {
            localPos = console.textComponent.textInfo.characterInfo[0].topLeft;
        }
        else
        {
            var wordBeginIndex = Sum(splitInput.Select(o => o.Length + 1).ToArray()) - splitInput[^1].Length - 2;
            localPos = console.textComponent.textInfo.characterInfo[wordBeginIndex].topRight;
        }

        Vector3 worldSpacePos = console.textViewport.TransformPoint(localPos);
        Vector3 consoleSpacePos = console.transform.parent.InverseTransformPoint(worldSpacePos);
        var sugestionPos = suggestionParent.localPosition;
        sugestionPos.x = consoleSpacePos.x - paddingOffset;
        suggestionParent.localPosition = sugestionPos;

        LayoutRebuilder.ForceRebuildLayoutImmediate(suggestionParent.transform as RectTransform);
    }

    public void AutoSelect(InputAction.CallbackContext context)
    {

    }

    public void MemUp(InputAction.CallbackContext context)
    {

    }

    public void MemDown(InputAction.CallbackContext context)
    {

    }

    public void Submit(InputAction.CallbackContext context)
    {
        static bool EqualsMethod(string[] splitMethod, string[] splitInput)
        {
            if (splitMethod.Length != splitInput.Length)
                return false;

            for (int i = 0; i < splitInput.Length; i++)
                if (!(splitMethod[i] == splitInput[i] || splitMethod[i].StartsWith("[")))
                    return false;

            return true;
        }

        static string JoinMethod(string[] splitMethod)
        {
            string method = "";
            foreach (var command in splitMethod)
            {
                if (command.StartsWith("[")) break;
                method += command + "_";
            }
            return method.Remove(method.Length - 1, 1); // remove the last _
        }

        if (!context.performed) return;

        console.ActivateInputField();
        suggestionParent.gameObject.SetActive(false);
        justRestarted = true;

        string input = console.text.Trim();
        console.text = "";
        if (input == "")
            return;

        inputIndex = lastInputs.Count;
        if (lastInputs[^1] != input)
            lastInputs.Add(input);

        string[] splitInput = input.Split(" ");
        string[][] currentMethods = splitMethods.Where(m => EqualsMethod(m, splitInput)).ToArray();

        if (currentMethods.Length == 0)
        {
            PrintError("command not found or is missing parameters");
            return;
        }

        if (currentMethods.Length != 1)
            currentMethods = currentMethods.OrderBy(m => m.Where(s => s.StartsWith("[")).Count()).ToArray();

        for (int i = 0; i < currentMethods.Length; i++)
        {
            var parameters = methodInfos[System.Array.IndexOf(methods, JoinMethod(currentMethods[i]))].GetParameters();
            if (i == currentMethods.Length - 1)
            {
                object[] paramsToPass = new object[currentMethods[i].Where(s => s.StartsWith("[")).Count() + 1];
                paramsToPass[0] = "";
                for (int o = 1; o < paramsToPass.Length; o++)
                {
                    try
                    {
                        paramsToPass[o] = System.Convert.ChangeType(splitInput[^(parameters.Length - o)], parameters[o].ParameterType);
                    }
                    catch (System.InvalidCastException)
                    {
                        PrintError(string.Format("parameter {0} is not of type {1}", i, parameters[i].ParameterType));
                        return;
                    }
                    catch (System.OverflowException)
                    {
                        PrintError(string.Format("parameter {0} is out of range for type {1}", i, parameters[i].ParameterType));
                        return;
                    }
                    catch (System.FormatException)
                    {
                        PrintError(string.Format("parameter {0} is of wrong format for type {1}", i, parameters[i].ParameterType));
                        return;
                    }
                }

                if (!(bool)methodInfos[System.Array.IndexOf(methods, JoinMethod(currentMethods[i]))].Invoke(null, paramsToPass))
                {
                    if ((string)paramsToPass[0] != "")
                        PrintError((string)paramsToPass[0]);
                    else
                        PrintError("Something went wrong");
                }
                else if ((string)paramsToPass[0] != "")
                    UpdateLog((string)paramsToPass[0]);
                return;
            }
            else
            {
                object[] paramsToPass = new object[currentMethods[i].Where(s => s.StartsWith("[")).Count() + 1];
                paramsToPass[0] = "";
                for (int o = 1; o < paramsToPass.Length; o++)
                {
                    try
                    {
                        paramsToPass[o] = System.Convert.ChangeType(splitInput[^(parameters.Length - o)], parameters[o].ParameterType);
                    }
                    catch
                    {
                        goto next;
                    }
                }

                if (!(bool)methodInfos[System.Array.IndexOf(methods, JoinMethod(currentMethods[i]))].Invoke(null, paramsToPass))
                {
                    if ((string)paramsToPass[0] != "")
                        PrintError((string)paramsToPass[0]);
                    else
                        PrintError("something went wrong");
                }
                else if ((string)paramsToPass[0] != "")
                    UpdateLog((string)paramsToPass[0]);
                return;
            }
        next:
            continue;
        }
    }

    public void OldSubmit(InputAction.CallbackContext context)
    {
        if(!context.performed) return;

        string input = console.text.Trim();
        console.text = "";
        if (input == "")
            return;

        List<string> splitInput = new(input.Split(" "));
        string currentMethod = "";

        for (int i = 0; i < splitInput.Count; i++) 
        {
            if (methods.Where(s => s.StartsWith(currentMethod + "_" + splitInput[i] + "_") || s == currentMethod + "_" + splitInput[i]).Count() > 0)
            {
                currentMethod += "_" + splitInput[i];

                if (i == splitInput.Count - 1)
                    splitInput.RemoveRange(0, i + 1);

            }
            else
            {
                splitInput.RemoveRange(0, i);
                break;
            }
        }

        if (!methods.Contains(currentMethod))
        {
            if (currentMethod == "")
                PrintError(string.Format("unknown command \"{0}\"", splitInput[0]));
            else
                PrintError(string.Format("unknown command \"{0}\"", currentMethod.Replace("_", " ") + " " + splitInput[0]));
            return;
        }

        var parameters = methodInfos[System.Array.IndexOf(methods, currentMethod)].GetParameters();

        if (splitInput.Count > parameters.Length - 1)
        {
            PrintError("too many parameters provided");
        }

        object[] paramsToPass = new object[splitInput.Count + 1];
        paramsToPass[0] = "";
        for (int i = 1; i < parameters.Length; i++)
        {
            if (i == splitInput.Count)
            {
                PrintError(string.Format("parameter {0} missing", i));
                return;
            }

            try
            { 
                paramsToPass[i] = System.Convert.ChangeType(splitInput[i], parameters[i].ParameterType);
            }
            catch (System.InvalidCastException)
            {
                PrintError(string.Format("parameter {0} is not of type {1}", i, parameters[i].ParameterType));
                return;
            }
            catch (System.OverflowException)
            {
                PrintError(string.Format("parameter {0} is out of range for type {1}", i, parameters[i].ParameterType));
                return;
            }
            catch (System.FormatException)
            {
                PrintError(string.Format("parameter {0} is of wrong format for type {1}", i, parameters[i].ParameterType));
                return;
            }
        }

        if ((bool)methodInfos[System.Array.IndexOf(methods, currentMethod)].Invoke(null, paramsToPass))
        {
            if ((string)paramsToPass[0] != "")
                PrintError((string)paramsToPass[0]);
            else
                PrintError("Something went wrong");
        }
        else if ((string)paramsToPass[0] != "")
            UpdateLog((string)paramsToPass[0]);
    }

    void PrintError(string msg)
    {
        UpdateLog("<color=red>Error: " + msg + "</color>");
    }

    void UpdateLog(string msg)
    {
        for (int i = 1; i < pastLogs.Length; i++)
        {
            pastLogs[i - 1] = pastLogs[i];
        }
        pastLogs[^1] = msg;

        logBox.text = "";

        foreach (var log in pastLogs)
            if (!string.IsNullOrEmpty(log))
                logBox.text += log + "\n";
    }
}
