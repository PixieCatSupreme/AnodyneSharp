﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Utilities
{
    public static class CoroutineUtils
    {
        public static IEnumerator OnceEvery(IEnumerator adapted, float tm)
        {
            while(adapted.MoveNext())
            {
                yield return adapted.Current;
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, tm, 1)) yield return null;
            }
            yield break;
        }

        public static IEnumerator OnceEvery(Action func, float tm)
        {
            while(true)
            {
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, tm, 1)) yield return null;
                func();
            }
        }

        public static IEnumerator ForEach<T>(IEnumerable<T> values, Action<T> action)
        {
            foreach(T item in values)
            {
                action(item);
                yield return null;
            }
        }

        public static IEnumerator<T> WaitFor<T>(Func<bool> pred)
        {
            while (!pred()) yield return default(T);
            yield break;
        }
    }
}
