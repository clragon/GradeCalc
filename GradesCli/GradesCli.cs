﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace Grades
{
    public static class Cli
    {
        /// <summary>
        /// Displays the main menu.
        /// From this menu, all other menus are accessible.
        /// This should be called to spawn a console interface for the user.
        /// </summary>
        public static void CliMenu()
        {
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.OverrideLanguage)
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = Properties.Settings.Default.Language;
            }

            // Catching CTRL+C event
            Console.CancelKeyPress += new ConsoleCancelEventHandler(IsCliExitPendingHandler);

            // Setting the bool for clearing console on menu switch
            ClearOnSwitch = true;

            // Setting the console and menu title
            NewConsoleTitle = Lang.GetString("Title");
            Console.Title = NewConsoleTitle;

            // Displaying the menu

            // Loop to prevent unwanted app-exit.
            bool IsAppExitPending = false;
            while (!IsAppExitPending)
            {
                // Clearing all previous console lines.
                ClearMenu();
                // Displaying the title of the menu.
                // Options are assigned with an number to choose from.
                Console.WriteLine("--- {0} ---", NewConsoleTitle);
                Console.WriteLine("[1] {0}", Lang.GetString("Subjects"));
                Console.WriteLine("[2] {0}", Lang.GetString("Overview"));
                Console.WriteLine("[3] {0}", Lang.GetString("Table"));
                Console.WriteLine("[4] {0}", Lang.GetString("Settings"));
                Console.WriteLine("[q] {0}", Lang.GetString("Exit"));
                Console.Write("\n");

                // Loop to enforce valid input.
                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    // Acquire a character.
                    Console.Write("{0}> ", Lang.GetString("Choose"));
                    string input = Console.ReadKey().KeyChar.ToString();
                    // Nifty short wait time for better feel of the app.
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    // New line since ReadKey doesnt print one.
                    Console.Write("\n");
                    // Line to prevent setting the cursor too far back with ResetInput if the user entered a newline character.
                    if (input == "\n") { Console.SetCursorPosition(0, Console.CursorTop - 1); }
                    switch (input)
                    {
                        case "1":
                            // Breaking the valid input inforcing loop.
                            IsInputValid = true;
                            // Calling the menu for choosing a subject.
                            ChooseSubject();
                            break;

                        case "2":
                            IsInputValid = true;
                            // Calling the overview menu.
                            OverviewMenu();
                            break;

                        case "3":
                            IsInputValid = true;
                            // Calling the menu to manage tables and their files.
                            ManageTable();
                            break;

                        case "4":
                            IsInputValid = true;
                            // Calling the settings menu.
                            Settings();
                            break;

                        case "q":
                            IsInputValid = true;
                            // Exit the app.
                            IsAppExitPending = true;
                            break;

                        default:
                            // Reset the cursor and clear input, then try again.
                            ResetInput();
                            break;
                    }
                }
            }

            ExitCli();
        }

        // Deprecated. Kept here for future reference.
        // public static ResourceManager Lang = new ResourceManager("language", typeof(Cli).Assembly);

        /// <summary>
        /// The ResourceManager for all language-dependent strings in the interface.
        /// </summary>
        public static ResourceManager Lang = language.ResourceManager;

        /// <summary>
        /// The currently open sourcefile.
        /// </summary>
        public static string SourceFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "/" + Properties.Settings.Default.SourceFile);

        /// <summary>
        /// The currently open Table.
        /// </summary>
        public static Table t = LoadTable();

        /// <summary>
        /// Loads a table. 
        /// Will create a new one if there is none found at the location of the current sourcefile.
        /// </summary>
        public static Table LoadTable()
        {
            // Checks if the current sourcefile exists.
            if (System.IO.File.Exists(SourceFile))
            {
                // Try to load it.
                try
                {
                    return Table.Read(SourceFile);
                }
                // Catch UnauthorizedAccessException and display an error for the user.
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("[{0}] {1} : {2}", Lang.GetString("Error"), System.IO.Path.GetFileName(SourceFile), Lang.GetString("DeniedTableAccess"));
                    Console.WriteLine(Lang.GetString("PressAnything"));
                    Console.ReadKey();
                    return GetEmptyTable();
                }
                // Catch any other error that might occur.
                catch (Exception)
                {
                    try
                    {
                        // Delete the sourcefile since it might be corrupted.
                        new Table().Clear(SourceFile);
                    }
                    catch (Exception) { }
                    // Return an empty table.
                    return GetEmptyTable();
                }
            }
            else
            {
                // Return an empty table.
                return GetEmptyTable();
            }
        }

        /// <summary>
        /// Will create a new table with default values to make fast and intuitive usage of the calculator possible.
        /// </summary>
        public static Table GetEmptyTable()
        {
            // Create a new table.
            Table t = new Table
            {
                // Default values for new tables.
                name = "terminal_" + DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss"),
                MinGrade = Properties.Settings.Default.MinGrade,
                MaxGrade = Properties.Settings.Default.MaxGrade,
                EnableGradeLimits = Properties.Settings.Default.EnableGradeLimits,
                EnableWeightSystem = Properties.Settings.Default.EnableWeightSystem
            };
            return t;
        }

        /// <summary>
        /// Displays a menu to manage tables and their respective files.
        /// </summary>
        public static void ManageTable()
        {
            // Save all unsaved table data.
            t.Save();
            // Same menu scheme as seen in the main menu.
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {

                ClearMenu();
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ManageTable"), t.name);
                Console.WriteLine("[1] {0}", Lang.GetString("ReadTable"));
                Console.WriteLine("[2] {0}", Lang.GetString("WriteTable"));
                Console.WriteLine("[3] {0}", Lang.GetString("DefaultTable"));
                Console.WriteLine("[4] {0}", Lang.GetString("RenameTable"));
                Console.WriteLine("[5] {0}", Lang.GetString("DeleteTable"));
                Console.WriteLine("[q] {0}", Lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> ", Lang.GetString("Choose"));
                    string input = Console.ReadKey().KeyChar.ToString();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    Console.Write("\n");
                    if (input == "\n") { Console.SetCursorPosition(0, Console.CursorTop - 1); }
                    switch (input)
                    {
                        case "1":
                            IsInputValid = true;
                            ChooseTable();
                            break;

                        case "2":
                            IsInputValid = true;
                            t.Save(true);
                            new System.Threading.ManualResetEvent(false).WaitOne(20);
                            break;

                        case "3":
                            IsInputValid = true;
                            Properties.Settings.Default.SourceFile = System.IO.Path.GetFileName(SourceFile);
                            Properties.Settings.Default.Save();
                            new System.Threading.ManualResetEvent(false).WaitOne(20);
                            break;

                        case "4":
                            IsInputValid = true;
                            RenameTable();
                            break;

                        case "5":
                            bool IsDeleteInputValid = false;
                            while (!IsDeleteInputValid)
                            {
                                // Ask the user for confirmation of deleting the current table.
                                // This is language-dependent.
                                Console.Write("{0}? [{1}]> ", Lang.GetString("DeleteTable"), Lang.GetString("YesOrNo"));
                                string deleteInput = Console.ReadKey().KeyChar.ToString();
                                new System.Threading.ManualResetEvent(false).WaitOne(20);
                                Console.Write("\n");
                                // Comparing the user input incase-sensitive to the current language's character for "Yes" (For example "Y").
                                if (string.Equals(deleteInput, Lang.GetString("Yes"), StringComparison.OrdinalIgnoreCase))
                                {
                                    IsDeleteInputValid = true;
                                    IsInputValid = true;
                                    t.Clear(SourceFile);
                                    ChooseTable(false);
                                }
                                // Comparing the user input incase-sensitive to the current language's character for "No" (For example "N").
                                else if (string.Equals(deleteInput, Lang.GetString("No"), StringComparison.OrdinalIgnoreCase))
                                {
                                    IsDeleteInputValid = true;
                                    IsInputValid = true;
                                }
                                // Input seems to be invalid, resetting the field.
                                else
                                {
                                    ResetInput();
                                }

                            }
                            break;

                        case "q":
                            // Exit the menu.
                            IsMenuExitPending = true;
                            IsInputValid = true;
                            break;

                        default:
                            // Reset the input.
                            ResetInput();
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// Displays a menu for choosing and loading a table.
        /// </summary>
        /// <param name="UserCanAbort">Wether the user can exit the menu without choosing a table or not.</param>
        public static void ChooseTable(bool UserCanAbort = true)
        {
            int index = -1;
            string InputString = "";
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                // Counter for printed entries.
                int printedEntries = 0;
                // List for found tables.
                List<string> tables = new List<string>();
                try
                {
                    // Fetching all files in the app directory that have the "grades.xml" ending.
                    tables = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*grades.xml").ToList();
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("DeniedTableAccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                catch (Exception) { }
                // Printing the menu. Each table has a number assigned to it.
                // The number is it's index (in the List of tables) + 1.
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ChooseTable"), tables.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(tables.Count).Length, ' '), Lang.GetString("CreateTable"));
                if (tables.Any())
                {
                    // Sorting the tables alphabetically.
                    tables.Sort((a, b) => a.CompareTo(b));
                    // Getting the maximum name length of the tables for padding.
                    int MaxLength = tables.Select(x => System.IO.Path.GetFileName(x).Length).Max();
                    for (int i = 0; i < tables.Count; i++)
                    {
                        try
                        {
                            // If the current input string is empty, print from 0 onwards.
                            if (InputString == "")
                            {
                                Console.WriteLine("[{0}] {1}", Convert.ToString(i + 1).PadLeft(Convert.ToString(tables.Count).Length, ' '), System.IO.Path.GetFileName(tables[i]).PadRight(MaxLength, ' ') + " | " + Table.Read(tables[i]).name);
                            }
                            // If the current input string contains anything, try to display only entries that start with the numbers in the input string.
                            else
                            {
                                if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString)
                                {
                                    Console.WriteLine("[{0}] {1}", Convert.ToString(i + 1).PadLeft(Convert.ToString(tables.Count).Length, ' '), System.IO.Path.GetFileName(tables[i]).PadRight(MaxLength, ' ') + " | " + Table.Read(tables[i]).name);
                                    printedEntries++;
                                }
                            }
                        }
                        // If any exception occurs, remove the table from the list and subtract it from the integer compared against printed entries.
                        catch (Exception)
                        {
                            tables.RemoveAt(i);
                            i--;
                        }

                        // Match the amount of printed tables against the window height. The height is subtracted by 5 to account for newlines and input.
                        if (tables.Count > Console.WindowHeight - 5)
                        {
                            if (printedEntries == Console.WindowHeight - 6)
                            {
                                Console.WriteLine("[{0}]", ".".PadLeft(Convert.ToString(tables.Count).Length, '.'));
                                break;
                            }
                        }
                        // If there are enough entries printed, break the loop.
                        else { if (printedEntries == Console.WindowHeight - 5) { break; } }

                    }
                }
                // If the user can abort the menu, print the option for it.
                if (UserCanAbort)
                {
                    Console.WriteLine("[{0}] {1}", "q".PadLeft(Convert.ToString(tables.Count).Length, ' '), Lang.GetString("Back"));
                }
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> {1}", Lang.GetString("Choose"), InputString);
                    string input = Console.ReadKey().KeyChar.ToString();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    switch (input)
                    {
                        case "q":
                            // If the user can abort the menu, abort the menu.
                            if (UserCanAbort)
                            {
                                IsInputValid = true;
                                IsMenuExitPending = true;
                            }
                            // Else print the default message for invalid input.
                            else
                            {
                                ResetInput();
                            }
                            break;

                        case "\b":
                            // if the user enters a backslash, remove one number from the input string.
                            if (!(InputString == ""))
                            {
                                Console.Write("\b");
                                InputString = InputString.Remove(InputString.Length - 1);
                            }
                            IsInputValid = true;
                            break;

                        case "\n":
                        case "\r":
                            // If the user has entered a newline, check if the input string matches any number.
                            // If any number is matched, it is then set as the index and the menu closes.
                            // if the input string is empty, reset the cursor to prevent issues with ResetInput.
                            if (InputString == "")
                            {
                                Console.SetCursorPosition(0, Console.CursorTop - 1);
                            }
                            else
                            {
                                // Tries to convert input string into an index.
                                index = Convert.ToInt32(InputString) - 1;
                                InputString = "";
                                IsInputValid = true;
                                try
                                {
                                    // Loads the new table.
                                    t = Table.Read(tables[index]);
                                    // Sets the new sourcefile.
                                    SourceFile = tables[index];
                                    IsMenuExitPending = true;
                                }
                                // If the table cannot be loaded, throw an error.
                                catch (Exception)
                                {
                                    ResetInput(string.Format("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("ReadTableError")));
                                }
                            }
                            break;

                        default:
                            // A number was chosen as option.
                            Console.Write("\n");
                            int choice;
                            if ((int.TryParse(input, out choice)))
                            {
                                // if the input is 0, call the menu to create a new table.
                                if ((InputString == "") && (choice == 0))
                                {
                                    IsInputValid = true;
                                    CreateTable();
                                }
                                else
                                {
                                    // Check if this input is even in the maximum index of the table list. 
                                    if (Convert.ToInt32(InputString + Convert.ToString(choice)) <= tables.Count)
                                    {
                                        // Int for items that match the input string.
                                        int MatchingItems = 0;
                                        // Set new input string.
                                        InputString = InputString + Convert.ToString(choice);
                                        // Calculate how many items match their numbers with the input string.
                                        for (int i = 0; i < tables.Count; i++) { if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString) { MatchingItems++; } }
                                        // Check if any item has a direct match with the input string and if so, choose it and exit.
                                        if ((InputString.Length == Convert.ToString(tables.Count).Length) || (MatchingItems == 1))
                                        {
                                            // Get the tables actual index by subtracting 1 again.
                                            index = Convert.ToInt32(InputString) - 1;
                                            InputString = "";
                                            IsInputValid = true;
                                            try
                                            {
                                                t = Table.Read(tables[index]);
                                                SourceFile = tables[index];
                                                IsMenuExitPending = true;
                                            }
                                            catch (Exception)
                                            {
                                                ResetInput(string.Format("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("ReadTableError")));
                                            }
                                        }
                                        else
                                        {
                                            IsInputValid = true;
                                        }
                                    }
                                    else
                                    {
                                        ResetInput();
                                    }
                                }
                            }
                            else
                            {
                                ResetInput();
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Displays a menu for creating a new table.
        /// </summary>
        public static void CreateTable()
        {
            // Get a new table.
            Table x = GetEmptyTable();
            // Get the name for it through user input.
            x.name = GetTable(string.Format("--- {0} ---", Lang.GetString("CreateTable")));
            // Create a file for the table.
            // Files will be automatically named grades.xml with an increasing number in front of them.
            if (System.IO.File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "grades.xml")))
            {
                int i = 1;
                while (true)
                {
                    i++;
                    if (!(System.IO.File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + string.Format("/" + i + "." + "grades.xml")))))
                    {
                        x.Write(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + string.Format("/" + i + "." + "grades.xml")));
                        break;
                    }
                }
            }
            else
            {
                x.Write(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "grades.xml"));
            }

        }

        /// <summary>
        /// Displays a menu for renaming the currently loaded table.
        /// </summary>
        public static void RenameTable()
        {
            // Get the new name for the table through user input.
            t.name = GetTable(string.Format("--- {0} : {1} ---", Lang.GetString("RenameTable"), t.name));
            // Save the table to it's file.
            t.Save();
        }

        /// <summary>
        /// Basic underlying menu scheme for creating or renaming a table.
        /// </summary>
        /// <param name="title">Title of the menu. Usually create or rename.</param>  
        public static string GetTable(string title)
        {
            string input = "";
            bool IsInputValid = false;
            while (!IsInputValid)
            {

                ClearMenu();
                Console.WriteLine(title);
                Console.Write("\n");
                Console.Write("{0}> ", Lang.GetString("NameOfTable"));
                input = Console.ReadLine();

                // Trim the input.
                input.Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    // Check if the input is equal to the CreateTable option to counter sneaky users.
                    if (!input.Equals(string.Format("({0})", Lang.GetString("CreateTable")), StringComparison.InvariantCultureIgnoreCase))
                    {
                        IsInputValid = true;
                    }
                    else
                    {
                        ResetInput();
                    }
                }
                else
                {
                    ResetInput();
                }
            }

            return input;
        }

        /// <summary>
        /// Displays a menu for managing subjects.
        /// </summary>
        /// <param name="s">The subject that is to be managed.</param>
        public static void ManageSubject(Table.Subject s)
        {
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                // Display the number of possible options if there are any.
                if (s.Grades.Any())
                {
                    Console.WriteLine("--- {0} : {1} : {2} ---", Lang.GetString("Subject"), s.Name, s.CalcAverage());
                }
                else
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("Subject"), s.Name);
                }
                Console.WriteLine("[1] {0}", Lang.GetString("Grades"));
                Console.WriteLine("[2] {0}", Lang.GetString("RenameSubject"));
                Console.WriteLine("[3] {0}", Lang.GetString("DeleteSubject"));
                Console.WriteLine("[q] {0}", Lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> ", Lang.GetString("Choose"));
                    string input = Console.ReadKey().KeyChar.ToString();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    Console.Write("\n");
                    if (input == "\n") { Console.SetCursorPosition(0, Console.CursorTop - 1); }
                    switch (input)
                    {
                        case "1":
                            IsInputValid = true;
                            ChooseGrade(s);
                            break;

                        case "2":
                            IsInputValid = true;
                            RenameSubject(s);
                            break;

                        case "3":
                            IsInputValid = true;
                            IsMenuExitPending = true;
                            t.RemSubject(t.Subjects.IndexOf(s));
                            break;

                        case "q":
                            IsInputValid = true;
                            IsMenuExitPending = true;
                            break;

                        default:
                            ResetInput();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Displays a menu for choosing a subject.
        /// </summary>
        public static void ChooseSubject()
        {
            int index = -1;
            string InputString = "";
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                int printedEntries = 0;
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ChooseSubject"), t.Subjects.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), Lang.GetString("CreateSubject"));
                for (int i = 0; i < t.Subjects.Count; i++)
                {
                    if (InputString == "")
                    {
                        Console.WriteLine("[{0}] {1}", Convert.ToString(i + 1).PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), t.Subjects[i].Name);
                        printedEntries++;
                    }
                    else
                    {
                        if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString)
                        {
                            Console.WriteLine("[{0}] {1}", Convert.ToString(i + 1).PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), t.Subjects[i].Name);
                            printedEntries++;
                        }
                    }

                    if (t.Subjects.Count > Console.WindowHeight - 5)
                    {
                        if (printedEntries == Console.WindowHeight - 6)
                        {
                            Console.WriteLine("[{0}]", ".".PadLeft(Convert.ToString(t.Subjects.Count).Length, '.'));
                            break;
                        }
                    }
                    else { if (printedEntries == Console.WindowHeight - 5) { break; } }

                }
                Console.WriteLine("[{0}] {1}", "q".PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), Lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> {1}", Lang.GetString("Choose"), InputString);
                    string input = Console.ReadKey().KeyChar.ToString();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    switch (input)
                    {
                        case "q":
                            IsInputValid = true;
                            IsMenuExitPending = true;
                            break;

                        case "\b":
                            if (!(InputString == ""))
                            {
                                Console.Write("\b");
                                InputString = InputString.Remove(InputString.Length - 1);
                            }
                            IsInputValid = true;
                            break;

                        case "\n":
                        case "\r":
                            if (InputString == "")
                            {
                                Console.SetCursorPosition(0, Console.CursorTop - 1);
                            }
                            else
                            {
                                index = Convert.ToInt32(InputString) - 1;
                                InputString = "";
                                IsInputValid = true;
                                ManageSubject(t.Subjects[index]);
                            }
                            break;

                        default:
                            Console.Write("\n");
                            int choice;
                            if ((int.TryParse(input, out choice)))
                            {
                                if ((InputString == "") && (choice == 0))
                                {
                                    IsInputValid = true;
                                    CreateSubject();
                                }
                                else
                                {
                                    if (Convert.ToInt32(InputString + Convert.ToString(choice)) <= t.Subjects.Count)
                                    {
                                        int MatchingItems = 0;
                                        InputString = InputString + Convert.ToString(choice);
                                        for (int i = 0; i < t.Subjects.Count; i++) { if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString) { MatchingItems++; } }
                                        if ((InputString.Length == Convert.ToString(t.Subjects.Count).Length) || (MatchingItems == 1))
                                        {
                                            index = Convert.ToInt32(InputString) - 1;
                                            InputString = "";
                                            IsInputValid = true;
                                            ManageSubject(t.Subjects[index]);
                                        }
                                        else
                                        {
                                            IsInputValid = true;
                                        }
                                    }
                                    else
                                    {
                                        // InputString = InputString.Remove(InputString.Length - 1);
                                        ResetInput();
                                    }
                                }
                            }
                            else
                            {
                                ResetInput();
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Displays a menu for creating a new subject.
        /// </summary>
        public static void CreateSubject()
        {
            t.AddSubject(GetSubject(string.Format("--- {0} ---", Lang.GetString("CreateSubject"))));
            t.Save();
        }

        /// <summary>
        /// Displays a menu for renaming an existing subject.
        /// </summary>
        /// <param name="s">The subject that is to be renamed.</param>
        public static void RenameSubject(Table.Subject s)
        {
            s.EditSubject(GetSubject(string.Format("--- {0} : {1} ---", Lang.GetString("RenameSubject"), s.Name)));
            t.Save();
        }

        public static string GetSubject(string title)
        {
            string input = "";
            bool IsInputValid = false;
            while (!IsInputValid)
            {

                ClearMenu();
                Console.WriteLine(title);
                Console.Write("\n");
                Console.Write("{0}> ", Lang.GetString("NameOfSubject"));
                input = Console.ReadLine();

                input.Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (!input.Equals(string.Format("({0})", Lang.GetString("CreateSubject")), StringComparison.InvariantCultureIgnoreCase))
                    {
                        IsInputValid = true;
                    }
                    else
                    {
                        ResetInput();
                    }
                }
                else
                {
                    ResetInput();
                }
            }

            return input;
        }

        /// <summary>
        /// Displays a menu for managing a grade.
        /// </summary>
        /// <param name="g">The grade that is to be managed.</param>
        public static void ManageGrade(Table.Subject.Grade g)
        {
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                Console.WriteLine("--- {0} : {1} | {2} ---", Lang.GetString("Grade"), g.Value, g.Weight);
                Console.WriteLine("[1] {0}", Lang.GetString("EditGrade"));
                Console.WriteLine("[2] {0}", Lang.GetString("DeleteGrade"));
                Console.WriteLine("[q] {0}", Lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> ", Lang.GetString("Choose"));
                    string input = Console.ReadKey().KeyChar.ToString();
                    Console.Write("\n");
                    if (input == "\n") { Console.SetCursorPosition(0, Console.CursorTop - 1); }
                    switch (input)
                    {

                        case "1":
                            IsInputValid = true;
                            ModifyGrade(g);
                            break;

                        case "2":
                            IsInputValid = true;
                            IsMenuExitPending = true;
                            // Remove the grade by using the OwnerSubject attribute.
                            // Effectively bypassing the need to pass the subject in which the grade is in.
                            g.OwnerSubject.RemGrade(g);
                            break;

                        case "q":
                            IsInputValid = true;
                            IsMenuExitPending = true;
                            break;

                        default:
                            ResetInput();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Displays a menu for choosing a grade.
        /// </summary>
        /// <param name="s">The subject which grades can be chosen from.</param>
        public static void ChooseGrade(Table.Subject s)
        {
            int index = -1;
            string InputString = "";
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                int printedEntries = 0;
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ChooseGrade"), s.Grades.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(s.Grades.Count).Length, ' '), Lang.GetString("CreateGrade"));
                if (s.Grades.Any())
                {
                    int i = 0;
                    // Calculate the maximum length of the values of all grades in the subject for padding.
                    int MaxLength = s.Grades.Select(x => x.Value.ToString().Length).Max();
                    foreach (Table.Subject.Grade g in s.Grades)
                    {
                        i++;
                        if (InputString == "")
                        {
                            Console.WriteLine("[{0}] {1} | {2}", Convert.ToString(i).PadLeft(Convert.ToString(s.Grades.Count).Length, ' '), Convert.ToString(g.Value).PadRight(MaxLength, ' '), g.Weight);
                            printedEntries++;
                        }
                        else
                        {
                            if (Convert.ToString(i).StartsWith(InputString) || Convert.ToString(i) == InputString)
                            {
                                Console.WriteLine("[{0}] {1} | {2}", Convert.ToString(i).PadLeft(Convert.ToString(s.Grades.Count).Length, ' '), Convert.ToString(g.Value).PadRight(MaxLength, ' '), g.Weight);
                                printedEntries++;
                            }
                        }

                        if (s.Grades.Count > Console.WindowHeight - 5)
                        {
                            if (printedEntries == Console.WindowHeight - 6)
                            {
                                Console.WriteLine("[{0}]", ".".PadLeft(Convert.ToString(s.Grades.Count).Length, '.'));
                                break;
                            }
                        }
                        else { if (printedEntries == Console.WindowHeight - 5) { break; } }
                    }
                }
                Console.WriteLine("[{0}] {1}", "q".PadLeft(Convert.ToString(s.Grades.Count).Length, ' '), Lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> {1}", Lang.GetString("Choose"), InputString);
                    string input = Console.ReadKey().KeyChar.ToString();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    switch (input)
                    {
                        case "q":
                            IsInputValid = true;
                            IsMenuExitPending = true;
                            break;

                        case "\b":
                            if (!(InputString == ""))
                            {
                                Console.Write("\b");
                                InputString = InputString.Remove(InputString.Length - 1);
                            }
                            IsInputValid = true;
                            break;

                        case "\n":
                        case "\r":
                            if (InputString == "")
                            {
                                Console.SetCursorPosition(0, Console.CursorTop - 1);
                            }
                            else
                            {
                                index = Convert.ToInt32(InputString) - 1;
                                InputString = "";
                                IsInputValid = true;
                                // Calls a menu to manage the grade with the index we just acquired.
                                ManageGrade(s.Grades[index]);
                            }
                            break;

                        default:
                            Console.Write("\n");
                            int choice;
                            if ((int.TryParse(input, out choice)))
                            {
                                if ((InputString == "") && (choice == 0))
                                {
                                    IsInputValid = true;
                                    CreateGrade(s);
                                }
                                else
                                {
                                    if (Convert.ToInt32(InputString + Convert.ToString(choice)) <= s.Grades.Count)
                                    {
                                        int MatchingItems = 0;
                                        InputString = InputString + Convert.ToString(choice);
                                        for (int i = 0; i < s.Grades.Count; i++) { if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString) { MatchingItems++; } }
                                        if ((InputString.Length == Convert.ToString(s.Grades.Count).Length) || (MatchingItems == 1))
                                        {
                                            index = Convert.ToInt32(InputString) - 1;
                                            InputString = "";
                                            IsInputValid = true;
                                            ManageGrade(s.Grades[index]);
                                        }
                                        else
                                        {
                                            IsInputValid = true;
                                        }
                                    }
                                    else
                                    {
                                        ResetInput();
                                    }
                                }
                            }
                            else
                            {
                                ResetInput();
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Displays a menu for creating a new grade.
        /// </summary>
        /// <param name="s">The subject in which a grade is to be created in.</param>
        public static void CreateGrade(Table.Subject s)
        {
            // Gets the values needed to create a grade from the menu template as tuple.
            Tuple<double, double> g = GetGrade(s, string.Format("--- {0} ---", Lang.GetString("CreateGrade")));
            // Adds a new grade with the received values.
            s.AddGrade(g.Item1, g.Item2);
            t.Save();

        }

        /// <summary>
        /// Displays a menu for editing an existing grade.
        /// </summary>
        /// <param name="g">The grade which is to be renamed.</param>
        public static void ModifyGrade(Table.Subject.Grade g)
        {
            // Gets the values needed to edit a grade from the menu template as tuple.
            Tuple<double, double> n = GetGrade(g.OwnerSubject, string.Format("--- {0} : {1} | {2} ---", Lang.GetString("EditGrade"), g.Value, g.Weight));
            // Edits the grade with the received values.
            g.EditGrade(n.Item1, n.Item2);
            t.Save();

        }

        /// <summary>
        /// Basic underlying menu scheme for creating or editing a grade.
        /// </summary>
        /// <param name="title">Title of the menu. Usually create or edit.</param>
        /// <param name="s">The subject in which grades are to be edited in.</param>
        public static Tuple<double, double> GetGrade(Table.Subject s, string title)
        {
            string input = "";
            double value = -1;
            double weight = -1;
            bool IsFirstInputValid = false;
            while (!IsFirstInputValid)
            {
                ClearMenu();
                Console.WriteLine(title);

                Console.Write("\n");
                Console.Write("{0}> ", Lang.GetString("Grade"));
                input = Console.ReadLine();

                if (double.TryParse(input, out value))
                {
                    // Check if the table has grade limits enabled.
                    if (s.OwnerTable.EnableGradeLimits)
                    {
                        // Check if the grade is within the table limits.
                        if ((value >= s.OwnerTable.MinGrade) && (value <= s.OwnerTable.MaxGrade))
                        {
                            IsFirstInputValid = true;
                        }
                        else
                        {
                            ResetInput();
                        }
                    }
                    // Else add the grade.
                    else
                    {
                        IsFirstInputValid = true;
                    }
                }
                else
                {
                    ResetInput();
                }
            }
            // Check if the table has the weight system enabled.
            if (s.OwnerTable.EnableWeightSystem)
            {
                // Get a weight for the grade from user input.
                bool IsSecondInputValid = false;
                while (!IsSecondInputValid)
                {
                    Console.Write("{0}> ", Lang.GetString("Weight"));
                    input = Console.ReadLine();

                    if (double.TryParse(input, out weight) && (weight > 0) && (weight <= 1) || (weight == 1.5) || (weight == 2))
                    {
                        IsSecondInputValid = true;
                    }
                    else
                    {
                        ResetInput();
                    }
                }
            }
            // Give the grade a default weight of 1.
            else
            {
                weight = 1;
            }

            return Tuple.Create(value, weight);
        }

        /// <summary>
        /// Display an overview of all subject averages, the total average and compensation points.
        /// </summary>
        public static void OverviewMenu()
        {
            ClearMenu();
            if (t.MinGrade == 1 && t.MaxGrade == 6)
            {
                if (t.Subjects.Any())
                {
                    // Calculate the maximum length of any word in front of the bar diagramm.
                    int MaxLength = t.Subjects.Select(x => x.Name.Length).Max();
                    if (MaxLength < Lang.GetString("Overview").Length) { MaxLength = Lang.GetString("Overview").Length; }
                    if (MaxLength < Lang.GetString("Total").Length) { MaxLength = Lang.GetString("Total").Length; }
                    if (Properties.Settings.Default.DisplayCompensation) { if (MaxLength < Lang.GetString("Compensation").Length) { MaxLength = Lang.GetString("Compensation").Length; } }
                    // Display the bar diagramm meter.
                    Console.WriteLine("{0} : 1 2 3 4 5 6: {1}", Lang.GetString("Overview").PadRight(MaxLength, ' '), Lang.GetString("Average"));
                    Console.Write("\n");
                    // Sort the subjects descending by their average grade.
                    t.Subjects.Sort((s1, s2) =>
                    {
                        return Convert.ToInt32(s2.CalcAverage() - s1.CalcAverage());
                    });
                    // Print a diagramm for each subject.
                    foreach (Table.Subject s in t.Subjects)
                    {
                        Console.WriteLine("{0} :{1}: {2}", s.Name.PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(s.CalcAverage() * 2)).PadRight(12, ' '), s.CalcAverage());
                    }
                    // Print total average grade.
                    Console.Write("\n");
                    Console.WriteLine("{0} :{1}: {2}", Lang.GetString("Total").PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(t.CalcAverage() * 2)).PadRight(12, ' '), t.CalcAverage());
                    Console.Write("\n");
                    // Print compensation points, if enabled.
                    if (Properties.Settings.Default.DisplayCompensation)
                    {
                        Console.Write("{0} {1}: {2}", Lang.GetString("Compensation").PadRight(MaxLength, ' '), new string(' ', 13), t.CalcCompensation());
                        Console.Write("\n");
                    }
                }
                // If no data is available, display a message for the user.
                else
                {
                    Console.WriteLine("{0} : {1}", Lang.GetString("Overview"), Lang.GetString("NoData"));
                }
            }
            else
            {
                Console.WriteLine("{0}", Lang.GetString("OverviewDataError"));
            }

            Console.Write("\n");
            // Prompt the user to press any key as soon as he is done looking at the diagramm.
            Console.Write("{0} {1}", Lang.GetString("PressAnything"), " ");
            Console.ReadKey();
        }

        /// <summary>
        /// Override saving method for tables. 
        /// It's purpose is to catch errors that can occur in the process.
        /// This method should always be used instead of the Table.Write method.
        /// </summary>
        /// <param name="verbose">Wether error messages should be displayed or not. disabled by default.</param>
        public static bool Save(this Table t, bool verbose = false)
        {
            try
            {
                // Try to save the table to the current sourcefile.
                t.Write(SourceFile);
                // Print a message that it was successful, if details are enabled.
                if (verbose)
                {
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("WriteTableSuccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                return true;
            }
            // Catch any error and write an error if details are enabled.
            catch (UnauthorizedAccessException)
            {
                if (verbose)
                {
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("DeniedTableAccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                return false;
            }
            catch (Exception)
            {
                if (verbose)
                {
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("WriteTableError"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                return false;
            }
        }

        /// <summary>
        /// Override method to calculate the average grade of a table.
        /// Uses the override method to calculate the average of a subject to function.
        /// </summary>
        public static double CalcAverage(this Table t)
        {
            if (t.Subjects.Any())
            {
                double averages = 0;
                foreach (Table.Subject s in t.Subjects)
                {
                    averages += s.CalcAverage();
                }
                // Rounded to 0.5
                // Average of the table is calculated by all averages of the subjects divided by the amounts of subjects.
                return Math.Round((averages / t.Subjects.Count) * 2, MidpointRounding.ToEven) / 2;
            }
            else { return 0; }
        }

        /// <summary>
        /// Override method to calculate the average grade of a subject.
        /// </summary>
        public static double CalcAverage(this Table.Subject s)
        {
            if (s.Grades.Any())
            {
                double values = 0, weights = 0;
                foreach (Table.Subject.Grade g in s.Grades)
                {
                    // If the weight system is enabled, get the weight of the grade.
                    if (s.OwnerTable.EnableWeightSystem)
                    {
                        weights += g.Weight;
                        // Actual value of a grade equals its value times it's weight.
                        values += g.Value * g.Weight;
                    }
                    // Else use the default weight of 1 for all grades.
                    else
                    {
                        weights++;
                        values += g.Value;
                    }
                    // Old relict. Left in here for future reference.
                    // Math.Round(g.weight * 4, MidpointRounding.ToEven) / 4
                }
                // Rounded to 0.5
                // Subject average is calculated by all avergaes of the grades divided by the amount of grades.
                return Math.Round((values / weights) * 2, MidpointRounding.ToEven) / 2;
            }
            else { return 0; }
        }

        /// <summary>
        /// Override method to calculate the compensation needed for a subject.
        /// </summary>
        public static double CalcCompensation(this Table.Subject s)
        {
            double points = 0;
            // Compensation points are calculated by subtracting 4 of the grade value.
            // Positive points have to outweight negative ones twice.
            points = (s.CalcAverage() - 4);
            if (points < 0) { points = (points * 2); }
            return points;
        }

        /// <summary>
        /// Override method to calculate the compensation needed for a table.
        /// Uses the override method to calculate the compensation needed for a subject to function.
        /// </summary>
        public static double CalcCompensation(this Table t)
        {
            if (t.Subjects.Any())
            {
                double points = 0;
                foreach (Table.Subject s in t.Subjects)
                {
                    points += s.CalcCompensation();
                }
                return points;
            }
            else { return 0; }
        }

        /// <summary>
        /// Wether the console should be cleared when displaying a new menu or not.
        /// </summary>
        public static bool ClearOnSwitch = Properties.Settings.Default.ClearOnSwitch;

        /// <summary>
        /// Clears the console if this is desired.
        /// This method should be always used instead of Console.Clear.
        /// </summary>
        public static void ClearMenu()
        {
            if (ClearOnSwitch)
            {
                Console.Clear();
            }
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
        /// Clears the last two console lines and sets the cursor one y-position back.
        /// Will also display a error message for a short time. 
        /// </summary>
        /// <param name="error">The error message to be displayed.</param>
        public static void ResetInput(string error = "$(default)")
        {
            // If no message was specified, display default.
            if (error == "$(default)")
            {
                error = string.Format("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("InvalidInput"));
            }
            // Display error and wait.
            Console.Write(error);
            new System.Threading.ManualResetEvent(false).WaitOne(150);
            // Clear the current line.
            ClearCurrentConsoleLine();
            // Clear the line above it and set it as the new cursor position.
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            ClearCurrentConsoleLine();
        }

        /// <summary>
        /// The new console title for the entire runtime of the app.
        /// </summary>
        public static string NewConsoleTitle;

        /// <summary>
        /// The old console title.
        /// It will be restored when exiting the app.
        /// </summary>
        public static string OldConsoleTitle = Console.Title;

        /// <summary>
        /// Exits the app, restores the old console title and clears the console.
        /// </summary>
        public static void ExitCli()
        {
            // Restore the console title.
            Console.Title = OldConsoleTitle;
            Console.WriteLine("Closing...");
            ClearMenu();
        }

        private static void IsCliExitPendingHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Call the exit function.
            ExitCli();
        }

        /// <summary>
        /// Override method to truncate a string.
        /// </summary>
        /// <param name="maxLength">The maximum length that is desired for this string.</param>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static void Settings()
        {
            bool IsAppExitPending = false;
            while (!IsAppExitPending)
            {
                ClearMenu();
                Console.WriteLine("--- {0} ---", NewConsoleTitle);
                Console.WriteLine("[ ] {0}", System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);
                Console.WriteLine("[1] {0}", Lang.GetString("ChooseLang"));
                Console.WriteLine("[q] {0}", Lang.GetString("Exit"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> ", Lang.GetString("Choose"));
                    string input = Console.ReadKey().KeyChar.ToString();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    Console.Write("\n");
                    if (input == "\n") { Console.SetCursorPosition(0, Console.CursorTop - 1); }
                    switch (input)
                    {
                        case "1":
                            IsInputValid = true;
                            ChooseLang();
                            break;

                        case "q":
                            IsInputValid = true;
                            IsAppExitPending = true;
                            break;

                        default:
                            ResetInput();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Displays a menu for choosing a language.
        /// </summary>
        public static void ChooseLang()
        {
            int index = -1;
            string InputString = "";
            bool IsMenuExitPending = false;
            List<System.Globalization.CultureInfo> Langs = GetAvailableCultures(Lang);
            while (!IsMenuExitPending)
            {
                ClearMenu();
                int printedEntries = 0;
                if (string.IsNullOrEmpty(Properties.Settings.Default.Language.Name))
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ChooseLang"), Langs.Count);
                }
                else
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ChooseLang"), Properties.Settings.Default.Language.Name);
                }
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(Langs.Count).Length, ' '), Lang.GetString("LangDefault"));
                if (Langs.Any())
                {
                    int i = 0;
                    foreach (System.Globalization.CultureInfo cult in Langs)
                    {
                        i++;
                        if (InputString == "")
                        {
                            Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(Langs.Count).Length, ' '), cult.Name);
                            printedEntries++;
                        }
                        else
                        {
                            if (Convert.ToString(i).StartsWith(InputString) || Convert.ToString(i) == InputString)
                            {
                                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(Langs.Count).Length, ' '), cult.Name);
                                printedEntries++;
                            }
                        }

                        if (Langs.Count > Console.WindowHeight - 5)
                        {
                            if (printedEntries == Console.WindowHeight - 6)
                            {
                                Console.WriteLine("[{0}]", ".".PadLeft(Convert.ToString(Langs.Count).Length, '.'));
                                break;
                            }
                        }
                        else { if (printedEntries == Console.WindowHeight - 5) { break; } }
                    }
                }
                Console.WriteLine("[{0}] {1}", "q".PadLeft(Convert.ToString(Langs.Count).Length, ' '), Lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> {1}", Lang.GetString("Choose"), InputString);
                    string input = Console.ReadKey().KeyChar.ToString();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    switch (input)
                    {
                        case "q":
                            IsInputValid = true;
                            IsMenuExitPending = true;
                            break;

                        case "\b":
                            if (!(InputString == ""))
                            {
                                Console.Write("\b");
                                InputString = InputString.Remove(InputString.Length - 1);
                            }
                            IsInputValid = true;
                            break;

                        case "\n":
                        case "\r":
                            if (InputString == "")
                            {
                                Console.SetCursorPosition(0, Console.CursorTop - 1);
                            }
                            else
                            {
                                index = Convert.ToInt32(InputString) - 1;
                                InputString = "";
                                IsInputValid = true;
                                Properties.Settings.Default.Language = Langs[index];
                                Properties.Settings.Default.OverrideLanguage = true;
                                Properties.Settings.Default.Save();
                                IsMenuExitPending = true;
                            }
                            break;

                        default:
                            Console.Write("\n");
                            int choice;
                            if ((int.TryParse(input, out choice)))
                            {
                                if ((InputString == "") && (choice == 0))
                                {
                                    IsInputValid = true;
                                    Properties.Settings.Default.Language = System.Globalization.CultureInfo.InvariantCulture;
                                    Properties.Settings.Default.OverrideLanguage = false;
                                    Properties.Settings.Default.Save();
                                    IsMenuExitPending = true;
                                }
                                else
                                {
                                    if (Convert.ToInt32(InputString + Convert.ToString(choice)) <= Langs.Count)
                                    {
                                        int MatchingItems = 0;
                                        InputString = InputString + Convert.ToString(choice);
                                        for (int i = 0; i < Langs.Count; i++) { if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString) { MatchingItems++; } }
                                        if ((InputString.Length == Convert.ToString(Langs.Count).Length) || (MatchingItems == 1))
                                        {
                                            index = Convert.ToInt32(InputString) - 1;
                                            InputString = "";
                                            IsInputValid = true;
                                            Properties.Settings.Default.Language = Langs[index];
                                            Properties.Settings.Default.OverrideLanguage = true;
                                            Properties.Settings.Default.Save();
                                            IsMenuExitPending = true;
                                        }
                                        else
                                        {
                                            IsInputValid = true;
                                        }
                                    }
                                    else
                                    {
                                        // InputString = InputString.Remove(InputString.Length - 1);
                                        ResetInput();
                                    }
                                }
                            }
                            else
                            {
                                ResetInput();
                            }
                            break;
                    }
                }
            }
        }


        public static List<System.Globalization.CultureInfo> GetAvailableCultures(ResourceManager resourceManager)
        {
            List<System.Globalization.CultureInfo> result = new List<System.Globalization.CultureInfo>();

            System.Globalization.CultureInfo[] cultures = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
            foreach (System.Globalization.CultureInfo culture in cultures)
            {
                try
                {
                    if (culture.Equals(System.Globalization.CultureInfo.InvariantCulture)) continue; // "==" won't work.

                    if (resourceManager.GetResourceSet(culture, true, false) != null)
                    {
                        result.Add(culture);
                    }
                }
                catch (System.Globalization.CultureNotFoundException) { }
            }
            return result;
        }
    }
}