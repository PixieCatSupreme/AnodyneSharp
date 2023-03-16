using System.IO;
using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using System.Linq;
using static Android.Provider.ContactsContract;
using AnodyneSharp.Resources;
using System.Collections.Generic;

namespace AnodyneSharp.Android
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.FullUser,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class Activity1 : AndroidGameActivity
    {
        private AnodyneGame _game;
        private View _view;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _game = new AnodyneGame();
            _view = _game.Services.GetService(typeof(View)) as View;

            SetContentView(_view);

            ResourceManager.GetDirectories = GetDirectories;
            ResourceManager.GetFiles = GetFiles;

            _game.Run();
        }

        public DirectoryInfo[] GetDirectories(string fullPath)
        {
            if (fullPath.StartsWith('/'))
            {
                fullPath = fullPath[1..];
            }

            string[] directoryList = Assets.List(fullPath);

            return directoryList.Where(d => !d.Contains('.')).Select(d => new DirectoryInfo(Path.Combine(fullPath, d))).ToArray();
        }

        public  List<FileInfo> GetFiles(string fullPath)
        {
            if (fullPath.StartsWith('/'))
            {
                fullPath = fullPath[1..];
            }

            string[] directoryList = Assets.List(fullPath);

            return directoryList.Where(d => d.Contains('.')).Select(d => new FileInfo(Path.Combine(fullPath, d))).ToList();
        }
    }
}
