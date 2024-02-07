using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Splines;

[CustomEditor(typeof(Spline))]
public class Splineinspector : Editor
{
    enum ToolMode
    {
        None,
        ModifyNode,
        ModifyConnection,
        CreateNode,
        RemoveNode
    }

    [SerializeField] List<int> selectedNodes;
    Tool lastTool = Tool.None;
    ToolMode currentMode = ToolMode.None;
    VisualElement buttonContainer, toolContainer;
    ButtonThatCanBeDisabled modifyConnectionButton, clearSelectionButton;
    Spline spline;
    private static readonly Color ActiveNodeColor = new(255, 232, 0);
    private static readonly Color SelectedNodeColor = new(255, 140, 0);
    private static readonly Color UnselectedNodeColor = new(144, 155, 184);
    private static readonly Color ControlPointColor = new(0, 62, 158);
    private static readonly Color ControlConnectionColor = new(135, 135, 135);
    private static readonly Color SplineColor = Color.white;

    readonly struct PointInfo
    {
        public readonly int indexA;
        public readonly int indexB;
        public readonly Vector3 coord;

        public PointInfo(Vector3 coord, int indexA, int indexB = -1)
        {
            this.indexA = indexA;
            this.indexB = indexB;
            this.coord = coord;
        }
    }

    private void OnSceneGUI()
    {
        void drawBeziers(Node node)
        {
            Handles.color = SplineColor;
            foreach (var connection in node.Connections.Where(c => c.nodeIndex > node.selfIndex))
            {
                var other = spline.Nodes[connection.nodeIndex];
                Vector3 controlA = spline.transform.position + node.localPosition + connection.controlPosition;
                Vector3 controlB = spline.transform.position + other.localPosition + other.GetControlFor(node);
                Handles.DrawBezier(spline.transform.position + node.localPosition, // local + main pos
                                   spline.transform.position + other.localPosition, // other local + main pos
                                   controlA, controlB, Color.white, null, 10f);
            }
        }

        //failsafe as this function seems to be called twice before create inspector gui is actually called?
        if (spline == null) return; 

        Undo.RecordObject(serializedObject.targetObject, "Moved Spline Nodes"); // TODO: make so it actually records movement only
        
        switch (currentMode)
        {
            case ToolMode.None:
                foreach (var node in spline.Nodes)
                {
                    Handles.color = UnselectedNodeColor;
                    Handles.SphereHandleCap(0, spline.transform.position + node.localPosition, Quaternion.identity, 1f, EventType.Ignore);

                    drawBeziers(node);
                }
                break;
            case ToolMode.ModifyNode:
                break;
            case ToolMode.ModifyConnection: // should only enable when >1 node
                //List<PointInfo> pointsClicked = new();

                if (selectedNodes.Count != 0)
                {
                    Handles.color = SelectedNodeColor;
                    if (Handles.Button(spline.transform.position + spline.Nodes[selectedNodes[0]].localPosition, Quaternion.identity, .2f, .1f, Handles.SphereHandleCap))
                    {
                        //pointsClicked.Add(new(spline.transform.position + spline.Nodes[selectedNodes[0]].localPosition, selectedNodes[0]));
                        selectedNodes.Clear();
                    }

                    Handles.color = UnselectedNodeColor;
                    foreach (var index in spline.Nodes[selectedNodes[0]].Connections.Select(c => c.nodeIndex))
                    {
                        if (Handles.Button(spline.transform.position + spline.Nodes[index].localPosition, Quaternion.identity, .2f, .1f, Handles.SphereHandleCap))
                        {
                            //pointsClicked.Add(new(spline.transform.position + spline.Nodes[index].localPosition, index));
                            selectedNodes.Add(index);
                            BridgeNodes();
                        }
                    }
                }

                foreach (var node in spline.Nodes)
                {
                    if (selectedNodes.Count == 0)
                    {
                        if (Handles.Button(spline.transform.position + node.localPosition, Quaternion.identity, .2f, .1f, Handles.SphereHandleCap))
                        {
                            //pointsClicked.Add(new(spline.transform.position + node.localPosition, node.selfIndex));
                            selectedNodes.Add(node.selfIndex);
                        }
                    }

                    //if !clickedNode check if hovering/clicking on spline need to sort by dist
                    Handles.color = SplineColor;
                    foreach (var connection in node.Connections.Where(c => c.nodeIndex > node.selfIndex))
                    {
                        var other = spline.Nodes[connection.nodeIndex];
                        BezierFunctions.Bezier bezier = new BezierFunctions.Bezier(
                            spline.transform.position + node.localPosition,
                            spline.transform.position + node.localPosition + connection.controlPosition,
                            spline.transform.position + other.localPosition + other.GetControlFor(node),
                            spline.transform.position + other.localPosition);
                        Handles.DrawBezier(bezier.p0, bezier.p3, bezier.p1, bezier.p2, Color.white, null, 10f);

                        if(BezierFunctions.IsRayIntersectingBezier(bezier, HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), Mathf.Max(0.1f, HandleUtility.GetHandleSize((bezier.p0 + bezier.p3 + bezier.p1 + bezier.p2) / 4)), out Vector3 intersection))
                        {

                        }
                    }
                }
                break;
            case ToolMode.CreateNode:
                foreach (var node in spline.Nodes)
                {
                    Handles.color = UnselectedNodeColor;
                    if (Handles.Button(spline.transform.position + node.localPosition, Quaternion.identity, .2f, .1f, Handles.SphereHandleCap))
                    {
                        CreateNode(node);
                    }

                    drawBeziers(node);
                }
                break;
            case ToolMode.RemoveNode:
                break;
            default:
                break;
        }

