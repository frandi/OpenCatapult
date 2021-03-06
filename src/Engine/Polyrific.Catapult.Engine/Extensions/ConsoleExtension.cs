﻿// Copyright (c) Polyrific, Inc 2018. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace Polyrific.Catapult.Engine.Extensions
{
    /// <summary>
    /// Helper methods which are adapted from <see cref="McMaster.Extensions.CommandLineUtils.Prompt"/>
    /// to make the the methods directly extend <see cref="McMaster.Extensions.CommandLineUtils.IConsole"/>.
    /// It is done to prevent hard dependency to <see cref="System.Console"/>.
    /// </summary>
    public static class ConsoleExtension
    {
        private const char Backspace = '\b';

        /// <summary>
        /// Gets a yes/no response from the console after displaying a <paramref name="prompt" />.
        /// <para>
        /// The parsing is case insensitive. Valid responses include: yes, no, y, n.
        /// </para>
        /// </summary>
        /// <param name="console">The console</param>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="defaultAnswer">If the user provides an empty response, which value should be returned</param>
        /// <param name="promptColor">The console color to display</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>True is 'yes'</returns>
        public static bool GetYesNo(this IConsole console, string prompt, bool defaultAnswer, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            var answerHint = defaultAnswer ? "[Y/n]" : "[y/N]";
            do
            {
                Write($"{prompt} {answerHint}", promptColor, promptBgColor);
                console.Write(' ');

                string resp;
                using (ShowCursor())
                {
                    resp = console.In.ReadLine()?.ToLower()?.Trim();
                }

                if (string.IsNullOrEmpty(resp))
                {
                    return defaultAnswer;
                }

                if (resp == "n" || resp == "no")
                {
                    return false;
                }

                if (resp == "y" || resp == "yes")
                {
                    return true;
                }

                console.WriteLine($"Invalid response '{resp}'. Please answer 'y' or 'n' or CTRL+C to exit.");
            }
            while (true);
        }

        /// <summary>
        /// Gets a console response from the console after displaying a <paramref name="prompt" />.
        /// </summary>
        /// <param name="console">The console</param>
        /// <param name="prompt">The question to display on command line</param>
        /// <param name="defaultValue">If the user enters a blank response, return this value instead.</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>The response the user gave. Can be null or empty</returns>
        public static string GetString(this IConsole console, string prompt, string defaultValue = null, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            if (defaultValue != null)
            {
                prompt = $"{prompt} [{defaultValue}]";
            }

            Write(prompt, promptColor, promptBgColor);
            console.Write(' ');

            string resp;
            using (ShowCursor())
            {
                resp = console.In.ReadLine();
            }

            if (!string.IsNullOrEmpty(resp))
            {
                return resp;
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets a response that contains a password. Input is masked with an asterisk.
        /// </summary>
        /// <param name="console">The console</param>
        /// <param name="prompt">The question to display on command line</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>The password as plaintext. Can be null or empty.</returns>
        public static string GetPassword(this IConsole console, string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            var resp = new StringBuilder();

            foreach (var key in ReadObfuscatedLine(console, prompt, promptColor, promptBgColor))
            {
                switch (key)
                {
                    case Backspace:
                        resp.Remove(resp.Length - 1, 1);
                        break;
                    default:
                        resp.Append(key);
                        break;
                }
            }

            return resp.ToString();
        }

        /// <summary>
        /// Gets an integer response from the console after displaying a <paramref name="prompt" />.
        /// </summary>
        /// <param name="console">The console</param>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="defaultAnswer">If the user provides an empty response, which value should be returned</param>
        /// <param name="promptColor">The console color to display</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>The response as a number</returns>
        public static int GetInt(this IConsole console, string prompt, int? defaultAnswer = null, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            do
            {
                Write(prompt, promptColor, promptBgColor);
                if (defaultAnswer.HasValue)
                {
                    Write($" [{defaultAnswer.Value}]", promptColor, promptBgColor);
                }
                console.Write(' ');

                string resp;
                using (ShowCursor())
                {
                    resp = console.In.ReadLine()?.ToLower()?.Trim();
                }

                if (string.IsNullOrEmpty(resp))
                {
                    if (defaultAnswer.HasValue)
                    {
                        return defaultAnswer.Value;
                    }
                    else
                    {
                        console.WriteLine("Please enter a valid number or press CTRL+C to exit.");
                        continue;
                    }
                }

                if (int.TryParse(resp, out var result))
                {
                    return result;
                }

                console.WriteLine($"Invalid number '{resp}'. Please enter a valid number or press CTRL+C to exit.");
            }
            while (true);
        }

        /// <summary>
        /// Base implementation of GetPassword and GetPasswordAsString. Prompts the user for
        /// a password and yields each key as the user inputs. Password is masked as input. Pressing Escape will reset the password
        /// by flushing the stream with backspace keys.
        /// </summary>
        /// <param name="console">The console</param>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>A stream of characters as input by the user including Backspace for deletions.</returns>
        private static IEnumerable<char> ReadObfuscatedLine(IConsole console, string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            const string whiteOut = "\b \b";
            Write(prompt, promptColor, promptBgColor);
            console.Write(' ');
            const ConsoleModifiers IgnoredModifiersMask = ConsoleModifiers.Alt | ConsoleModifiers.Control;
            var readChars = 0;
            ConsoleKeyInfo key;
            do
            {
                using (ShowCursor())
                {
                    // TODO: Find a way to replace this "Console" with "console"
                    key = Console.ReadKey(intercept: true);
                }

                if ((key.Modifiers & IgnoredModifiersMask) != 0)
                {
                    continue;
                }

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        console.WriteLine();
                        break;
                    case ConsoleKey.Backspace:
                        if (readChars > 0)
                        {
                            console.Write(whiteOut);
                            --readChars;
                            yield return Backspace;
                        }
                        break;
                    case ConsoleKey.Escape:
                        // Reset the password
                        while (readChars > 0)
                        {
                            console.Write(whiteOut);
                            yield return Backspace;
                            --readChars;
                        }
                        break;
                    default:
                        readChars += 1;
                        console.Write('*');
                        yield return key.KeyChar;
                        break;
                }
            }
            while (key.Key != ConsoleKey.Enter);
        }

        private static void Write(string value, ConsoleColor? foreground, ConsoleColor? background)
        {
            if (foreground.HasValue)
            {
                Console.ForegroundColor = foreground.Value;
            }

            if (background.HasValue)
            {
                Console.BackgroundColor = background.Value;
            }

            Console.Write(value);

            if (foreground.HasValue || background.HasValue)
            {
                Console.ResetColor();
            }
        }

        private static IDisposable ShowCursor() => new CursorState();

        private class CursorState : IDisposable
        {
            private readonly bool _original;

            public CursorState()
            {
                try
                {
                    _original = Console.CursorVisible;
                }
                catch
                {
                    // some platforms throw System.PlatformNotSupportedException
                    // Assume the cursor should be shown
                    _original = true;
                }

                TrySetVisible(true);
            }

            private void TrySetVisible(bool visible)
            {
                try
                {
                    Console.CursorVisible = visible;
                }
                catch
                {
                    // setting cursor may fail if output is piped or permission is denied.
                }
            }

            public void Dispose()
            {
                TrySetVisible(_original);
            }
        }
    }
}