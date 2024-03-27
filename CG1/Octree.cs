using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Media = System.Windows.Media;

namespace CG1;

public class OctreeNode
{
    public OctreeNode(int level, OctreeQuantizer parent)
    {
        if (level < OctreeQuantizer.MaxDepth - 1)
            parent.AddLevelNode(level, this);
    }
    public Color Color { get; set; } = new();
    public Media.Color GetColor() => Color.GetNormalizedColor();
    public int PaletteIndex { get; set; }
    private OctreeNode?[] Children { get; } = new OctreeNode[8];

    public bool IsLeaf => Color.PixelCount > 0;

    public List<OctreeNode> GetLeafNodes()
    {
        var leafNodes = new List<OctreeNode>();

        foreach (var node in Children)
        {
            if (node is null)
                continue;

            if (node.IsLeaf)
                leafNodes.Add(node);
            else
                leafNodes.AddRange(node.GetLeafNodes());
        }

        return leafNodes;
    }

    private int GetColorIndex(Color color, int level)
    {
        var index = 0;
        var mask = 0b10000000 >> level;
        if ((color.R & mask) != 0) index |= 0b100;
        if ((color.G & mask) != 0) index |= 0b010;
        if ((color.B & mask) != 0) index |= 0b001;
        return index;
    }

    public void AddColor(Media.Color color, int level, OctreeQuantizer parent)
    {
        if(level >= OctreeQuantizer.MaxDepth)
        {
            Color.Add(new(color));
            return;
        }

        var index = GetColorIndex(new(color), level);

        if (Children[index] is null)
            Children[index] = new(level, parent);

        Children[index]?.AddColor(color, level + 1, parent);
    }

    public int GetPaletteIndex(Color color, int level)
    {
        if (IsLeaf)
            return PaletteIndex;

        var index = GetColorIndex(color, level);
        if (Children[index] is not null)
            return Children[index]!.GetPaletteIndex(color, level + 1);

        foreach(var node in Children)
        {
            if (node is null)
                continue;
            return node.GetPaletteIndex(color, level + 1);
        }

        throw new Exception("Non-leaf is childless");
    }

    public int RemoveLeaves()
    {
        var result = 0;

        for(var i = 0; i < Children.Count(); i++)
        {
            if (Children[i] is null)
                continue;

            Color.Add(Children[i]!.Color);
            Children[i] = null;
            result++;
        }

        return result - 1;
    }
}

public class OctreeQuantizer
{
    public const int MaxDepth = 8;
    public OctreeQuantizer()
    {
        Root = new OctreeNode(0, this);
    }

    private List<OctreeNode?>?[] Levels { get; } = new List<OctreeNode?>?[MaxDepth];
    private OctreeNode Root { get; }
    public void AddColor(Media.Color color) => Root.AddColor(color, 0, this);
    private List<OctreeNode> GetLeafNodes() => Root.GetLeafNodes();

    public List<Media.Color> MakePalette(int colorCount)
    {
        var palette = new List<Media.Color>();
        var paletteIndex = 0;
        var leafCount = GetLeafNodes().Count;

        for(var level = MaxDepth - 1; level >= 0; level--)
        {
            if (Levels[level] is not null)
            {
                foreach(var node in Levels[level]!)
                {
                    if (node is null || node == Root)
                        continue;

                    leafCount -= node.RemoveLeaves();
                    if (leafCount <= colorCount)
                        break;
                }
            }

            if (leafCount <= colorCount)
                break;

            Levels[level] = [];
        }

        foreach(var node in GetLeafNodes())
        {
            if (paletteIndex >= colorCount)
                break;
            if (node.IsLeaf)
                palette.Add(node.GetColor());
            node.PaletteIndex = paletteIndex;
            paletteIndex++;
        }

        return palette;
    }

    public int GetPaletteIndex(Media.Color color) => Root.GetPaletteIndex(new(color), 0);
    public void AddLevelNode(int level, OctreeNode node)
    {
        if (Levels[level] is null)
            Levels[level] = [];

        Levels[level]!.Add(node);
    }
}

public class Color
{
    public Color(int r = 0, int g = 0, int b = 0, int pixelCount = 0)
    {
        R = r;
        G = g;
        B = b;
        PixelCount = pixelCount;
    }
    public Color(Media.Color color)
    {
        R = color.R;
        G = color.G;
        B = color.B;
        PixelCount = 1;
    }

    public int R;
    public int G;
    public int B;
    public int PixelCount;

    public void Add(Color other)
    {
        R += other.R;
        G += other.G;
        B += other.B;
        PixelCount += other.PixelCount;
    }

    public Media.Color GetNormalizedColor()
    {
        var r = (byte)(R / PixelCount);
        var g = (byte)(G / PixelCount);
        var b = (byte)(B / PixelCount);
        return Media.Color.FromRgb(r, g, b);
    }
}