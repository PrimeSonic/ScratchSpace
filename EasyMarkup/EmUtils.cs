﻿namespace EasyMarkup
{
    using System;
    using Core;

    internal static class EmUtils
    {
        public static bool Deserialize<T>(this T emProperty, string serializedData)
            where T : EmBase
        {
            try
            {
                return emProperty.FromString(serializedData);
            }
            catch (EmException emEx)
            {
                //QuickLogger.Error($"[EasyMarkup] Deserialize halted unexpectedly for {emProperty.Key}{Environment.NewLine}{emEx.ToString()}");
                return false;
            }
            catch (Exception ex)
            {
                //QuickLogger.Error($"[EasyMarkup] Deserialize halted unexpectedly for {emProperty.Key}{Environment.NewLine}Error reported: {ex}");
                return false;
            }
        }

        public static string Serialize<T>(this T emProperty, bool prettyPrint = true)
            where T : EmBase
        {
            if (prettyPrint)
            {
                return emProperty.PrettyPrint();
            }

            return emProperty.ToString();
        }

        public static bool DeserializeKeyOnly<T>(this T emProperty, string serializedData, out string foundKey)
            where T : EmBase
        {
            try
            {
                return EmBase.CheckKey(serializedData, out foundKey, emProperty.Key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EasyMarkup] DeserializeKeyOnly halted unexpectedly for {emProperty.Key}{Environment.NewLine}" + $"Error reported: {ex}");
                foundKey = null;
                return false;
            }
        }

        public static string CommentText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return $"{EmBase.SpChar_CommentBlock} {text} {EmBase.SpChar_CommentBlock}";
        }

        public static string[] CommentTextLines(string[] textArray)
        {
            var commentedArray = new string[textArray.Length];

            for (int i = 0; i < textArray.Length; i++)
            {
                commentedArray[i] = CommentText(textArray[i]);
            }

            return commentedArray;
        }

        public static string[] CommentTextLinesCentered(string[] textArray)
        {
            var commentedArray = new string[textArray.Length];

            int maxSize = 0;
            for (int i = 0; i < textArray.Length; i++)
            {
                maxSize = Math.Max(maxSize, textArray[i].Length);
            }

            if (maxSize % 2 != 0)
            {
                maxSize++;
            }

            for (int i = 0; i < textArray.Length; i++)
            {
                int totalPadding = maxSize - textArray[i].Length;
                int leftPad = 0;
                int rightPad = 0;

                if (totalPadding % 2 == 0)
                {
                    rightPad = leftPad = totalPadding / 2;
                }
                else
                {
                    leftPad = (totalPadding - 1) / 2;
                    rightPad = (totalPadding - 1) / 2 + 1;
                }

                string paddedText = $"{new string(' ', leftPad)}{textArray[i]}{new string(' ', rightPad)}";

                commentedArray[i] = CommentText(paddedText);
            }

            return commentedArray;
        }
    }
}