        /*for (int i = 0; i < spline.nodes.Count; i++)
        {
            Node currentNode = spline.nodes[i];
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Debug.Log(ray.direction);

            if (isEditing)
            {
                if (selectedNodes.Contains(i))
                {
                    currentNode.localPosition = Handles.PositionHandle(currentNode.localPosition + spline.transform.position, Quaternion.identity) - spline.transform.position;

                    Vector3 worldPos = currentNode.localPosition + spline.transform.position;
                    if (currentNode.controlMode != Node.ControlMode.None)
                    {
                        Handles.color = ControlPointColor;
                        currentNode.controlPointA = Handles.FreeMoveHandle(currentNode.controlPointA + worldPos, Quaternion.identity, 0.1f, Vector3.one, Handles.SphereHandleCap) - worldPos;
                        currentNode.controlPointB = Handles.FreeMoveHandle(currentNode.controlPointB + worldPos, Quaternion.identity, 0.1f, Vector3.one, Handles.SphereHandleCap) - worldPos;
                        Handles.color = ControlConnectionColor;
                        Vector3[] positions = new Vector3[] { currentNode.controlPointA + worldPos, worldPos, worldPos, currentNode.controlPointB + worldPos };
                        Handles.DrawDottedLines(positions, 4f);
                    }

                    Handles.color = selectedNodes[0] == i ? ActiveNodeColor : SelectedNodeColor;
                    if (Handles.Button(worldPos, Quaternion.LookRotation(SceneView.currentDrawingSceneView.camera.transform.forward, SceneView.currentDrawingSceneView.camera.transform.up), .2f, .1f, Handles.SphereHandleCap))
                        selectedNodes.Remove(i);
                    UpdateButtons();
                }
                else
                {
                    Vector3 worldPos = currentNode.localPosition + spline.transform.position;
                    //Debug.Log(SceneView.currentDrawingSceneView.camera.transform.position);
                    //Debug.Log(Quaternion.FromToRotation(SceneView.currentDrawingSceneView.camera.transform.position, worldPos));
                    Handles.color = UnselectedNodeColor;
                    if (Handles.Button(worldPos, Quaternion.LookRotation(SceneView.currentDrawingSceneView.camera.transform.forward, SceneView.currentDrawingSceneView.camera.transform.up), .2f, .1f, Handles.SphereHandleCap))
                    {
                        if (!Event.current.control)
                            selectedNodes.Clear();
                        selectedNodes.Add(i);
                        UpdateButtons();
                    }
                }
            }

            Handles.color = SplineColor;
            foreach (int nodeIndex in spline.nodes[i].connectedNodes)
            {
                if (nodeIndex > i)
                {
                    var otherNode = spline.nodes[nodeIndex];
                    Vector3 controlA = currentNode.controlMode == Spline.Node.ControlMode.None ? // if none angle straight at the next node
                                        otherNode.localPosition + spline.transform.position :
                                        currentNode.localPosition + currentNode.controlPointB + spline.transform.position; // local + control + main pos
                    Vector3 controlB = otherNode.controlMode == Spline.Node.ControlMode.None ?
                                        currentNode.localPosition + spline.transform.position :
                                        otherNode.localPosition + otherNode.controlPointA + spline.transform.position; // other local + control + main pos


                    Handles.DrawBezier(currentNode.localPosition + spline.transform.position, // local + main pos
                        otherNode.localPosition + spline.transform.position, // other local + main pos
                        controlA, controlB, Color.white, null, 10f);
                }
            }
        }
        */

