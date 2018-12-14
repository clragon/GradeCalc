using System;
using System.Collections.Generic;
using System.Linq;

namespace Cloe
{
    // Console List Options Enumerator

    public class ListMenu<T>
    {
        /// <summary>
        /// The Function to update the list of entries.
        /// The current list of entries is passed and a list of entries is expected to be returned.
        /// By default, no update happens.
        /// </summary>
        public Func<List<T>, List<T>> UpdateEntries = DefaultUpdateEntries;

        /// <summary>
        /// The static default UpdateEntries function
        /// Setting it will provide a new default.
        /// </summary>
        public static Func<List<T>, List<T>> DefaultUpdateEntries = (List<T> entries) => { return entries; };

        /// <summary>
        /// The function to display the title on top of the list.
        /// The current list of entries is passed.
        /// By default, this displays nothing.
        /// </summary>
        public Action<List<T>> DisplayTitle = DefaultDisplayTitle;

        /// <summary>
        /// The static default DisplayTitle function
        /// Setting it will provide a new default.
        /// </summary>
        public static Action<List<T>> DefaultDisplayTitle = (List<T> entries) => { };

        /// <summary>
        /// The function to display an entry.
        /// The current list of entries, the current entry to be displayed, it's index in the list of entries and it's number as option are passed.
        /// By default, this displays something like "[<number>] <entry>".
        /// </summary>
        public Action<List<T>, T, int, int> DisplayEntry = DefaultDisplayEntry;

