using AnodyneSharp.Entities;
using AnodyneSharp.Input;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
        ZHS
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

        public static Language CurrentLanguage { get; private set; }

        private static Dictionary<string, DialogueNPC> _sceneTree;

        public static void LoadDialogue(Language lang)
        {
            CurrentLanguage = lang;

            ReadFile();
        }

        public static string GetDialogue(string npc, string area, string scene, int id)
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

            return ReplaceKeys(s.GetDialogue(id));
        }

        private static string ReplaceKeys(string line)
        {
            //TODO controller mode check
            //if ()
            //{
            //
            //}
            //else
            //{

            Keys t = 0;

            line = line
                .Replace("[SOMEKEY-X]", Enum.GetName(t.GetType(), KeyInput.RebindableKeys[KeyFunctions.Cancel].Keys.First()))
                .Replace("[SOMEKEY-C]", Enum.GetName(t.GetType(), KeyInput.RebindableKeys[KeyFunctions.Accept].Keys.First()))
                .Replace("[SOMEKEY-LEFT]", Enum.GetName(t.GetType(), KeyInput.RebindableKeys[KeyFunctions.Left].Keys.First()))
                .Replace("[SOMEKEY-UP]", Enum.GetName(t.GetType(), KeyInput.RebindableKeys[KeyFunctions.Up].Keys.First()))
                .Replace("[SOMEKEY-RIGHT]", Enum.GetName(t.GetType(), KeyInput.RebindableKeys[KeyFunctions.Right].Keys.First()))
                .Replace("[SOMEKEY-DOWN]", Enum.GetName(t.GetType(), KeyInput.RebindableKeys[KeyFunctions.Down].Keys.First()))
                .Replace("[SOMEKEY-ENTER]", Enum.GetName(t.GetType(), KeyInput.RebindableKeys[KeyFunctions.Pause].Keys.First()))
                ;
            //}

            return line;
        }

        private static void ReadFile()
        {
            var assembly = Assembly.GetExecutingAssembly();

            string path = $"{assembly.GetName().Name}.{DialogueFilePath}_{Enum.GetName(CurrentLanguage.GetType(), CurrentLanguage)}.txt";

            ParseState state = ParseState.START;

            _sceneTree = new Dictionary<string, DialogueNPC>();

            DialogueNPC npc = null;
            string areaName = "";
            string scene = "";

            List<string> dialogue = new List<string>();
            int? loopID = null;
            bool alignTop = false;

            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
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
                                    loopID = dialogue.Count - 1;
                                    continue;
                                }
                                dialogue.Add(line);
                                break;
                        }

                    }
                }
            }
        }

        private static string GetName(string line)
        {
            return line.Split(' ')[1];
        }
    }
}
