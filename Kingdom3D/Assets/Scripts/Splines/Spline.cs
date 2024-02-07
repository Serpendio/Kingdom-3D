using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Splines
{
    [System.Serializable]
    [ExecuteAlways]
    public class Spline : MonoBehaviour
    {
        [SerializeReference] private List<Node> nodes;
        public List<Node> Nodes => nodes;

        private void Awake()
        {
            if (nodes == null)
            {
                // set it as a serialized property so that the editor will remember changes made out of play mode
                // we don't want undo as the nodes list should never be null while the spline component exists
                /*SerializedObject serializedObject = new(this);
                var nodeProperty = serializedObject.FindProperty("nodes");
                nodeProperty.SetValueNoRecord(new List<Node>() { new(Vector3.zero) });
                EditorUtility.SetDirty(this);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                new SerializedObject(nodeProperty.GetArrayElementAtIndex(0).objectReferenceValue).ApplyModifiedPropertiesWithoutUndo();*/
                nodes = new List<Node>() { new(Vector3.zero) };
                new SerializedObject(this).ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public bool CreateNode(int connectionIndex, Vector3 localPosition)
        {
            if (connectionIndex < 0 || connectionIndex >= nodes.Count) // index out of bounds
                return false;

            //create the node then give it and its connection references to each other
            var node = new Node(localPosition) { selfIndex = nodes.Count };
            node.AddConnection(nodes[connectionIndex]);
            nodes[connectionIndex].AddConnection(node);
            nodes.Add(node);

            return true;
        }

        public bool InsertNode(int startIndex, int endIndex, bool subdivide = true)
        {
            if (startIndex < 0 || startIndex >= nodes.Count || endIndex < 0 || endIndex >= nodes.Count) // index out of bounds
                return false;

            if (subdivide && nodes[startIndex].IsConnectedTo(nodes[endIndex])) // nodes are connected so separate them
            {
                SeparateNodes(startIndex, endIndex);
            }

            //create the node then give it and its connections references to each other
            var node = new Node((nodes[startIndex].localPosition + nodes[endIndex].localPosition) / 2f) { selfIndex = nodes.Count };
            node.AddConnection(nodes[startIndex]);
            node.AddConnection(nodes[endIndex]);
            nodes[startIndex].AddConnection(node);
            nodes[endIndex].AddConnection(node);
            nodes.Add(node);

            return true;
        }

        public bool BridgeNodes(int startIndex, int endIndex)
        {
            if (startIndex < 0 || startIndex >= nodes.Count || endIndex < 0 || endIndex >= nodes.Count) // index out of bounds
                return false;

            if (nodes[startIndex].IsConnectedTo(nodes[endIndex])) // nodes already connected
                return true;

            nodes[startIndex].AddConnection(nodes[endIndex]);
            nodes[endIndex].AddConnection(nodes[startIndex]);
            return true;
        }

        public bool SeparateNodes(int startIndex, int endIndex)
        {
            if (startIndex < 0 || startIndex >= nodes.Count || endIndex < 0 || endIndex >= nodes.Count) // index out of bounds
                return false;

            if (!nodes[startIndex].IsConnectedTo(nodes[endIndex])) // nodes already separated
                return true;

            nodes[startIndex].RemoveConnection(nodes[endIndex]);
            nodes[endIndex].RemoveConnection(nodes[startIndex]);
            return true;
        }

        public bool DissolveNode(int index)
        {
            if (index < 0 || index >= nodes.Count) // index out of bounds
                return false;

            if (nodes.Count == 1) // only one node already
            {
                nodes[0] = new(Vector3.zero);
                return true;
            }

            //connect all to first connection
            var connectionIndexes = nodes[index].Connections.Select(c => c.nodeIndex).ToArray();
            System.Array.Sort(connectionIndexes);

            for (int i = 1; i < connectionIndexes.Length; i++)
            {
                // connect each node to the first connection (if they aren't already connected)
                if (!nodes[connectionIndexes[i]].IsConnectedTo(nodes[connectionIndexes[0]]))
                {
                    nodes[connectionIndexes[0]].AddConnection(nodes[connectionIndexes[i]]);
                    nodes[connectionIndexes[i]].AddConnection(nodes[connectionIndexes[0]]);
                }
            }

            RemoveNodeAt(index);
            return true;
        }

        public bool RemoveNodeAt(int removalIndex)
        {
            if (removalIndex < 0 || removalIndex >= nodes.Count) // index out of bounds
                return false;

            if (nodes.Count == 1) // only one node already
            {
                nodes[0] = new(Vector3.zero);
                return true;
            }

            //remove the node from all its connections
            foreach (var connection in nodes[removalIndex].Connections)
            {
                nodes[connection.nodeIndex].RemoveConnection(nodes[removalIndex]);
            }

            //for every node with an index greater than the one to remove
            for (int currentNodeIndex = removalIndex + 1; currentNodeIndex < nodes.Count; currentNodeIndex++)
            {
                var connections = nodes[currentNodeIndex].Connections;
                for (int o = 0; o < connections.Count; o++)
                {
                    if (connections[o].nodeIndex < currentNodeIndex)
                    {
                        Node currentNode = nodes[connections[o].nodeIndex];
                        //currentNode.Connections[currentNode.GetConnectionIndex(nodes[currentNodeIndex])].nodeIndex--;
                        //nodes[currentNodeIndex].Connections[o].nodeIndex--;
                        currentNode.DecrementConnectingNodeIndex(currentNode.GetConnectionIndex(nodes[currentNodeIndex]));
                        nodes[currentNodeIndex].DecrementConnectingNodeIndex(o);
                    }
                }

                //reduce its own stored index
                nodes[currentNodeIndex].selfIndex--;
            }

            // actually remove the node
            nodes.RemoveAt(removalIndex);
            return true;
        }
    }
}
