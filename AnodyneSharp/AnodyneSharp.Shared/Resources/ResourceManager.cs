using AnodyneSharp.Drawing;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Extensions.FileSystemGlobbing;
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
        public static string BaseDir;

        private static Dictionary<string, Texture2D> _textures = new();
        private static Dictionary<string, string> _music = new();
        private static Dictionary<string, string> _ambience = new();
        private static Dictionary<string, SFXLimiter> _sfx = new();

        public static bool LoadResources(ContentManager content)
        {
            LoadTextures(content);
#if !ANDROID
            LoadMusic(content);
#endif
            LoadAmbience(content);
            LoadSFX(content);

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

        private static void LoadTextures(ContentManager content)
        {
            foreach (FileInfo file in GetFiles("textures"))
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);

                try
                {
                    _textures[key] = content.Load<Texture2D>(GetFolderTree(file) + key);
                }
                catch (Exception)
                {
                    _textures[key] = Texture2D.FromFile(SpriteDrawer._spriteBatch.GraphicsDevice, file.FullName);
                }
            }
        }

        private static void LoadMusic(ContentManager content)
        {
            foreach (FileInfo file in GetFiles("bgm"))
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);

                _music[key] = file.FullName;
            }
        }

        private static void LoadAmbience(ContentManager content)
        {
            foreach (FileInfo file in GetFiles("ambience"))
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);

                _ambience[key] = file.FullName;
            }
        }

        private static void LoadSFX(ContentManager content)
        {
            foreach (FileInfo file in GetFiles("sfx"))
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);

                _ = int.TryParse(file.Directory.Name, out int limit);

                try
                {
                    _sfx[key] = new(content.Load<SoundEffect>(GetFolderTree(file) + key), limit);
                }
                catch (Exception)
                {
                    _sfx[key] = new(SoundEffect.FromFile(file.FullName), limit);
                }
            }
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

        private static IEnumerable<FileInfo> GetFiles(string dirName)
        {
            Matcher matcher = new();
            matcher.AddInclude($"./Content/{dirName}/**");
            matcher.AddExclude("./Content/**/old/");
            matcher.AddInclude($"./Mods/*/Content/{dirName}/**");

            return matcher.GetResultsInFullPath(BaseDir).Select(s => new FileInfo(s));
        }
    }
}