        /// <summary>
        /// The static default DisplayEntry function
        /// Setting it will provide a new default.
        /// </summary>
        public static Action<List<T>, T, int, int> DefaultDisplayEntry = (List<T> entries, T entry, int index, int num) => { Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(entries.Count).Length, ' '), entry); };

        /// <summary>
        /// The function that is called when the user enters 0;
        /// It's different from the rest of the list and can be customized to for example allowing creating new entries.
        /// The function is expected to return a bool that indicates if the menu should be exited after executing the method or not.
        /// If you want to use it, it could look like this;
        /// Console.WriteLine("[{0}] {1}", Convert.ToString(0).PadLeft(Convert.ToString(entries.Count).Length, ' '), "Create new entry");
        /// </summary>
        public Func<List<T>, bool> ZeroMethod = DefaultZeroMethod;

        /// <summary>
        /// The static default ZeroMethod. By default, it shows the error message for invalid input and returns to the menu.
        /// </summary>
        public static Func<List<T>, bool> DefaultZeroMethod = (List<T> entries) => { ResetInput(); return false; };

        /// <summary>
        /// The string that is shown on prompting user input.
        /// </summary>
        public string Prompt = DefaultPrompt;

        /// <summary>
        /// The static default string that is shown on prompting user input.
        /// </summary>
        public static string DefaultPrompt = "Choose";

        /// <summary>
        /// Defines if the user can exit the menu without choosing an entry.
        /// </summary>
        public bool UserCanExit = DefaultUserCanExit;

        /// <summary>
        /// The static default bool to define if the user can exit the menu without choosing an entry.
        /// </summary>
        public static bool DefaultUserCanExit = true;

        /// <summary>
        /// The character the user has to input to exit the menu.
        /// </summary>
        public char ExitKey = DefaultExitKey;

        /// <summary>
        /// The static default character the user has to input to exit the menu.
        /// </summary>
        public static char DefaultExitKey = 'q';

        /// <summary>
        /// The string that is displayed for the exit entry.
        /// </summary>
        public string ExitEntry = DefaultExitEntry;

        /// <summary>
        /// The static default string that is displayed for the exit entry.
        /// </summary>
        public static string DefaultExitEntry = "Back";

        /// <summary>
        /// The static default string that is displayed when the user inputs any character that isnt a valid option.
        /// </summary>
        public static string DefaultInputError = "Invalid input";

        /// <summary>
        /// The static default string displayed in front of the default input error message.
        /// </summary>
        public static string DefaultErrorName = "Error";

        /// <summary>
        /// The static default bool that defines if the console should be cleared when loading a menu.
        /// </summary>
        public static bool ClearOnSwitch = true;

        /// <summary>
        /// The list of objects you want to display.
        /// <para/>They will be displayed as enumrated options for the user to choose from.</param>
        /// </summary>
        public List<T> Entries;

        /// <summary>
        /// The function that handles the chosen entry.
        /// <para/>The list of entries and the index of the chosen entry will be passed. A bool indicating whether or not the menu should exit, after executing the function, is expected to be returned.</param>
        /// </summary>
        public Func<List<T>, int, bool> HandleEntry;

        /// <summary>
        /// The ListMenu class
        /// </summary>
        /// <param name="entries">The list of objects you want to display.<para/>They will be displayed as enumrated options for the user to choose from.</param>
        /// <param name="handleEntry">The function that handles the chosen entry. <para/>The list of entries and the index of the chosen entry will be passed. A bool indicating whether or not the menu should exit, after executing the function, is expected to be returned.</param>
        public ListMenu(List<T> entries, Func<List<T>, int, bool> handleEntry)
        {
            Entries = entries;
            HandleEntry = handleEntry;
        }

        /// <summary>
        /// Display the menu. This should be called after setting all attributes of the class correctly.
        /// </summary>
        public void Show()
        {
            string readInput = string.Empty;
            bool MenuExitIsPending = false;
            while (!MenuExitIsPending)
            {
                ClearMenu();
                int printedEntries = 0;
                Entries = UpdateEntries(Entries);
                DisplayTitle(Entries);
                if (Entries.Any())
                {
                    int num = 0;
                    foreach (T entry in Entries)
                    {
                        num++;
                        if (string.IsNullOrEmpty(readInput) || Convert.ToString(num).StartsWith(readInput))
                        {
                            DisplayEntry(Entries, entry, Entries.IndexOf(entry), num);
                            printedEntries++;
                        }

                        if (Entries.Count > Console.WindowHeight - 5)
                        {
                            if (printedEntries >= Console.WindowHeight - (5 + 1))
                            {
                                Console.WriteLine("[{0}] +{1}", ".".PadLeft(Convert.ToString(Entries.Count).Length, '.'), Entries.Count);
                                break;
                            }
                        }
                        else
                        {
                            if (printedEntries == Console.WindowHeight - 5)
                            {
                                break;
                            }
                        }

                    }
                }

                if (UserCanExit)
                {
                    Console.WriteLine("[{0}] {1}", Convert.ToString(ExitKey).PadLeft(Convert.ToString(Entries.Count).Length, ' '), ExitEntry);
                }

                Console.WriteLine();

                bool InputIsValid = false;
                while (!InputIsValid)
                {
                    Console.Write("{0}> {1}", Prompt, readInput);
                    ConsoleKeyInfo input = Console.ReadKey();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    int choiceNum = -1;
                    switch (input)
                    {
                        case var key when key.KeyChar.Equals(ExitKey):
                            if (UserCanExit)
                            {
                                Console.WriteLine();
                                InputIsValid = true;
                                MenuExitIsPending = true;
                            }
                            else
                            {
                                Console.WriteLine();
                                ResetInput();
                            }
                            break;

                        case var key when key.Key.Equals(ConsoleKey.Backspace):
                            if (!string.IsNullOrEmpty(readInput))
                            {
                                Console.Write("\b");
                                readInput = readInput.Remove(readInput.Length - 1);
                            }
                            InputIsValid = true;
                            break;

                        case var key when key.Key.Equals(ConsoleKey.Enter):
                            if (!string.IsNullOrEmpty(readInput))
                            {
                                if (HandleEntry(Entries, (Convert.ToInt32(readInput) - 1)))
                                {
                                    MenuExitIsPending = true;
                                }
                                readInput = string.Empty;
                                InputIsValid = true;
                            }
                            break;

                        case var key when int.TryParse(key.KeyChar.ToString(), out choiceNum):
                            if (string.IsNullOrEmpty(readInput) && choiceNum.Equals(0))
                            {
                                InputIsValid = true;
                                if (ZeroMethod(Entries))
                                {
                                    MenuExitIsPending = true;
                                }
                            }
                            else
                            {
                                if (Convert.ToInt32(readInput + Convert.ToString(choiceNum)) <= Entries.Count)
                                {
                                    InputIsValid = true;
                                    int matchingEntries = 0;
                                    readInput = readInput + Convert.ToString(choiceNum);
                                    for (int i = 0; i < Entries.Count; i++)
                                    {
                                        if (Convert.ToString(i + 1).StartsWith(readInput) || Convert.ToString(i + 1) == readInput) { matchingEntries++; }
                                    }
                                    if ((readInput.Length == Convert.ToString(Entries.Count).Length) || (matchingEntries == 1))
                                    {
                                        if (HandleEntry(Entries, (Convert.ToInt32(readInput) - 1)))
                                        {
                                            MenuExitIsPending = true;
                                        }
                                        readInput = string.Empty;
                                    }
                                }
                                else
                                {
                                    ResetInput();
                                }
                            }
                            break;

                        default:
                            Console.WriteLine();
                            ResetInput();
                            break;
                    }
                }

            }
        }

        /// <summary>
        /// Clears the last two console lines and sets the cursor one y-position back.
        /// Will also display a error message for a short time. 
        /// </summary>
        /// <param name="errorMsg">The error message to be displayed.</param>
        public static void ResetInput(string errorMsg = "")
        {
            // Display error and wait.
            Console.Write(string.Format("[{0}] {1}", DefaultErrorName, !string.IsNullOrWhiteSpace(errorMsg) ? errorMsg : DefaultInputError));
            new System.Threading.ManualResetEvent(false).WaitOne(150);
            // Clear the current line.
            ClearCurrentConsoleLine();
            // Clear the line above it and set it as the new cursor position.
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            ClearCurrentConsoleLine();
        }

        /// <summary>
        /// Clears the current console line and resets the cursor.
        /// </summary>
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        /// <summary>
        /// Clears the entire console if that is desired.
        /// Wrapper for Console.Clear() which checks for the ClearOnSwitch boolean.
        /// </summary>
        public static void ClearMenu()
        {
            if (ClearOnSwitch)
            {
                Console.Clear();
            }
        }


    }
}
