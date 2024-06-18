using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace backup_manager
{
    public static class Extensions
    {
        public static Task<bool> WaitOneAsync(this WaitHandle waitHandle, int timeoutMs)
        {
            if (waitHandle == null)
                throw new ArgumentNullException(nameof(waitHandle));

            var tcs = new TaskCompletionSource<bool>();

            RegisteredWaitHandle registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                callBack: (state, timedOut) => { tcs.TrySetResult(!timedOut); },
                state: null,
                millisecondsTimeOutInterval: timeoutMs,
                executeOnlyOnce: true);

            return tcs.Task.ContinueWith((antecedent) =>
            {
                registeredWaitHandle.Unregister(waitObject: null);
                try
                {
                    return antecedent.Result;
                }
                catch
                {
                    return false;
                }
            });
        }
        public static string GetDisplayAttributeFrom(this Enum enumValue, Type enumType)
        {
            string displayName = "";
            MemberInfo info = enumType.GetMember(enumValue.ToString()).First();

            if (info != null && info.CustomAttributes.Any())
            {
                DisplayAttribute nameAttr = info.GetCustomAttribute<DisplayAttribute>();
                displayName = nameAttr != null ? nameAttr.Name : enumValue.ToString();
            }
            else
            {
                displayName = enumValue.ToString();
            }
            return displayName;
        }
        internal static string GetCleanFileName(this string rawFileName)
        {
            var fileName = string.Join("", rawFileName.Split(Path.GetInvalidFileNameChars())).Replace(" ", "");

            if(fileName.Length > 63)
                throw new Exception("File name to long exception. (fn must be less than 63 characters");

            return fileName;
        }
        public static string StringBeforeLastRegEx(this string str, Regex regex)
        {
            var matches = regex.Matches(str);

            return matches.Count > 0
                ? str.Substring(0, matches.Last().Index)
                : str;

        }

        public static bool EndsWithRegEx(this string str, Regex regex)
        {
            var matches = regex.Matches(str);

            return
                matches.Count > 0 &&
                str.Length == (matches.Last().Index + matches.Last().Length);
        }

        public static string StringAfter(this string str, string substring)
        {
            var index = str.IndexOf(substring, StringComparison.Ordinal);

            return index >= 0
                ? str.Substring(index + substring.Length)
                : "";
        }

        public static string[] GetLines(this string str) =>
            Regex.Split(str, "\r\n|\r|\n");
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> func)
        {
            foreach (var item in sequence)
            {
                func(item);
            }
        }
    }
}