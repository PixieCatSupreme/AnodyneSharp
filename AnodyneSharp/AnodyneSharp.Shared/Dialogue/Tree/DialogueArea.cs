using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AnodyneSharp.Dialogue
{
    public class DialogueArea
    {
        [JsonInclude]
        public Dictionary<string, DialogueScene> _scenes;

        public DialogueArea()
        {
            _scenes = new Dictionary<string, DialogueScene>();
        }

        internal void AddScene(string scene, DialogueScene dialogueScene)
        {
            _scenes.Add(scene, dialogueScene);
        }

        internal DialogueScene GetScene(string scene)
        {
            if (!_scenes.ContainsKey(scene))
            {
                return null;
            }

            return _scenes[scene];
        }
    }
}
