using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RuntimeConsole : MonoBehaviour
{
    public static RuntimeConsole Instance { get; private set; }

    [SerializeField] MonoBehaviour commands;
    [SerializeField] TMP_InputField console;
    [SerializeField] GameObject log, suggestion;
    [SerializeField] Transform suggestionParent, logBox;

    string[] methods;
    string[][] splitMethods;
    MethodInfo[] methodInfos;
    string[] pastLogs;
    bool ignoreUpdateSuggestion = false;
    readonly List<string> memory = new();
    int memoryIndex = 0;
    int autosuggestIndex = -1;

    float paddingOffset;

    static readonly System.Type[] types = new System.Type[] { typeof(string), typeof(float), typeof(double), typeof(bool), typeof(int) };
    static readonly string[] typeNames = new string[] { "string", "float", "double", "bool", "int" };

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

        // setup singleton
        if (Instance == null && commands != null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // get all valid methods from commands as string using reflection
        methodInfos = commands.GetType()
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

        // the rest
        pastLogs = new string[6];
        GetComponent<PlayerInput>().DeactivateInput();
        paddingOffset = 10 - suggestionParent.GetComponent<VerticalLayoutGroup>().padding.left;
        console.ActivateInputField();
        memory.Add("");

    }

    public void ShowConsole()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        LevelController.player.GetComponent<PlayerInput>().DeactivateInput();
        console.Select();
        GetComponent<PlayerInput>().ActivateInput();
    }

    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LevelController.player.GetComponent<PlayerInput>().ActivateInput();
            HideConsole();
        }
    }

    public void HideConsole()
    {
        transform.GetChild(0).gameObject.SetActive(false);
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

        if (ignoreUpdateSuggestion)
        {
            ignoreUpdateSuggestion = false;
            return;
        }
        autosuggestIndex = -1;
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
        sugestionPos.x = consoleSpacePos.x + paddingOffset;
        suggestionParent.localPosition = sugestionPos;

        LayoutRebuilder.ForceRebuildLayoutImmediate(suggestionParent.transform as RectTransform);
    }

    public void AutoSelect(InputAction.CallbackContext context)
    {
        if (!context.performed || !suggestionParent.gameObject.activeSelf || suggestionParent.childCount == 0)
            return;


        string input = console.text;
        string replacement;
        List<string> autofills = new();
        for (int i = 0; i < suggestionParent.childCount; i++)
        {
            var txt = suggestionParent.GetChild(i).GetComponent<TextMeshProUGUI>().text;
            if (!txt.StartsWith("["))
                autofills.Add(txt);
        }

        if (autofills.Count == 0)
            return;

        autosuggestIndex++;
        if (autosuggestIndex > autofills.Count - 1)
            autosuggestIndex = 0;

        if (input.Length == 0)
            replacement = autofills[autosuggestIndex];
        else
        {
            var split = input.Split(' ');
            split[^1] = autofills[autosuggestIndex];
            replacement = string.Join(" ", split);
        }

        if (input == replacement)
            return;

        ignoreUpdateSuggestion = true;
        console.text = replacement;
        console.MoveTextEnd(false);
    }

    public void MemPrev(InputAction.CallbackContext context)
    {
        if (!context.performed || memoryIndex < 1)
            return;

        memoryIndex--;
        console.text = memory[memoryIndex];
        suggestionParent.gameObject.SetActive(false);
    }

    public void MemNext(InputAction.CallbackContext context)
    {
        if (!context.performed || memoryIndex > memory.Count - 2)
            return;

        memoryIndex++;
        console.text = memory[memoryIndex];
        suggestionParent.gameObject.SetActive(false);
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
        ignoreUpdateSuggestion = true;

        string input = console.text.Trim();
        console.text = "";
        if (input == "")
            return;

        if (memory[^1] != input)
            memory.Add(input);
        memoryIndex = memory.Count;

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

                if (!(bool)methodInfos[System.Array.IndexOf(methods, JoinMethod(currentMethods[i]))].Invoke(commands, paramsToPass))
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

                if (!(bool)methodInfos[System.Array.IndexOf(methods, JoinMethod(currentMethods[i]))].Invoke(commands, paramsToPass))
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

    void PrintError(string msg)
    {
        UpdateLog("<color=red>Error: " + msg + "</color>");
    }

    void UpdateLog(string msg)
    {
        Instantiate(log, logBox).GetComponent<TextMeshProUGUI>().text = msg;

        if (logBox.childCount == 21)
            Destroy(logBox.GetChild(0).gameObject);

        LayoutRebuilder.ForceRebuildLayoutImmediate(logBox.transform as RectTransform);
    }
}
