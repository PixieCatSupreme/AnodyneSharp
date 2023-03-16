using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnodyneSharp.Resources
{
    public delegate DirectoryInfo[] GetDirectories(string path);
    public delegate List<FileInfo> GetFiles(string path);
    public static class ResourceManager
    {
        public static GetDirectories GetDirectories;
        public static GetFiles GetFiles;

        private static Dictionary<string, Texture2D> _textures = new();
        private static Dictionary<string, string> _music = new();
        private static Dictionary<string, string> _ambience = new();
        private static Dictionary<string, SFXLimiter> _sfx = new();

        public static bool LoadResources(ContentManager content)
        {

            DirectoryInfo[] directories = GetDirectories?.Invoke("Content");

            LoadTextures(content, directories.First(d => d.Name == "textures"));
            //LoadMusic(content, directories.First(d => d.Name == "bgm"));
            LoadAmbience(content, directories.First(d => d.Name == "ambience"));
            LoadSFX(content, directories.First(d => d.Name == "sfx"));

            return true;
        }

        public static Texture2D GetTexture(string textureName, bool forceCorrectTexture = false, bool allowUnknown = false)
        {
            if (!forceCorrectTexture && GlobalState.GameMode != GameMode.Normal)
            {
                return _textures.Values.ElementAt(GlobalState.RNG.Next(_textures.Count));
            }

            if (!_textures.ContainsKey(textureName))
            {
                if (!allowUnknown)
                {
                    DebugLogger.AddWarning($"Texture file called {textureName}.png not found!");
                }
                return null;
            }

            return _textures[textureName];
        }

        public static string GetMusicPath(string musicName)
        {
            if (!_music.ContainsKey(musicName))
            {
                DebugLogger.AddWarning($"Music file called {musicName}.ogg not found!");
                return null;
            }

            return _music[musicName];
        }

        public static string GetAmbiencePath(string ambienceName)
        {
            if (!_ambience.ContainsKey(ambienceName))
            {
                return null;
            }
            return _ambience[ambienceName];
        }

        public static SoundEffectInstance GetSFX(string sfxName)
        {
            if (!_sfx.ContainsKey(sfxName))
            {
                DebugLogger.AddWarning($"SFX file called {sfxName}.mp3 not found!");
                return null;
            }

            return _sfx[sfxName].Get();
        }

        public static Dictionary<string, SFXLimiter> GetSFX()
        {

            return _sfx;
        }

        private static void LoadTextures(ContentManager content, DirectoryInfo directory)
        {
            List<FileInfo> files = GetChildFiles(directory);

            foreach (FileInfo file in files)
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);

                _textures[key] = content.Load<Texture2D>(GetFolderTree(file) + key);
            }
        }

        private static void LoadMusic(ContentManager content, DirectoryInfo directory)
        {
            List<FileInfo> files = GetChildFiles(directory);

            foreach (FileInfo file in files)
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);

                _music[key] = file.FullName;
            }
        }

        private static void LoadAmbience(ContentManager content, DirectoryInfo directory)
        {
            List<FileInfo> files = GetChildFiles(directory);

            foreach (FileInfo file in files)
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);

                _ambience[key] = file.FullName;
            }
        }

        private static void LoadSFX(ContentManager content, DirectoryInfo directory)
        {
            List<FileInfo> files = GetChildFiles(directory);

            foreach (FileInfo file in files)
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);

                _ = int.TryParse(file.Directory.Name, out int limit);

                _sfx[key] = new(content.Load<SoundEffect>(GetFolderTree(file) + key), limit);
            }
        }

        private static List<FileInfo> GetChildFiles(DirectoryInfo directory)
        {
            if (directory.Name.ToLower() == "old")
            {
                return new List<FileInfo>();
            }

            List<FileInfo> files = GetFiles(directory.FullName);

            foreach (var child in GetDirectories(directory.FullName))
            {
                files.AddRange(GetChildFiles(child));
            }

            return files;
        }

        private static string GetFolderTree(FileInfo file)
        {
            string path = "";
            DirectoryInfo curFolder = file.Directory;

            do
            {
                path = curFolder.Name + "/" + path;
                curFolder = curFolder.Parent;
            } while (curFolder.Name != "Content");

            return path;
        }
    }
}
