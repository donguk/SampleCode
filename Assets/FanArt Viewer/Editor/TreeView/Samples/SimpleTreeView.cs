using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ClimbGames.Editor
{
    class SimpleTreeView : TreeView<TreeElement>
    {
        public SimpleTreeView(TreeViewState<int> state, TreeModel<TreeElement> model) : base(state, model)
        {

        }
    }
}