using AnodyneSharp.Dialogue;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Resources.Loading
{
    public class DialogueLoader : ContentLoader
    {
        public DialogueLoader(string filePath) 
        : base(filePath)
        {
        }

        private Dictionary<string, Dictionary<string, Dictionary<string , Scene>>> _sceneTree;

        public Dictionary<string, Dictionary<string, Dictionary<string, Scene>>>
    }
}
