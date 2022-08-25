using System;
using System.Collections.Generic;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Utilities
{
    public static class CoroutineUtils
    {
        public static IEnumerator<string> OnceEvery(IEnumerator<string> adapted, float tm)
        {
            while(adapted.MoveNext())
            {
                yield return adapted.Current;
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, tm, 1)) yield return null;
            }
            yield break;
        }

        public static IEnumerator<T> WaitFor<T>(Func<bool> pred)
        {
            while (!pred()) yield return default(T);
            yield break;
        }
    }
}
