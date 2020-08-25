using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Resources
{
    public static class ResourceManager
    {
        private static Dictionary<string, Texture2D> _textures;
        private static Dictionary<string, Song> _music;
        private static Dictionary<string, SoundEffect> _sfx;


        static ResourceManager()
        {
            _textures = new Dictionary<string, Texture2D>();
            _music = new Dictionary<string, Song>();
            _sfx = new Dictionary<string, SoundEffect>();
        }

        public static bool LoadResources(ContentManager content)
        {
            DirectoryInfo dir = new DirectoryInfo(content.RootDirectory);
            if (!dir.Exists)
            {
                return false;
            }

            DirectoryInfo[] directories = dir.GetDirectories();

            LoadTextures(content, directories.First(d => d.Name == "textures"));
            LoadMusic(content, directories.First(d => d.Name == "mp3"));
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

        public static Song GetMusic(string musicName)
        {
            if (!_music.ContainsKey(musicName))
            {
                DebugLogger.AddWarning($"Music file called {musicName}.mp3 not found!");
                return null;
            }

            return _music[musicName];
        }

        public static SoundEffect GetSFX(string sfxName)
        {
            if (!_sfx.ContainsKey(sfxName))
            {
                DebugLogger.AddWarning($"SFX file called {sfxName}.mp3 not found!");
                return null;
            }

            return _sfx[sfxName];
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

                _music[key] = content.Load<Song>(GetFolderTree(file) + key);
            }
        }

        private static void LoadSFX(ContentManager content, DirectoryInfo directory)
        {
            List<FileInfo> files = GetChildFiles(directory);

            foreach (FileInfo file in files)
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);

                _sfx[key] = content.Load<SoundEffect>(GetFolderTree(file) + key);
            }
        }

        private static List<FileInfo> GetChildFiles(DirectoryInfo directory)
        {
            if (directory.Name.ToLower() == "old")
            {
                return new List<FileInfo>();
            }

            List<FileInfo> files = directory.GetFiles().ToList();

            foreach (var child in directory.GetDirectories())
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
