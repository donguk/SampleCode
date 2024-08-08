using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ClimbGames.Client
{
    class SimpleTreeView : TreeView<TreeData>
    {
        public SimpleTreeView(TreeViewState state, TreeModel<TreeData> model) : base(state, model)
        {
            
        }
    }
}