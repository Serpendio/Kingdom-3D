using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Splines
{
    [System.Serializable]
    public class Connection
    {
        public int nodeIndex; // index in nodes of the connection, can't be a pointer as this would duplicate when serialised. serializedreference might work; need more research
        public Vector3 controlPosition; // relative to the node position

        public Connection(int nodeIndex, Vector3 controlPosition)
        {
            this.nodeIndex = nodeIndex;
            this.controlPosition = controlPosition;
        }
    }


    [System.Serializable]
    public class Node
    {
        // if there are exactly 2 connections, the first four modes are valid, otherwise first 2 modes only
        public enum ControlMode
        {
            None,   // Angle straight at the other node
            Split,  // Each control has its own angle and magnitude
            Align,  // Controls share angle, but have their own magnitude
            Mirror, // Controls share angle and magnitude
        }

        [SerializeReference] private List<Connection> connections; // technically readonly, but serializable doesn't allow for readonly (after much bug fixing!)
        public List<Connection> Connections => connections;


        public Vector3 localPosition; // position relative to the gameobject
        public int selfIndex;
        public ControlMode controlMode;

        public Node(Vector3 localPosition)
        {
            //connectedNodes = new List<int>();
            this.localPosition = localPosition;
            controlMode = ControlMode.Split;
            connections = new();

        }

        public void AddConnection(Node otherNode)
        {
            if (!IsConnectedTo(otherNode))
                connections.Add(new(otherNode.selfIndex, (otherNode.localPosition - localPosition).normalized));
        }

        public bool RemoveConnection(Node otherNode)
        {
            int index = GetConnectionIndex(otherNode);
            if (index != -1)
            {
                connections.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool IsConnectedTo(Node node)
        {
            return connections.Any(c => c.nodeIndex == node.selfIndex);
            //return connections.Where(c => c.nodeIndex == node.selfIndex).Contains().Count() == 0
        }

        public int GetConnectionIndex(Node node)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].nodeIndex == node.selfIndex)
                {
                    return i;
                }
            }
            return -1;
        }

        public Vector3 GetControlFor(Node node)
        {
            foreach (var connection in connections)
            {
                if (connection.nodeIndex == node.selfIndex)
                {
                    return connection.controlPosition;
                }
            }
            return Vector3.zero;
        }

        public void UpdateConnectingNodeIndex(int connectionIndex, int newNodeIndex)
        {
            connections[connectionIndex].nodeIndex = newNodeIndex;
        }

        public void DecrementConnectingNodeIndex(int connectionIndex)
        {
            connections[connectionIndex].nodeIndex--;
        }

        public void IncrementConnectingNodeIndex(int connectionIndex)
        {
            connections[connectionIndex].nodeIndex++;
        }

        public void UpdateControlPosition(int connectionIndex, Vector3 newPosition)
        {
            connections[connectionIndex].controlPosition = newPosition;
        }
    }
}