        serializedObject.ApplyModifiedProperties();


        if (currentMode != ToolMode.None && Tools.current != Tool.None)
            Tools.current = Tool.None;
    }

    public override VisualElement CreateInspectorGUI()
    {
        // Clone a visual tree from UXML
        VisualTreeAsset splineXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/SplineEditor.uxml");
        VisualElement inspector = splineXML.CloneTree();

        spline = target as Spline;

        //TODO: could probably use clickedWithEventInfo to avoid lambda, but due to lack of documentation, haven't used it yet
        buttonContainer = inspector.Q<VisualElement>("buttonContainer");
        buttonContainer.Q<ButtonThatCanBeDisabled>("btnModifyNodes").clickable.clicked += () => { currentMode = ToolMode.ModifyNode; SwitchInspector(); };
        modifyConnectionButton = buttonContainer.Q<ButtonThatCanBeDisabled>("btnModifyConnections");
        modifyConnectionButton.clickable.clicked += () => { currentMode = ToolMode.ModifyConnection; SwitchInspector(); };
        buttonContainer.Q<ButtonThatCanBeDisabled>("btnCreateNodes").clickable.clicked += () => { currentMode = ToolMode.CreateNode; SwitchInspector(); };
        buttonContainer.Q<ButtonThatCanBeDisabled>("btnRemoveNodes").clickable.clicked += () => { currentMode = ToolMode.RemoveNode; SwitchInspector(); };
        //buttonContainer.Q<ButtonThatCanBeDisabled>("btnRemoveNodes").clickable.clicked += () => { Debug.Log(BezierFunctions.EstimateLength(new(new(0, 0), new(1, 1), new(2, 1), new(3, 0)), 1)); };
        
        toolContainer = inspector.Q<VisualElement>("toolContainer");
        toolContainer.Q<ButtonThatCanBeDisabled>("btnCancel").clickable.clicked += () => { currentMode = ToolMode.None; SwitchInspector(); };
        toolContainer.Q<Toggle>("toggle").RegisterValueChangedCallback(AlternateTool);

        selectedNodes = new();

        // Return the finished inspector UI
        return inspector;
    }

    private void SwitchInspector()
    {
        if (currentMode == ToolMode.None)
        {
            selectedNodes.Clear();
            buttonContainer.style.display = DisplayStyle.Flex;
            toolContainer.style.display = DisplayStyle.None;
            Tools.current = lastTool;
        }
        else
        {
            buttonContainer.style.display = DisplayStyle.None;
            toolContainer.style.display = DisplayStyle.Flex;
            lastTool = Tools.current;

            toolContainer.Q<Label>("descriptionLabel").text =
                currentMode == ToolMode.ModifyNode ? "Use the move handles to reposition the nodes and their control points" :
                currentMode == ToolMode.ModifyConnection ? "Select two unconnected nodes to join them\nSelect a connection to break it" :
                currentMode == ToolMode.CreateNode ? "Select a node to create a new node around it\nSelect a connection to insert a node at the mouse position" :
                currentMode == ToolMode.RemoveNode ? "Select a node to remove it"
                : "Error: you shouldn't be seeing this";

            toolContainer.Q<Toggle>("toggle").value = false;
            toolContainer.Q<Toggle>("toggle").style.display = currentMode == ToolMode.RemoveNode ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void AlternateTool(ChangeEvent<bool> change)
    {
        const string txt = "\nAll nodes connected to the deleted one will remain linked";
        var label = toolContainer.Q<Label>();

        if (change.newValue)
            label.text += txt;
        else
            label.text = label.text.Remove(label.text.Length - txt.Length);
    }

    private void CreateNode(Node node)
    {
        //TODO: Make camera actually turn? (if I decide to focus on new node
        Undo.RecordObject(serializedObject.targetObject, "Created Node");
        spline.CreateNode(node.selfIndex, node.localPosition + 
            Quaternion.Euler(new(Random.Range(0f, 90f), Random.Range(0, 360f), 0)) * Vector3.right); // gives any point in a 1 unit hemisphere (y >= node)
        //SceneView.lastActiveSceneView.camera.transform.LookAt(spline.Nodes[^1].localPosition + spline.transform.position);
        //SceneView.lastActiveSceneView.Frame(new Bounds(spline.Nodes[^1].localPosition + spline.transform.position, Vector3.one * 5), false);
        EditorUtility.SetDirty(serializedObject.targetObject);
    }

    private void DissolveNodes()
    {
        bool allSelected = selectedNodes.Count == spline.Nodes.Count;

        Undo.RecordObject(serializedObject.targetObject, "Dissolved Nodes");

        selectedNodes = selectedNodes.OrderByDescending(i => i).ToList();
        for(int i = 0; i < selectedNodes.Count; i++)
        {
            spline.DissolveNode(selectedNodes[i]);
        }

        selectedNodes.Clear();

        if (allSelected)
        {
            selectedNodes.Add(0);
            SceneView.lastActiveSceneView.camera.transform.LookAt(spline.Nodes[^1].localPosition + spline.transform.position);
            SceneView.lastActiveSceneView.Frame(new Bounds(spline.Nodes[^1].localPosition + spline.transform.position, Vector3.one * 5), false);
        }
        EditorUtility.SetDirty(serializedObject.targetObject);
    }

    private void SubdivideConnection()
    {
        Undo.RecordObject(serializedObject.targetObject, "Subdivided Connection");
        spline.InsertNode(selectedNodes[0], selectedNodes[1]);
        selectedNodes.Clear();
        selectedNodes.Add(spline.Nodes.Count - 1);
        EditorUtility.SetDirty(serializedObject.targetObject);
    }

    private void BridgeNodes()
    {
        Undo.RecordObject(serializedObject.targetObject, "Bridged Nodes");
        spline.BridgeNodes(selectedNodes[0], selectedNodes[1]);
        EditorUtility.SetDirty(serializedObject.targetObject);
    }

    private void SeverConnection()
    {
        Undo.RecordObject(serializedObject.targetObject, "Severed Connection");
        spline.SeparateNodes(selectedNodes[0], selectedNodes[1]);
        EditorUtility.SetDirty(serializedObject.targetObject);
    }

    void OnEnable()
    {
        lastTool = Tools.current;
    }

    void OnDisable()
    {
        Tools.current = lastTool;
    }
}
