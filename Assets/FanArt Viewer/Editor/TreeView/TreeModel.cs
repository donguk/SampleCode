using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClimbGames.Client
{
    public class TreeModel<T> : UnityEditor.TreeViewExamples.TreeModel<T> where T : UnityEditor.TreeViewExamples.TreeElement
    {
        public TreeModel(IList<T> data) : base(data)
        {

        }
    }
}