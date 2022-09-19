
using System;
using System.Linq;
using Godot;

public class NodePool<T> where T : Node
{
    public T[] Nodes { get; private set; }

    private Node _parent;

    public NodePool(Node parent, int size, Func<int, T> factory)
    {
        _parent = parent;
        Nodes = new T[size];
        for (var i = 0; i < size; i++)
            Nodes[i] = factory(i);
    }

    public T GetNode()
    {
        var first = Nodes.FirstOrDefault(node => !node.IsInsideTree());
        if (first == null) return null;
        _parent.AddChild(first);
        return first;
    }

    public void Release(T node)
    {
        _parent.RemoveChild(node);
    }
}