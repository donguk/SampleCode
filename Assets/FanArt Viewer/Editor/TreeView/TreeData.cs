using System.Collections;
using System.Collections.Generic;
using UnityEditor.TreeViewExamples;
using UnityEngine;

namespace ClimbGames.Client
{
    public class TreeData : TreeElement
    {
        public TreeData(string name, int depth, int id) : base(name, depth, id)
        {
            this.name = name;
            this.depth = depth;
            this.id = id;
        }
    }
}
