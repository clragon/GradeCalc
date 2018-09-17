using System;
using System.Linq;
using System.Resources;
using System.Collections.Generic;

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

            List<string> options = new List<string> { Lang.GetString("Subjects"), Lang.GetString("Overview"), Lang.GetString("Table"), Lang.GetString("Settings") };

            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("--- {0} ---", NewConsoleTitle);
            }

            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool ZeroMethod() { ResetInput(); return false; }

            bool ChoiceMethod(List<string> Options, int index)
            {
                // switch (Options[index])
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("Subjects")):
                        // Calling the menu for choosing a subject.
                        ChooseSubject();
                        break;

                    case var i when i.Equals(Lang.GetString("Overview")):
                        // Calling the overview menu.
                        OverviewMenu();
                        break;

                    case var i when i.Equals(Lang.GetString("Table")):
                        // Calling the menu to manage tables and their files.
                        ManageTable();
                        break;

                    case var i when i.Equals(Lang.GetString("Settings")):
                        // Calling the settings menu.
                        Settings();
                        break;

                    default:
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(options, DisplayTitle, DisplayOption, ZeroMethod, ChoiceMethod, false ,false, true, "Exit");

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
            t.Save();

            List<string> options = new List<string> { Lang.GetString("ReadTable"), Lang.GetString("WriteTable"), Lang.GetString("SetDefaultTable"), Lang.GetString("RenameTable"), Lang.GetString("DeleteTable") };

            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ManageTable"), t.name);
            }

            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool ZeroMethod() { ResetInput(); return false; }

            bool ChoiceMethod(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("ReadTable")):
                        ChooseTable();
                        break;

                    case var i when i.Equals(Lang.GetString("WriteTable")):
                        t.Save(true);
                        new System.Threading.ManualResetEvent(false).WaitOne(20);
                        break;

                    case var i when i.Equals(Lang.GetString("SetDefaultTable")):
                        Properties.Settings.Default.SourceFile = System.IO.Path.GetFileName(SourceFile);
                        Properties.Settings.Default.Save();
                        Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("SetDefaultTableSuccess"));
                        new System.Threading.ManualResetEvent(false).WaitOne(500);
                        break;

                    case var i when i.Equals(Lang.GetString("RenameTable")):
                        RenameTable();
                        t.Save();
                        break;

                    case var i when i.Equals(Lang.GetString("DeleteTable")):
                        Action Yes = () =>
                        {
                            t.Clear(SourceFile);
                            t = LoadTable();
                            ChooseTable(false);

                        };

                        YesNoMenu("DeleteTable", Yes, () => { });

                        break; 

                    default:
                        // Reset the input.
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(options, DisplayTitle, DisplayOption, ZeroMethod, ChoiceMethod);

        }

        /// <summary>
        /// Displays a menu for choosing and loading a table.
        /// </summary>
        /// <param name="UserCanAbort">Wether the user can exit the menu without choosing a table or not.</param>
        public static void ChooseTable(bool UserCanAbort = true)
        {
            List<string> tableFiles = GetTableFiles();

            List<string> GetTableFiles()
            {
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

                tables.Sort((a, b) => a.CompareTo(b));

                return tables;
            }

            void DisplayTitle(List<string> Tables)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ChooseTable"), Tables.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(Tables.Count).Length, ' '), Lang.GetString("CreateTable"));
            }

            void DisplayOption(List<string> TableFiles, string t, int index, int i)
            {
                // Updating the list here.
                TableFiles = GetTableFiles();
                int MaxLength = TableFiles.Select(x => System.IO.Path.GetFileName(x).Length).Max();
                string name;
                try
                {
                    name = Table.Read(TableFiles[index]).name;
                }
                catch (Exception)
                {
                    name = Lang.GetString("NoData");
                }
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(TableFiles.Count).Length, ' '),
                    System.IO.Path.GetFileName(TableFiles[index]).PadRight(MaxLength, ' ') + " | " + name);
            }

            bool ZeroMethod()
            {
                CreateTable();
                return false;
            }

            bool ChoiceMethod(List<string> Tables, int index)
            {
                try
                {
                    t = Table.Read(Tables[index]);
                    SourceFile = Tables[index];
                }
                catch (Exception)
                {
                    ResetInput(string.Format("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("ReadTableError")));
                }
                return false;
            }

            ListToMenu(tableFiles, DisplayTitle, DisplayOption, ZeroMethod, ChoiceMethod, true, false, UserCanAbort);

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
            List<string> options = new List<string> { Lang.GetString("Grades"), Lang.GetString("RenameSubject"), Lang.GetString("DeleteSubject") };

            void DisplayTitle(List<string> Options)
            {
                if (s.Grades.Any())
                {
                    Console.WriteLine("--- {0} : {1} : {2} ---", Lang.GetString("Subject"), s.Name, s.CalcAverage());
                }
                else
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("Subject"), s.Name);
                }
            }

            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool ZeroMethod() { ResetInput(); return false; }

            bool ChoiceMethod(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("Grades")):
                        ChooseGrade(s);
                        break;

                    case var i when i.Equals(Lang.GetString("RenameSubject")):
                        RenameSubject(s);
                        break;

                    case var i when i.Equals(Lang.GetString("DeleteSubject")):
                        t.RemSubject(t.Subjects.IndexOf(s));
                        return true;

                    default:
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(options, DisplayTitle, DisplayOption, ZeroMethod, ChoiceMethod);

        }

        /// <summary>
        /// Displays a menu for choosing a subject.
        /// </summary>
        public static void ChooseSubject()
        {

            void DisplayTitle(List<Table.Subject> Subjects)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ChooseSubject"), Subjects.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), Lang.GetString("CreateSubject"));
            }

            void DisplayOption(List<Table.Subject> Subjects, Table.Subject s, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(Subjects.Count).Length, ' '), s.Name);
            }

            bool ZeroMethod() { CreateSubject(); return false; }

            bool ChoiceMethod(List<Table.Subject> Subjects, int index)
            {
                ManageSubject(t.Subjects[index]);
                return false;
            }

            ListToMenu(t.Subjects, DisplayTitle, DisplayOption, ZeroMethod, ChoiceMethod);

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
            List<string> options = new List<string> { Lang.GetString("EditGrade"), Lang.GetString("DeleteGrade") };

            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ManageTable"), t.name);
            }

            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool ZeroMethod() { ResetInput(); return false; }

            bool ChoiceMethod(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("EditGrade")):
                        ModifyGrade(g);
                        break;

                    case var i when i.Equals(Lang.GetString("DeleteGrade")):
                        // Remove the grade by using the OwnerSubject attribute.
                        // Effectively bypassing the need to pass the subject in which the grade is in.
                        g.OwnerSubject.RemGrade(g);
                        return true;

                    default:
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(options, DisplayTitle, DisplayOption, ZeroMethod, ChoiceMethod);

        }

        /// <summary>
        /// Displays a menu for choosing a grade.
        /// </summary>
        /// <param name="s">The subject which grades can be chosen from.</param>
        public static void ChooseGrade(Table.Subject s)
        {
            void DisplayTitle(List<Table.Subject.Grade> Grades)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ChooseGrade"), Grades.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(Grades.Count).Length, ' '), Lang.GetString("CreateGrade"));
            }

            void DisplayOption(List<Table.Subject.Grade> Grades, Table.Subject.Grade g, int index, int i)
            {
                int MaxLength = Grades.Select(x => x.Value.ToString().Length).Max();
                Console.WriteLine("[{0}] {1} | {2}", Convert.ToString(i).PadLeft(Convert.ToString(Grades.Count).Length, ' '), Convert.ToString(g.Value).PadRight(MaxLength, ' '), g.Weight);
            }

            bool ZeroMethod()
            {
                CreateGrade(s);
                return false;
            }

            bool ChoiceMethod(List<Table.Subject.Grade> Grades, int index)
            {
                ManageGrade(Grades[index]);
                return false;
            }

            ListToMenu(s.Grades, DisplayTitle, DisplayOption, ZeroMethod, ChoiceMethod);

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
                        if (s.OwnerTable.MaxGrade > s.OwnerTable.MinGrade)
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
                        else
                        {
                            if ((value <= s.OwnerTable.MinGrade) && (value >= s.OwnerTable.MaxGrade))
                            {
                                IsFirstInputValid = true;
                            }
                            else
                            {
                                ResetInput();
                            }
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
            if (t.Subjects.Any())
            {
                if (t.MinGrade == 1 && t.MaxGrade == 6)
                {
                    // Calculate the maximum length of any word in front of the bar diagramm.
                    int MaxLength = t.Subjects.Select(x => x.Name.Length).Max();
                    if (MaxLength < Lang.GetString("Overview").Length) { MaxLength = Lang.GetString("Overview").Length; }
                    if (MaxLength < Lang.GetString("Total").Length) { MaxLength = Lang.GetString("Total").Length; }
                    if (Properties.Settings.Default.DisplayCompensation) { if (MaxLength < Lang.GetString("Compensation").Length) { MaxLength = Lang.GetString("Compensation").Length; } }
                    // Display the bar diagramm meter.
                    int BarLength = 12;
                    Console.WriteLine("{0} : 1 2 3 4 5 6: {1}", Lang.GetString("Overview").PadRight(MaxLength, ' '), Lang.GetString("Average"));
                    // Sort the subjects descending by their average grade.
                    t.Subjects.Sort((s1, s2) =>
                    {
                        return Convert.ToInt32(s2.CalcAverage() - s1.CalcAverage());
                    });
                    // Print a diagramm for each subject.
                    foreach (Table.Subject s in t.Subjects)
                    {
                        Console.WriteLine("{0} :{1}: {2}", s.Name.PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(s.CalcAverage() * 2)).PadRight(BarLength, ' ').Truncate(BarLength), s.CalcAverage());
                    }
                    // Print total average grade.
                    Console.Write("\n");
                    Console.WriteLine("{0} :{1}: {2}", Lang.GetString("Total").PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(t.CalcAverage() * 2)).PadRight(BarLength, ' ').Truncate(BarLength), t.CalcAverage());
                    Console.Write("\n");
                    // Print compensation points, if enabled.
                    if (Properties.Settings.Default.DisplayCompensation)
                    {
                        Console.Write("{0} {1}: {2}", Lang.GetString("Compensation").PadRight(MaxLength, ' '), new string(' ', BarLength + 1), t.CalcCompensation());
                        Console.Write("\n");
                    }
                }
                else
                {
                    Console.WriteLine("{0} : {1}", Lang.GetString("Overview"), Lang.GetString("OverviewDataError"));
                }
            }
            // If no data is available, display a message for the user.
            else
            {
                Console.WriteLine("{0} : {1}", Lang.GetString("Overview"), Lang.GetString("NoData"));
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

        /// <summary>
        /// Displays a menu for adjusting settings.
        /// </summary>
        public static void Settings()
        {
            List<string> options = new List<string> { Lang.GetString("ChooseLang"), Lang.GetString("DefaultTable"), Lang.GetString("ResetSettings") };

            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("--- {0} : {1} ---", NewConsoleTitle, Lang.GetString("Settings"));
            }

            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool ZeroMethod() { ResetInput(); return false; }

            bool ChoiceMethod(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("ChooseLang")):
                        ChooseLang();
                        break;

                    case var i when i.Equals(Lang.GetString("DefaultTable")):
                        SetDefaultTable();
                        break;

                    case var i when i.Equals(Lang.GetString("ResetSettings")):
                        try { System.IO.File.Delete(System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath); }
                        catch { }
                        Properties.Settings.Default.Reset();
                        // Implement log here
                        break;

                    default:
                        // Reset the input.
                        ResetInput();
                        break;
                }

                return false;
            }

            ListToMenu(options, DisplayTitle, DisplayOption, ZeroMethod, ChoiceMethod);

        }

        /// <summary>
        /// Displays a menu for choosing a language.
        /// </summary>
        public static void ChooseLang()
        {

            List<System.Globalization.CultureInfo> langs = GetAvailableCultures(Lang);

            void DisplayTitle(List<System.Globalization.CultureInfo> Langs)
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.Language.Name))
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ChooseLang"), Langs.Count);
                }
                else
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("ChooseLang"), Properties.Settings.Default.Language.Name);
                }
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(Langs.Count).Length, ' '), Lang.GetString("LangDefault"));
            }

            void DisplayOption(List<System.Globalization.CultureInfo> Langs, System.Globalization.CultureInfo lang, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(Langs.Count).Length, ' '), lang.Name);
            }

            bool ZeroMethod()
            {
                Properties.Settings.Default.Language = System.Globalization.CultureInfo.InvariantCulture;
                Properties.Settings.Default.OverrideLanguage = false;
                Properties.Settings.Default.Save();
                // Implement log here
                return true;
            }

            bool ChoiceMethod(List<System.Globalization.CultureInfo> Langs, int index)
            {
                Properties.Settings.Default.Language = Langs[index];
                Properties.Settings.Default.OverrideLanguage = true;
                Properties.Settings.Default.Save();
                // Implement log here
                return true;
            }

            ListToMenu(langs, DisplayTitle, DisplayOption, ZeroMethod, ChoiceMethod, true, true);

        }

        /// <summary>
        /// Displays a menu for choosing the default table.
        /// </summary>
        public static void SetDefaultTable()
        {
            List<string> tableFiles = new List<string>();
            try
            {
                // Fetching all files in the app directory that have the "grades.xml" ending.
                tableFiles = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*grades.xml").ToList();
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("DeniedTableAccess"));
                new System.Threading.ManualResetEvent(false).WaitOne(500);
            }
            catch (Exception) { }

            tableFiles.Sort((a, b) => b.CompareTo(a));


            void DisplayTitle(List<string> Tables)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("SetDefaultTable"), Tables.Count);
            }

            void DisplayOption(List<string> TableFiles, string t, int index, int i)
            {
                int MaxLength = TableFiles.Select(x => System.IO.Path.GetFileName(x).Length).Max();
                string name;
                try
                {
                    name = Table.Read(TableFiles[index]).name;
                }
                catch (Exception)
                {
                    name = Lang.GetString("NoData");
                }
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(TableFiles.Count).Length, ' '), 
                    System.IO.Path.GetFileName(TableFiles[index]).PadRight(MaxLength, ' ') + " | " + name);
            }

            bool ZeroMethod() { ResetInput(); return false; }

            bool ChoiceMethod(List<string> Tables, int index)
            {
                try
                {
                    Properties.Settings.Default.SourceFile = System.IO.Path.GetFileName(Tables[index]);
                    Properties.Settings.Default.Save();
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("SetDefaultTableSuccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                catch (Exception)
                {
                    ResetInput(string.Format("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("ReadTableError")));
                }
                return false;
            }

            ListToMenu(tableFiles, DisplayTitle, DisplayOption, ZeroMethod, ChoiceMethod, true, false);
        }

        /// <summary>
        /// Return the available languages for this app.
        /// </summary>
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


        /// <summary>
        /// The template for displaying a menu.
        /// </summary>
        /// <param name="Objects">A list of objects that will be displayed as choices.</param>
        /// <param name="DisplayTitle">Pass a function that displays the title here. It should include displaying the 0-option if you want to use it.</param>
        /// <param name="DisplayOptions">Pass a function that will display the options here.</param>
        /// <param name="ZeroMethod">Pass a function that will handle the 0-option here, if you want to use it.</param>
        /// <param name="ChoiceMethod">Pass a function that handles any numerical option from the passed list here.</param>
        /// <param name="ExitAfterChoice">Defines if the menu should exit after a choice was made.</param>
        /// <param name="ExitAfterZero">Defines if the menu should exit after the 0-option.</param>
        /// <param name="UserCanAbort">Defines if the user can exit the menu.</param>
        /// <param name="BackString">The string that is displayed for the option to exit the menu.</param>
        public static void ListToMenu<T>(List<T> Objects, Action<List<T>> DisplayTitle, Action<List<T>, T, int, int> DisplayOptions, Func<bool> ZeroMethod, Func<List<T>, int, bool> ChoiceMethod, bool ExitAfterChoice = false, bool ExitAfterZero = false, bool UserCanAbort = true, string BackString = "Back")
        {
            int index = -1;
            string InputString = "";
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                int printedEntries = 0;
                DisplayTitle(Objects);
                if (Objects.Any())
                {
                    int i = 0;
                    foreach (T x in Objects)
                    {
                        i++;
                        if (InputString == "")
                        {
                            DisplayOptions(Objects, x, i - 1, i);
                            printedEntries++;
                        }
                        else
                        {
                            if (Convert.ToString(i).StartsWith(InputString) || Convert.ToString(i) == InputString)
                            {
                                DisplayOptions(Objects, x, i - 1, i);
                                printedEntries++;
                            }
                        }

                        if (Objects.Count > Console.WindowHeight - 5)
                        {
                            if (printedEntries == Console.WindowHeight - 6)
                            {
                                Console.WriteLine("[{0}]", ".".PadLeft(Convert.ToString(Objects.Count).Length, '.'));
                                break;
                            }
                        }
                        else { if (printedEntries == Console.WindowHeight - 5) { break; } }
                    }
                }

                if (UserCanAbort)
                {
                        Console.WriteLine("[{0}] {1}", "q".PadLeft(Convert.ToString(Objects.Count).Length, ' '), Lang.GetString(BackString));
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
                            if (UserCanAbort)
                            {
                                IsInputValid = true;
                                IsMenuExitPending = true;
                            }
                            else
                            {
                                Console.Write("\n");
                                ResetInput();
                            }
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
                                if (ChoiceMethod(Objects, index) || ExitAfterChoice)
                                {
                                    IsMenuExitPending = true;
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
                                    if (ZeroMethod() || ExitAfterZero)
                                    {
                                        IsMenuExitPending = true;
                                    }
                                }
                                else
                                {
                                    if (Convert.ToInt32(InputString + Convert.ToString(choice)) <= Objects.Count)
                                    {
                                        int MatchingItems = 0;
                                        InputString = InputString + Convert.ToString(choice);
                                        for (int i = 0; i < Objects.Count; i++) { if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString) { MatchingItems++; } }
                                        if ((InputString.Length == Convert.ToString(Objects.Count).Length) || (MatchingItems == 1))
                                        {
                                            index = Convert.ToInt32(InputString) - 1;
                                            InputString = "";
                                            IsInputValid = true;
                                            if (ChoiceMethod(Objects, index) || ExitAfterChoice)
                                            {
                                                IsMenuExitPending = true;
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

        public static void YesNoMenu(string title, Action Yes, Action No)
        {
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                // Ask the user for confirmation of deleting the current table.
                // This is language dependent.
                Console.Write("{0}? [{1}]> ", Lang.GetString(title), Lang.GetString("YesOrNo"));
                string Input = Console.ReadKey().KeyChar.ToString();
                new System.Threading.ManualResetEvent(false).WaitOne(20);
                Console.Write("\n");
                // Comparing the user input incase-sensitive to the current language's character for "Yes" (For example "Y").
                if (string.Equals(Input, Lang.GetString("Yes"), StringComparison.OrdinalIgnoreCase))
                {
                    IsInputValid = true;
                    Yes();

                }
                // Comparing the user input incase-sensitive to the current language's character for "No" (For example "N").
                else if (string.Equals(Input, Lang.GetString("No"), StringComparison.OrdinalIgnoreCase))
                {
                    IsInputValid = true;
                    No();
                }
                // Input seems to be invalid, resetting the field.
                else
                {
                    ResetInput();
                }
            }
        }
    }
}