using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace Grades
{
    public static class Cli
    {

        public static void CliMenu()
        {

            // catching CTRL+C event
            Console.CancelKeyPress += new ConsoleCancelEventHandler(IsCliExitPendingHandler);

            // setting the bool for clearing console on menu switch
            ClearOnSwitch = true;

            // setting the console and menu title
            NewConsoleTitle = lang.GetString("Title");
            Console.Title = NewConsoleTitle;

            // main menu implementation
            bool IsAppExitPending = false;
            while (!IsAppExitPending)
            {
                ClearMenu();
                Console.WriteLine("--- {0} ---", NewConsoleTitle);
                Console.WriteLine("[1] {0}", lang.GetString("Subjects"));
                Console.WriteLine("[2] {0}", lang.GetString("Overview"));
                Console.WriteLine("[3] {0}", lang.GetString("Table"));
                Console.WriteLine("[q] {0}", lang.GetString("Exit"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> ", lang.GetString("Choose"));
                    string input = Console.ReadKey().KeyChar.ToString();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    Console.Write("\n");
                    if (input == "\n") { Console.SetCursorPosition(0, Console.CursorTop - 1); }
                    switch (input)
                    {
                        case "1":
                            IsInputValid = true;
                            ChooseSubject();
                            break;

                        case "2":
                            IsInputValid = true;
                            OverviewMenu();
                            break;

                        case "3":
                            IsInputValid = true;
                            ManageTable();
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

            ExitCli();
        }

        public static ResourceManager lang = new ResourceManager("language", typeof(Cli).Assembly);

        public static string SourceFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "/grades.xml");

        public static Table t = LoadTable();

        public static Table LoadTable()
        {
            if (System.IO.File.Exists(SourceFile))
            {
                try
                {
                    return Table.Read(SourceFile);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("[{0}] {1} : {2}", lang.GetString("Error"), System.IO.Path.GetFileName(SourceFile), lang.GetString("DeniedTableAccess"));
                    Console.WriteLine(lang.GetString("PressAnything"));
                    Console.ReadKey();
                    return GetEmptyTable();
                }
                catch (Exception)
                {
                    try
                    {
                        new Table().Clear(SourceFile);
                    }
                    catch (Exception) { }
                    return GetEmptyTable();
                }
            }
            else
            {
                return GetEmptyTable();
            }
        }

        public static Table GetEmptyTable()
        {
            Table t = new Table();
            t.name = "terminal_" + DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss");
            t.MinGrade = 1;
            t.MaxGrade = 6;
            t.EnableWeightSystem = true;
            return t;
        }

        public static void ManageTable()
        {
            t.Save();
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {

                ClearMenu();
                Console.WriteLine("--- {0} : {1} ---", lang.GetString("ManageTable"), t.name);
                Console.WriteLine("[1] {0}", lang.GetString("ReadTable"));
                Console.WriteLine("[2] {0}", lang.GetString("WriteTable"));
                Console.WriteLine("[3] {0}", lang.GetString("RenameTable"));

                Console.WriteLine("[4] {0}", lang.GetString("DeleteTable"));
                Console.WriteLine("[q] {0}", lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> ", lang.GetString("Choose"));
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
                            break;

                        case "3":
                            IsInputValid = true;
                            RenameTable();
                            break;

                        case "4":
                            IsMenuExitPending = true;
                            IsInputValid = true;
                            t.Clear(SourceFile);
                            ChooseTable();
                            break;

                        case "q":
                            IsMenuExitPending = true;
                            IsInputValid = true;
                            break;

                        default:
                            ResetInput();
                            break;
                    }
                }
            }

        }

        public static void ChooseTable()
        {
            int index = -1;
            string InputString = "";
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                int printedEntries = 0;
                List<string> tables = new List<string>();
                try
                {
                    tables = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*grades.xml").ToList();
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("[{0}] {1}", lang.GetString("Error"), lang.GetString("DeniedTableAccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                catch (Exception) { }
                Console.WriteLine("--- {0} : {1} ---", lang.GetString("ChooseTable"), tables.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(tables.Count).Length, ' '), lang.GetString("CreateTable"));
                if (tables.Any())
                {
                    tables.Sort((a, b) => a.CompareTo(b));
                    int MaxLength = tables.Select(x => System.IO.Path.GetFileName(x).Length).Max();
                    for (int i = 0; i < tables.Count; i++)
                    {
                        try
                        {
                            if (InputString == "")
                            {
                                Console.WriteLine("[{0}] {1}", Convert.ToString(i + 1).PadLeft(Convert.ToString(tables.Count).Length, ' '), System.IO.Path.GetFileName(tables[i]).PadRight(MaxLength, ' ') + " | " + Table.Read(tables[i]).name);
                            }
                            else
                            {
                                if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString)
                                {
                                    Console.WriteLine("[{0}] {1}", Convert.ToString(i + 1).PadLeft(Convert.ToString(tables.Count).Length, ' '), System.IO.Path.GetFileName(tables[i]).PadRight(MaxLength, ' ') + " | " + Table.Read(tables[i]).name);
                                    printedEntries++;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            tables.RemoveAt(i);
                            i--;
                        }

                        if (tables.Count > Console.WindowHeight - 5)
                        {
                            if (printedEntries == Console.WindowHeight - 6)
                            {
                                Console.WriteLine("[{0}]", ".".PadLeft(Convert.ToString(tables.Count).Length, '.'));
                                break;
                            }
                        }
                        else { if (printedEntries == Console.WindowHeight - 5) { break; } }

                    }
                }
                Console.WriteLine("[{0}] {1}", "q".PadLeft(Convert.ToString(tables.Count).Length, ' '), lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> {1}", lang.GetString("Choose"), InputString);
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
                                try
                                {
                                    t = Table.Read(tables[index]);
                                    SourceFile = tables[index];
                                    IsMenuExitPending = true;
                                }
                                catch (Exception)
                                {
                                    ResetInput(string.Format("[{0}] {1}", lang.GetString("Error"), lang.GetString("ReadTableError")));
                                }
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
                                    if (Convert.ToInt32(InputString + Convert.ToString(choice)) <= tables.Count)
                                    {
                                        int MatchingItems = 0;
                                        InputString = InputString + Convert.ToString(choice);
                                        for (int i = 0; i < tables.Count; i++) { if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString) { MatchingItems++; } }
                                        if ((InputString.Length == Convert.ToString(tables.Count).Length) || (MatchingItems == 1))
                                        {
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
                                                ResetInput(string.Format("[{0}] {1}", lang.GetString("Error"), lang.GetString("ReadTableError")));
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

        public static void CreateTable()
        {

            Table x = GetEmptyTable();
            x.name = GetTable(String.Format("--- {0} ---", lang.GetString("CreateTable")));

            int i = 1;
            while (true)
            {
                i++;
                if (!(System.IO.File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + String.Format("/" + i + "." + "grades.xml")))))
                {
                    x.Write(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + String.Format("/" + i + "." + "grades.xml")));
                    break;
                }
            }
        }

        public static void RenameTable()
        {
            t.name = GetTable(String.Format("--- {0} : {1} ---", lang.GetString("RenameTable"), t.name));
            t.Save();
        }

        public static string GetTable(string title)
        {
            string input = "";
            bool IsInputValid = false;
            while (!IsInputValid)
            {

                ClearMenu();
                Console.WriteLine(title);
                Console.Write("\n");
                Console.Write("{0}> ", lang.GetString("NameOfTable"));
                input = Console.ReadLine();

                input.Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (!input.Equals(String.Format("({0})", lang.GetString("CreateTable")), StringComparison.InvariantCultureIgnoreCase))
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

        public static void ManageSubject(Table.Subject s)
        {
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                if (s.Grades.Any())
                {
                    Console.WriteLine("--- {0} : {1} : {2} ---", lang.GetString("Subject"), s.name, s.CalcAverage());
                }
                else
                {
                    Console.WriteLine("--- {0} : {1} ---", lang.GetString("Subject"), s.name);
                }
                Console.WriteLine("[1] {0}", lang.GetString("Grades"));
                Console.WriteLine("[2] {0}", lang.GetString("RenameSubject"));
                Console.WriteLine("[3] {0}", lang.GetString("DeleteSubject"));
                Console.WriteLine("[q] {0}", lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> ", lang.GetString("Choose"));
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

        public static void ChooseSubject()
        {
            int index = -1;
            string InputString = "";
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                int printedEntries = 0;
                Console.WriteLine("--- {0} : {1} ---", lang.GetString("ChooseSubject"), t.Subjects.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), lang.GetString("CreateSubject"));
                for (int i = 0; i < t.Subjects.Count; i++)
                {
                    if (InputString == "")
                    {
                        Console.WriteLine("[{0}] {1}", Convert.ToString(i + 1).PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), t.Subjects[i].name);
                        printedEntries++;
                    }
                    else
                    {
                        if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString)
                        {
                            Console.WriteLine("[{0}] {1}", Convert.ToString(i + 1).PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), t.Subjects[i].name);
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
                Console.WriteLine("[{0}] {1}", "q".PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> {1}", lang.GetString("Choose"), InputString);
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

        public static void CreateSubject()
        {
            t.AddSubject(GetSubject(String.Format("--- {0} ---", lang.GetString("CreateSubject"))));
            t.Save();
        }

        public static void RenameSubject(Table.Subject s)
        {
            s.EditSubject(GetSubject(String.Format("--- {0} : {1} ---", lang.GetString("RenameSubject"), s.name)));
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
                Console.Write("{0}> ", lang.GetString("NameOfSubject"));
                input = Console.ReadLine();

                input.Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (!input.Equals(String.Format("({0})", lang.GetString("CreateSubject")), StringComparison.InvariantCultureIgnoreCase))
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

        public static void ManageGrade(Table.Subject.Grade g)
        {
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                Console.WriteLine("--- {0} : {1} | {2} ---", lang.GetString("Grade"), g.value, g.weight);
                Console.WriteLine("[1] {0}", lang.GetString("EditGrade"));
                Console.WriteLine("[2] {0}", lang.GetString("DeleteGrade"));
                Console.WriteLine("[q] {0}", lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> ", lang.GetString("Choose"));
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

        public static void ChooseGrade(Table.Subject s)
        {
            int index = -1;
            string InputString = "";
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                int printedEntries = 0;
                Console.WriteLine("--- {0} : {1} ---", lang.GetString("ChooseGrade"), s.Grades.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(s.Grades.Count).Length, ' '), lang.GetString("CreateGrade"));
                if (s.Grades.Any())
                {
                    int i = 0;
                    int MaxLength = s.Grades.Select(x => x.value.ToString().Length).Max();
                    foreach (Table.Subject.Grade g in s.Grades)
                    {
                        i++;
                        if (InputString == "")
                        {
                            Console.WriteLine("[{0}] {1} | {2}", Convert.ToString(i).PadLeft(Convert.ToString(s.Grades.Count).Length, ' '), Convert.ToString(g.value).PadRight(MaxLength, ' '), g.weight);
                            printedEntries++;
                        }
                        else
                        {
                            if (Convert.ToString(i).StartsWith(InputString) || Convert.ToString(i) == InputString)
                            {
                                Console.WriteLine("[{0}] {1} | {2}", Convert.ToString(i).PadLeft(Convert.ToString(s.Grades.Count).Length, ' '), Convert.ToString(g.value).PadRight(MaxLength, ' '), g.weight);
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
                Console.WriteLine("[{0}] {1}", "q".PadLeft(Convert.ToString(s.Grades.Count).Length, ' '), lang.GetString("Back"));
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> {1}", lang.GetString("Choose"), InputString);
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

        public static void CreateGrade(Table.Subject s)
        {
            Tuple<double, double> g = GetGrade(s, String.Format("--- {0} ---", lang.GetString("CreateGrade")));
            s.AddGrade(g.Item1, g.Item2);
            t.Save();

        }

        public static void ModifyGrade(Table.Subject.Grade g)
        {
            Tuple<double, double> n = GetGrade(g.OwnerSubject, String.Format("--- {0} : {1} | {2} ---", lang.GetString("EditGrade"), g.value, g.weight));
            g.EditGrade(n.Item1, n.Item2);
            t.Save();

        }

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
                Console.Write("{0}> ", lang.GetString("Grade"));
                input = Console.ReadLine();

                if (double.TryParse(input, out value) && (value >= s.OwnerTable.MinGrade) && (value <= s.OwnerTable.MaxGrade))
                {
                    IsFirstInputValid = true;
                }
                else
                {
                    ResetInput();
                }
            }
            if (s.OwnerTable.EnableWeightSystem)
            {
                bool IsSecondInputValid = false;
                while (!IsSecondInputValid)
                {
                    Console.Write("{0}> ", lang.GetString("Weight"));
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
            else
            {
                weight = 1;
            }

            return Tuple.Create(value, weight);
        }

        public static void OverviewMenu()
        {
            ClearMenu();
            if (t.Subjects.Any())
            {
                int MaxLength = t.Subjects.Select(x => x.name.Length).Max();
                if (MaxLength < lang.GetString("Overview").Length) { MaxLength = lang.GetString("Overview").Length; }
                if (MaxLength < lang.GetString("Total").Length) { MaxLength = lang.GetString("Total").Length; }
                Console.WriteLine("{0} : 1 2 3 4 5 6: {1}", lang.GetString("Overview").PadRight(MaxLength, ' '), lang.GetString("Average"));
                Console.Write("\n");
                // sort list by highest average
                foreach (Table.Subject s in t.Subjects)
                {
                    Console.WriteLine("{0} :{1}: {2}", s.name.PadRight(MaxLength, ' '), new String('=', Convert.ToInt32(s.CalcAverage() * 2)).PadRight(12, ' '), s.CalcAverage());
                }
                Console.Write("\n");
                Console.WriteLine("{0} :{1}: {2}", lang.GetString("Total").PadRight(MaxLength, ' '), new String('=', Convert.ToInt32(t.CalcAverage() * 2)).PadRight(12, ' '), t.CalcAverage());

                // display compensation points
            }
            else
            {
                Console.WriteLine("{0} : {1}", lang.GetString("Overview"), lang.GetString("NoData"));
            }
            Console.Write("\n");
            Console.Write(lang.GetString("PressAnything"));
            Console.ReadKey();
        }

        public static bool Save(this Table t, bool verbose = false)
        {
            try
            {
                t.Write(SourceFile);
                if (verbose)
                {
                    Console.WriteLine("[{0}] {1}", lang.GetString("Log"), lang.GetString("WriteTableSuccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                if (verbose)
                {
                    Console.WriteLine("[{0}] {1}", lang.GetString("Error"), lang.GetString("DeniedTableAccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                return false;
            }
            catch (Exception)
            {
                if (verbose)
                {
                    Console.WriteLine("[{0}] {1}", lang.GetString("Error"), lang.GetString("WriteTableError"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                return false;
            }
        }

        public static double CalcAverage(this Table t)
        {
            if (t.Subjects.Any())
            {
                double averages = 0;
                foreach (Table.Subject s in t.Subjects)
                {
                    averages += s.CalcAverage();
                }
                return Math.Round((averages / t.Subjects.Count) * 2, MidpointRounding.ToEven) / 2;
            }
            else { return 0; }
        }

        public static double CalcAverage(this Table.Subject s)
        {
            if (s.Grades.Any())
            {
                double values = 0, weights = 0;
                foreach (Table.Subject.Grade g in s.Grades)
                {
                    if (s.OwnerTable.EnableWeightSystem)
                    {
                        weights += g.weight;
                    }
                    else
                    {
                        weights++;
                    }
                    // Math.Round(g.weight * 4, MidpointRounding.ToEven) / 4
                    values += g.value * g.weight;
                }
                return Math.Round((values / weights) * 2, MidpointRounding.ToEven) / 2;
            }
            else { return 0; }
        }

        public static double CalcCompensation(this Table.Subject s)
        {
            double points = 0;
            points = (4 - s.CalcAverage());
            if (points < 0) { points = (points * 2); }
            return points;
        }

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

        public static bool ClearOnSwitch;

        public static void ClearMenu()
        {
            if (ClearOnSwitch)
            {
                Console.Clear();
            }
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void ResetInput(string error = "$(default)")
        {
            if (error == "$(default)")
            {
                error = string.Format("[{0}] {1}", lang.GetString("Error"), lang.GetString("InvalidInput"));
            }
            Console.Write(error);
            new System.Threading.ManualResetEvent(false).WaitOne(150);
            ClearCurrentConsoleLine();
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            ClearCurrentConsoleLine();
        }

        public static string NewConsoleTitle;
        public static string OldConsoleTitle = Console.Title;

        public static void ExitCli()
        {
            Console.Title = OldConsoleTitle;
            Console.WriteLine("Closing...");
            ClearMenu();
        }

        private static void IsCliExitPendingHandler(Object sender, ConsoleCancelEventArgs args)
        {
            ExitCli();
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}