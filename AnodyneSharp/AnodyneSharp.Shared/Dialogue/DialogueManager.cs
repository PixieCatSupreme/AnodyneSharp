using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using static AnodyneSharp.Utilities.TextUtilities;

namespace AnodyneSharp.Dialogue
{
    public enum Language
    {
        EN,
        ES,
        IT,
        JP,
        KR,
        PT_BR,
        ZH_CN
    }
    public static class DialogueManager
    {
        private enum ParseState
        {
            START,
            NPC,
            AREA,
            SCENE
        }

        private const string DialogueFilePath = "Content.Dialogue.dialogue";

        private static Dictionary<string, DialogueNPC> _sceneTree;

        private static int _lastPlayedChunk;

        public static void LoadDialogue(Language lang)
        {
            GlobalState.CurrentLanguage = lang;

            ReadFile();
        }

        public static string GetDialogue(string npc, string scene, int id = -1)
        {
            return GetDialogue(npc, GlobalState.CURRENT_MAP_NAME, scene, id);
        }

        public static string GetDialogue(string npc, string area, string scene, int id = -1)
        {
            if (!_sceneTree.ContainsKey(npc))
            {
                return $"Missing npc {npc}";
            }

            DialogueNPC dn = _sceneTree[npc];

            DialogueArea a = dn.GetArea(area);

            if (a == null)
            {
                return $"Missing area {area}";
            }

            DialogueScene s = a.GetScene(scene);

            if (s == null)
            {
                return $"Missing scene {scene}";
            }

            GlobalState.DialogueTop = s.AlignTop;

            return ReplaceKeys(s.GetDialogue(id));
        }

        private static string ReplaceKeys(string line)
        {
            if (KeyInput.ControllerMode)
            {
                line = line
                    .Replace("[SOMEKEY-X]", GetButtonString(KeyInput.RebindableKeys[KeyFunctions.Cancel].Buttons.First()))
                    .Replace("[SOMEKEY-C]", GetButtonString(KeyInput.RebindableKeys[KeyFunctions.Accept].Buttons.First()))
                    .Replace("[SOMEKEY-LEFT]", GetButtonString(KeyInput.RebindableKeys[KeyFunctions.Left].Buttons.First()))
                    .Replace("[SOMEKEY-UP]", GetButtonString(KeyInput.RebindableKeys[KeyFunctions.Up].Buttons.First()))
                    .Replace("[SOMEKEY-RIGHT]", GetButtonString(KeyInput.RebindableKeys[KeyFunctions.Right].Buttons.First()))
                    .Replace("[SOMEKEY-DOWN]", GetButtonString(KeyInput.RebindableKeys[KeyFunctions.Down].Buttons.First()))
                    .Replace("[SOMEKEY-ENTER]", GetButtonString(KeyInput.RebindableKeys[KeyFunctions.Pause].Buttons.First()))
                    ;
            }
            else
            {
                line = line
                    .Replace("[SOMEKEY-X]", GetKeyBoardString(KeyInput.RebindableKeys[KeyFunctions.Cancel].Keys.First()))
                    .Replace("[SOMEKEY-C]", GetKeyBoardString(KeyInput.RebindableKeys[KeyFunctions.Accept].Keys.First()))
                    .Replace("[SOMEKEY-LEFT]", GetKeyBoardString(KeyInput.RebindableKeys[KeyFunctions.Left].Keys.First()))
                    .Replace("[SOMEKEY-UP]", GetKeyBoardString(KeyInput.RebindableKeys[KeyFunctions.Up].Keys.First()))
                    .Replace("[SOMEKEY-RIGHT]", GetKeyBoardString(KeyInput.RebindableKeys[KeyFunctions.Right].Keys.First()))
                    .Replace("[SOMEKEY-DOWN]", GetKeyBoardString(KeyInput.RebindableKeys[KeyFunctions.Down].Keys.First()))
                    .Replace("[SOMEKEY-ENTER]", GetKeyBoardString(KeyInput.RebindableKeys[KeyFunctions.Pause].Keys.First()))
                    ;
            }

            return line;
        }



        private static void ReadFile()
        {
            var assembly = Assembly.GetExecutingAssembly();

            string path = $"{assembly.GetName().Name}.{DialogueFilePath}_{Enum.GetName(GlobalState.CurrentLanguage.GetType(), GlobalState.CurrentLanguage)}.txt";

            ParseState state = ParseState.START;

            _sceneTree = new Dictionary<string, DialogueNPC>();

            DialogueNPC npc = null;
            string areaName = "";
            string scene = "";

            List<string> dialogue = new List<string>();
            int? loopID = null;
            bool alignTop = false;

            using Stream stream = assembly.GetManifestResourceStream(path);
            using StreamReader reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().Trim();

                if (line.StartsWith("#"))
                {
                    continue;
                }

                switch (state)
                {
                    case ParseState.START:
                        if (line.StartsWith("npc"))
                        {
                            npc = new DialogueNPC();
                            state = ParseState.NPC;
                            _sceneTree.Add(GetName(line), npc);
                        }
                        break;
                    case ParseState.NPC:
                        if (line == "does reset")
                        {
                            npc.doesReset = true;
                            continue;
                        }
                        if (line == "end npc")
                        {
                            state = ParseState.START;
                            continue;
                        }
                        if (line.StartsWith("area"))
                        {
                            areaName = GetName(line);
                            state = ParseState.AREA;
                            npc.AddArea(areaName);
                        }
                        break;
                    case ParseState.AREA:
                        if (line == "end area")
                        {
                            state = ParseState.NPC;
                            continue;
                        }
                        if (line.StartsWith("scene"))
                        {
                            scene = GetName(line);
                            state = ParseState.SCENE;

                            loopID = null;
                            dialogue = new List<string>();
                            alignTop = false;
                        }
                        break;
                    case ParseState.SCENE:
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        if (line == "TOP")
                        {
                            alignTop = true;
                            continue;
                        }
                        if (line == "end scene")
                        {
                            state = ParseState.AREA;
                            npc.GetArea(areaName).AddScene(scene, new DialogueScene(alignTop, loopID, dialogue));
                            continue;
                        }
                        if (line == "LOOP")
                        {
                            loopID = dialogue.Count;
                            continue;
                        }
                        dialogue.Add(line);
                        break;
                }

            }
        }

        private static string GetName(string line)
        {
            return line.Split(' ')[1];
        }
    }
}
