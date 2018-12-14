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

            // Setting the console and menu title
            NewConsoleTitle = Lang.GetString("Title");
            Console.Title = NewConsoleTitle;

            // List of options for the main menu.
            List<string> options = new List<string> { Lang.GetString("Subjects"), Lang.GetString("Overview"), Lang.GetString("Table"), Lang.GetString("Settings") };

            // Displaying the main menu title which is the same as the console title.
            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("--- {0} ---", NewConsoleTitle);
            }

            // Handling the options.
            bool HandleOption(List<string> Options, int index)
            {
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

            // Calling the menu template to display the main menu with the specified parameters.
            ListToMenu(options, HandleOption, DisplayTitle, ExitOption:Lang.GetString("Exit"));

            // Exit the app when the main menu closes.
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
                    Console.WriteLine("[{0}] {1} : {2}", Lang.GetString("Error"), System.IO.Path.GetFileName(SourceFile), Lang.GetString("TableDeniedAccess"));
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
                // Default values for new tables, pulled from the settings file.
                Name = "terminal_" + DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss"),
                MinGrade = Properties.Settings.Default.DefaultMinGrade,
                MaxGrade = Properties.Settings.Default.DefaultMaxGrade,
                UseWeightSystem = Properties.Settings.Default.DefaultUseWeightSystem
            };
            return t;
        }

        /// <summary>
        /// Displays a menu to manage tables and their respective files.
        /// </summary>
        public static void ManageTable()
        {
            // Make sure the current table is saved.
            t.Save();

            // List of options.
            List<string> options = new List<string> { Lang.GetString("TableRead"), Lang.GetString("TableWrite"), Lang.GetString("TableEdit"), Lang.GetString("TableDelete") };

            // Title.
            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableManage"), t.Name);
            }

            // Handle options.
            bool HandleOption(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("TableRead")):
                        // Call the table to choose a table to load.
                        ChooseTable();
                        break;

                    case var i when i.Equals(Lang.GetString("TableWrite")):
                        // Save the table.
                        t.Save(true);
                        new System.Threading.ManualResetEvent(false).WaitOne(20);
                        break;

                    case var i when i.Equals(Lang.GetString("TableSetDefault")):
                        // Set the current table as new default table on startup.
                        Properties.Settings.Default.SourceFile = System.IO.Path.GetFileName(SourceFile);
                        Properties.Settings.Default.Save();
                        // Display log message.
                        Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("TableSetDefaultSuccess"));
                        new System.Threading.ManualResetEvent(false).WaitOne(500);
                        break;

                    case var i when i.Equals(Lang.GetString("TableEdit")):
                        // Call the menu for editing a table.
                        ModifyTable();
                        t.Save();
                        break;

                    case var i when i.Equals(Lang.GetString("TableDelete")):
                        // Define the Yes option for the following confirmation menu.
                        Action Yes = () =>
                        {
                            t.Clear(SourceFile);
                            t = LoadTable();
                            ChooseTable(false);
                        };
                        // Call a Yes/No menu for ensuring the user wants to delete the table.
                        YesNoMenu("TableDelete", Yes, () => { });
                        break;

                    default:
                        // Reset the input.
                        ResetInput();
                        break;
                }
                return false;
            }

            // Display the menu.
            ListToMenu(options, HandleOption, DisplayTitle);

        }

        /// <summary>
        /// Displays a menu for choosing and loading a table.
        /// </summary>
        /// <param name="UserCanAbort">Wether the user can exit the menu without choosing a table or not.</param>
        public static void ChooseTable(bool UserCanAbort = true)
        {
            // Create a list for storing names of files that hold tables.
            List<string> tableFiles = GetTableFiles();

            // Get a list of strings of files that contain a table.
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
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableDeniedAccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                catch (Exception) { }

                // Sort list alphabetically.
                tables.Sort((a, b) => a.CompareTo(b));

                return tables;
            }

            // Display the title and the 0-option (creating a new table).
            void DisplayTitle(List<string> Tables)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableChoose"), Tables.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(Tables.Count).Length, ' '), Lang.GetString("TableCreate"));
            }

            // Method for displaying the options.
            void DisplayOption(List<string> TableFiles, string t, int index, int i)
            {
                // Maxlength for padding.
                int MaxLength = TableFiles.Select(x => System.IO.Path.GetFileName(x).Length).Max();
                // Name of the table. If it fails to load, display the NoData string.
                string name;
                try
                {
                    name = Table.Read(TableFiles[index]).Name;
                }
                catch (Exception)
                {
                    name = Lang.GetString("NoDataAvailable");
                }
                // Display the table as option.
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(TableFiles.Count).Length, ' '),
                    System.IO.Path.GetFileName(TableFiles[index]).PadRight(MaxLength, ' ') + " | " + name);
            }

            // handling Zero Method.
            bool ZeroOption(List<string> options)
            {
                // Call the menu for creating a new table.
                CreateTable();
                return false;
            }

            // Handling the options.
            bool HandleOption(List<string> Tables, int index)
            {
                try
                {
                    t = Table.Read(Tables[index]);
                    SourceFile = Tables[index];
                }
                catch (Exception)
                {
                    ResetInput(string.Format("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableReadError")));
                }
                return true;
            }

            List<string> UpdateObjects(List<string> tables)
            {
                return GetTableFiles();
            }

            // Display the menu.
            ListToMenu(tableFiles, HandleOption, DisplayTitle, DisplayOption, ZeroOption, UpdateObjects, UserCanAbort);

        }

        /// <summary>
        /// Displays a menu for creating a new table.
        /// </summary>
        public static void CreateTable()
        {
            // Get a new table.
            Table x = GetEmptyTable();
            // Get the name for it through user input.
            x.Name = GetTable(string.Format("--- {0} ---", Lang.GetString("TableCreate")));
            // Create a file for the table.
            // Files will be automatically named grades.xml with an increasing number in front of them.
            if (System.IO.File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "grades.xml")))
            {
                // Names start at 2.
                int i = 1;
                while (true)
                {
                    // Increase the number in front of the file's name.
                    i++;
                    // Check if there is no file with that name already.
                    if (!(System.IO.File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + string.Format("/" + i + "." + "grades.xml")))))
                    {
                        // Create it.
                        x.Write(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + string.Format("/" + i + "." + "grades.xml")));
                        break;
                    }
                }
            }
            else
            {
                // Create a file without a number.
                x.Write(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "grades.xml"));
            }

        }

        /// <summary>
        /// Displays a menu for renaming the currently loaded table.
        /// </summary>
        public static void RenameTable()
        {
            // Get the new name for the table through user input.
            t.Name = GetTable(string.Format("--- {0} : {1} ---", Lang.GetString("TableRename"), t.Name));
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
                Console.Write("{0}> ", Lang.GetString("TableName"));
                input = Console.ReadLine();

                // Trim the input.
                input.Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    // Check if the input is equal to the CreateTable option to counter sneaky users.
                    if (!input.Equals(string.Format("({0})", Lang.GetString("TableCreate")), StringComparison.InvariantCultureIgnoreCase))
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

        public static void SetTableGradeLimits(bool UseMinGrade)
        {
            string input = "";
            double value = -1;
            bool IsInputValid = false;
            double old = -1;
            while (!IsInputValid)
            {
                ClearMenu();
                string limit;
                if (UseMinGrade)
                {
                    limit = "GradeMin";
                    old = t.MinGrade;
                }
                else
                {
                    limit = "GradeMax";
                    old = t.MaxGrade;
                }

                Console.WriteLine("{0} : {1}", Lang.GetString(limit), old);

                Console.Write("\n");
                Console.Write("{0}> ", Lang.GetString(limit));
                input = Console.ReadLine();

                if (double.TryParse(input, out value))
                {
                    IsInputValid = true;
                }
                else
                {
                    ResetInput();
                }
            }

            if (UseMinGrade)
            {
                t.MinGrade = value;
            }
            else
            {
                t.MaxGrade = value;
            }

            t.Save();
        }

        public static void ModifyTable()
        {
            // Make sure the current table is saved.
            t.Save();

            // List of options.
            List<string> options = new List<string> { Lang.GetString("TableName"), Lang.GetString("GradeMin"), Lang.GetString("GradeMax"), Lang.GetString("TableUseWeight") };

            // Dictionatry mapping options to object properties.
            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { Lang.GetString("TableName"), "Name" },
                { Lang.GetString("GradeMin"), "MinGrade" },
                { Lang.GetString("GradeMax"), "MaxGrade" },
                { Lang.GetString("TableUseWeight"), "UseWeightSystem" }
            };

            // Title.
            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableEdit"), t.Name);
            }

            // Getting the maximum length of all strings.
            int MaxLength = options.Select(x => x.Length).Max();
            // Option displaying template.
            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                string display = t.GetType().GetProperty(ValueMap[o]).GetValue(t).ToString();

                if (string.IsNullOrEmpty(display)) { display = Lang.GetString("NoData"); }
                if (display == "True") { display = Lang.GetString("Yes"); }
                if (display == "False") { display = Lang.GetString("No"); }

                Console.WriteLine("[{0}] {1} | {2}", i, o.PadRight(MaxLength, ' '), display);

            }

            // Handle options.
            bool HandleOption(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("TableName")):
                        RenameTable();
                        break;

                    case var i when i.Equals(Lang.GetString("GradeMin")):
                        SetTableGradeLimits(true);
                        break;

                    case var i when i.Equals(Lang.GetString("GradeMax")):
                        SetTableGradeLimits(false);
                        break;

                    case var i when i.Equals(Lang.GetString("TableUseWeight")):
                        t.UseWeightSystem = !t.UseWeightSystem;
                        t.Save();
                        break;

                    default:
                        // Reset the input.
                        ResetInput();
                        break;
                }
                return false;
            }

            // Display the menu.
            ListToMenu(options, HandleOption, DisplayTitle, DisplayOption);
        }

        /// <summary>
        /// Displays a menu for managing subjects.
        /// </summary>
        /// <param name="s">The subject that is to be managed.</param>
        public static void ManageSubject(Table.Subject s)
        {
            List<string> options = new List<string> { Lang.GetString("Grades"), Lang.GetString("SubjectRename"), Lang.GetString("SubjectDelete") };

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

            bool HandleOption(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("Grades")):
                        ChooseGrade(s);
                        break;

                    case var i when i.Equals(Lang.GetString("SubjectRename")):
                        RenameSubject(s);
                        break;

                    case var i when i.Equals(Lang.GetString("SubjectDelete")):
                        t.RemSubject(t.Subjects.IndexOf(s));
                        t.Save();
                        return true;

                    default:
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(options, HandleOption, DisplayTitle);

        }

        /// <summary>
        /// Displays a menu for choosing a subject.
        /// </summary>
        public static void ChooseSubject()
        {

            void DisplayTitle(List<Table.Subject> Subjects)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("SubjectChoose"), Subjects.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), Lang.GetString("SubjectCreate"));
            }

            void DisplayOption(List<Table.Subject> Subjects, Table.Subject s, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(Subjects.Count).Length, ' '), s.Name);
            }

            bool ZeroOption(List<Table.Subject> Subjects)
            {
                CreateSubject();
                return false;
            }

            bool HandleOption(List<Table.Subject> Subjects, int index)
            {
                ManageSubject(t.Subjects[index]);
                return false;
            }

            List<Table.Subject> UpdateObjects(List<Table.Subject> grades)
            {
                return t.Subjects;
            }

            ListToMenu(t.Subjects, HandleOption, DisplayTitle, DisplayOption, ZeroOption, UpdateObjects);

        }

        /// <summary>
        /// Displays a menu for creating a new subject.
        /// </summary>
        public static void CreateSubject()
        {
            t.AddSubject(GetSubject(string.Format("--- {0} ---", Lang.GetString("SubjectCreate"))));
            t.Save();
        }

        /// <summary>
        /// Displays a menu for renaming an existing subject.
        /// </summary>
        /// <param name="s">The subject that is to be renamed.</param>
        public static void RenameSubject(Table.Subject s)
        {
            s.EditSubject(GetSubject(string.Format("--- {0} : {1} ---", Lang.GetString("SubjectRename"), s.Name)));
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
                Console.Write("{0}> ", Lang.GetString("SubjectName"));
                input = Console.ReadLine();

                input.Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (!input.Equals(string.Format("({0})", Lang.GetString("SubjectCreate")), StringComparison.InvariantCultureIgnoreCase))
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
            List<string> options = new List<string> { Lang.GetString("GradeEdit"), Lang.GetString("GradeDelete") };

            void DisplayTitle(List<string> Options)
            {
                if (g.OwnerSubject.OwnerTable.UseWeightSystem)
                {
                    Console.WriteLine("--- {0} : {1} | {2} ---", Lang.GetString("Grade"), g.Value, g.Weight);
                }
                else
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("Grade"), g.Value);
                }
            }

            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool HandleOption(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("GradeEdit")):
                        ModifyGrade(g);
                        break;

                    case var i when i.Equals(Lang.GetString("GradeDelete")):
                        // Remove the grade by using the OwnerSubject attribute.
                        // Effectively bypassing the need to pass the subject in which the grade is in.
                        g.OwnerSubject.RemGrade(g);
                        t.Save();
                        return true;

                    default:
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(options, HandleOption, DisplayTitle, DisplayOption);

        }

        /// <summary>
        /// Displays a menu for choosing a grade.
        /// </summary>
        /// <param name="s">The subject which grades can be chosen from.</param>
        public static void ChooseGrade(Table.Subject s)
        {
            void DisplayTitle(List<Table.Subject.Grade> Grades)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("GradeChoose"), Grades.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(Grades.Count).Length, ' '), Lang.GetString("GradeCreate"));
            }

            void DisplayOption(List<Table.Subject.Grade> Grades, Table.Subject.Grade g, int index, int i)
            {
                int MaxLength = Grades.Select(x => x.Value.ToString().Length).Max();
                if (s.OwnerTable.UseWeightSystem)
                {
                    Console.WriteLine("[{0}] {1} | {2}", Convert.ToString(i).PadLeft(Convert.ToString(Grades.Count).Length, ' '), Convert.ToString(g.Value).PadRight(MaxLength, ' '), g.Weight);
                }
                else
                {
                    Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(Grades.Count).Length, ' '), Convert.ToString(g.Value).PadRight(MaxLength, ' '));
                }
            }

            bool ZeroOption(List<Table.Subject.Grade> Grades)
            {
                CreateGrade(s);
                return false;
            }

            bool HandleOption(List<Table.Subject.Grade> Grades, int index)
            {
                ManageGrade(Grades[index]);
                return false;
            }

            List<Table.Subject.Grade> UpdateObjects(List<Table.Subject.Grade> grades)
            {
                return s.Grades;
            }

            ListToMenu(s.Grades, HandleOption, DisplayTitle, DisplayOption, ZeroOption, UpdateObjects);

        }

        /// <summary>
        /// Displays a menu for creating a new grade.
        /// </summary>
        /// <param name="s">The subject in which a grade is to be created in.</param>
        public static void CreateGrade(Table.Subject s)
        {
            // Gets the values needed to create a grade from the menu template as tuple.
            Tuple<double, double> g = GetGrade(s, string.Format("--- {0} ---", Lang.GetString("GradeCreate")));
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
            Tuple<double, double> n;
            if (g.OwnerSubject.OwnerTable.UseWeightSystem)
            {
                n = GetGrade(g.OwnerSubject, string.Format("--- {0} : {1} | {2} ---", Lang.GetString("GradeEdit"), g.Value, g.Weight));
            }
            else
            {
                n = GetGrade(g.OwnerSubject, string.Format("--- {0} : {1} ---", Lang.GetString("GradeEdit"), g.Value));
            }
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
                    if (Properties.Settings.Default.EnableGradeLimits)
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
            if (s.OwnerTable.UseWeightSystem)
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
            if (t.Subjects.Any() && t.Subjects.SelectMany(s => s.Grades).Any())
            {

                // Calculate the maximum length of any word in front of the bar diagramm.
                int MaxLength = t.Subjects.Select(x => x.Name.Length).Max();
                if (MaxLength < Lang.GetString("Overview").Length) { MaxLength = Lang.GetString("Overview").Length; }
                if (MaxLength < Lang.GetString("Total").Length) { MaxLength = Lang.GetString("Total").Length; }
                if (Properties.Settings.Default.DisplayCompensation) { if (MaxLength < Lang.GetString("Compensation").Length) { MaxLength = Lang.GetString("Compensation").Length; } }
                int BarLength;

                // Overview for the swiss grade system.
                if (t.MinGrade == 1 && t.MaxGrade == 6)
                {

                    // Display the bar diagramm meter.
                    BarLength = 12;
                    Console.WriteLine("{0} : 1 2 3 4 5 6: {1}", Lang.GetString("Overview").PadRight(MaxLength, ' '), Lang.GetString("Average"));

                    // Sort the subjects descending by their average grade.
                    // Or maybe... actually... not.
                    // t.Subjects.OrderByDescending(x => x.CalcAverage()).ToList()
                    // Print a diagramm for each subject.
                    foreach (Table.Subject s in t.Subjects)
                    {
                        if (s.Grades.Any())
                        {
                            Console.WriteLine("{0} :{1}: {2}", s.Name.PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(s.CalcAverage() * 2)).PadRight(BarLength, ' ').Truncate(BarLength), s.CalcAverage());
                        }
                        else
                        {
                            Console.WriteLine("{0} :{1}: {2}", s.Name.PadRight(MaxLength, ' '), new string(' ', BarLength), Lang.GetString("NoData"));
                        }
                    }

                    // Print total average grade.
                    Console.Write("\n");
                    Console.WriteLine("{0} :{1}: {2}", Lang.GetString("Total").PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(t.CalcAverage() * 2)).PadRight(BarLength, ' ').Truncate(BarLength), t.CalcAverage());
                    Console.Write("\n");

                    // Print compensation points, if enabled.
                    if (Properties.Settings.Default.DisplayCompensation)
                    {
                        Console.WriteLine("{0} {1}: {2}", Lang.GetString("Compensation").PadRight(MaxLength, ' '), new string(' ', BarLength + 1), t.CalcCompensation());
                    }

                }
                // Basic overview for any system. Will not work with systems that use numbers below zero.
                else
                {
                    // Outdated line to prevent overview for wrong systems.
                    // Console.WriteLine("{0} : {1}", Lang.GetString("Overview"), Lang.GetString("OverviewDataError"));

                    // Display the bar diagramm meter.
                    Console.WriteLine("{0} :1        50      100: {1}", Lang.GetString("Overview").PadRight(MaxLength, ' '), Lang.GetString("Average"));
                    BarLength = 20;

                    // Print a diagramm for each subject.
                    foreach (Table.Subject s in t.Subjects)
                    {
                        if (s.Grades.Any())
                        {
                            Console.WriteLine("{0} :{1}: {2}", s.Name.PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(BarLength / (t.MaxGrade - t.MinGrade) * s.CalcAverage())).PadRight(BarLength, ' '), s.CalcAverage());
                        }
                        else
                        {
                            Console.WriteLine("{0} :{1}: {2}", s.Name.PadRight(MaxLength, ' '), new string(' ', BarLength), Lang.GetString("NoData"));
                        }
                    }

                    // Print total average grade.
                    Console.Write("\n");
                    Console.WriteLine("{0} :{1}: {2}", Lang.GetString("Total").PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(BarLength / (t.MaxGrade - t.MinGrade) * t.CalcAverage())).PadRight(BarLength, ' '), t.CalcAverage());
                    Console.Write("\n");
                }

            }
            // If no data is available, display a message for the user.
            else
            {
                Console.WriteLine("{0} : {1}", Lang.GetString("Overview"), Lang.GetString("NoDataAvailable"));
            }

            Console.Write("\n");
            // Prompt the user to press any key as soon as he is done looking at the diagramm.
            Console.Write("{0} {1}", Lang.GetString("PressAnything"), " ");
            Console.ReadKey();
            Console.WriteLine("\n");
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
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("TableWriteSuccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                return true;
            }
            // Catch any error and write an error if details are enabled.
            catch (UnauthorizedAccessException)
            {
                if (verbose)
                {
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableDeniedAccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                return false;
            }
            catch (Exception)
            {
                if (verbose)
                {
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableWriteError"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                return false;
            }
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
        public static void ResetInput(string error = "InputInvalid")
        {
            // Display error and wait.
            Console.Write(string.Format("[{0}] {1}", Lang.GetString("Error"), Lang.GetString(error)));
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
            List<string> options = new List<string> { Lang.GetString("SettingsOptions"), Lang.GetString("Tables"), Lang.GetString("SettingsReset"), Lang.GetString("Credits"), };

            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("--- {0} : {1} ---", NewConsoleTitle, Lang.GetString("Settings"));
            }

            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool HandleOption(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("SettingsOptions")):
                        ModifySettings();
                        break;

                    case var i when i.Equals(Lang.GetString("Tables")):
                        ModifyTableDefaults();
                        break;

                    case var i when i.Equals(Lang.GetString("SettingsReset")):
                        void Yes()
                        {
                            Properties.Settings.Default.Reset();
                            try { System.IO.File.Delete(System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath); } catch { }
                            Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("SettingsResetSuccess"));
                        }
                        YesNoMenu("SettingsReset", Yes, () => { });
                        new System.Threading.ManualResetEvent(false).WaitOne(500);
                        break;

                    case var i when i.Equals(Lang.GetString("Credits")):
                        ClearMenu();
                        Console.WriteLine(Lang.GetString("CreditsApp"));
                        Console.WriteLine();
                        Console.WriteLine(Lang.GetString("CreditsIcon"));
                        Console.WriteLine();
                        Console.WriteLine(Lang.GetString("PressAnything"));
                        Console.ReadKey();
                        break;

                    default:
                        // Reset the input.
                        ResetInput();
                        break;
                }

                return false;
            }

            ListToMenu(options, HandleOption, DisplayTitle, DisplayOption);

        }

        public static void ModifySettings()
        {
            // List of options.
            List<string> options = new List<string> { Lang.GetString("TableDefault"), Lang.GetString("LanguageChoose"), Lang.GetString("SettingsClearMenus"), Lang.GetString("SettingsEnableGradeLimits"), Lang.GetString("SettingsShowCompensation"), };

            // Dictionatry mapping options to object properties.
            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { Lang.GetString("TableDefault"), "SourceFile" },
                { Lang.GetString("LanguageChoose"), "Language" },
                { Lang.GetString("SettingsClearMenus"), "ClearOnSwitch" },
                { Lang.GetString("SettingsEnableGradeLimits"), "EnableGradeLimits" },
                { Lang.GetString("SettingsShowCompensation"), "DisplayCompensation" },

            };

            // Title.
            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("--- {0} : {1} ---", NewConsoleTitle, Lang.GetString("SettingsOptions"));
            }

            // Getting the maximum length of all strings.
            int MaxLength = options.Select(x => x.Length).Max();
            // Option displaying template.
            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                string display = Properties.Settings.Default.GetType().GetProperty(ValueMap[o]).GetValue(Properties.Settings.Default).ToString();

                if (string.IsNullOrEmpty(display)) { display = Lang.GetString("NoData"); }
                if (display == "True") { display = Lang.GetString("Yes"); }
                if (display == "False") { display = Lang.GetString("No"); }

                Console.WriteLine("[{0}] {1} | {2}", i, o.PadRight(MaxLength, ' '), display);

            }

            // Handle options.
            bool HandleOption(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("TableDefault")):
                        SetDefaultTable();
                        break;

                    case var i when i.Equals(Lang.GetString("LanguageChoose")):
                        ChooseLang();
                        break;

                    case var i when i.Equals(Lang.GetString("SettingsClearMenus")):
                        Properties.Settings.Default.ClearOnSwitch = !Properties.Settings.Default.ClearOnSwitch;
                        Properties.Settings.Default.Save();
                        ClearOnSwitch = Properties.Settings.Default.ClearOnSwitch;
                        break;

                    case var i when i.Equals(Lang.GetString("SettingsShowCompensation")):
                        Properties.Settings.Default.DisplayCompensation = !Properties.Settings.Default.DisplayCompensation;
                        Properties.Settings.Default.Save();
                        break;

                    case var i when i.Equals(Lang.GetString("SettingsEnableGradeLimits")):
                        Properties.Settings.Default.EnableGradeLimits = !Properties.Settings.Default.EnableGradeLimits;
                        Properties.Settings.Default.Save();
                        break;

                    default:
                        // Reset the input.
                        ResetInput();
                        break;
                }
                return false;
            }

            // Display the menu.
            ListToMenu(options, HandleOption, DisplayTitle, DisplayOption);
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
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("LanguageChoose"), Langs.Count);
                }
                else
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("LanguageChoose"), Properties.Settings.Default.Language.Name);
                }
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(Langs.Count).Length, ' '), Lang.GetString("LanguageDefault"));
            }

            void DisplayOption(List<System.Globalization.CultureInfo> Langs, System.Globalization.CultureInfo lang, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(Langs.Count).Length, ' '), lang.Name);
            }

            bool ZeroOption(List<System.Globalization.CultureInfo> Langs)
            {
                Properties.Settings.Default.Language = System.Globalization.CultureInfo.InvariantCulture;
                Properties.Settings.Default.OverrideLanguage = false;
                Properties.Settings.Default.Save();
                Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("LanguageResetSuccess"));
                return true;
            }

            bool HandleOption(List<System.Globalization.CultureInfo> Langs, int index)
            {
                Properties.Settings.Default.Language = Langs[index];
                Properties.Settings.Default.OverrideLanguage = true;
                Properties.Settings.Default.Save();
                Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("LanguageSetSuccess"));
                return true;
            }

            List<System.Globalization.CultureInfo> UpdateObjects(List<System.Globalization.CultureInfo> Objects) { return Objects; }

            ListToMenu(langs, HandleOption, DisplayTitle, DisplayOption, ZeroOption, UpdateObjects);

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
                Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableDeniedAccess"));
                new System.Threading.ManualResetEvent(false).WaitOne(500);
            }
            catch (Exception) { }

            tableFiles.Sort((a, b) => b.CompareTo(a));


            void DisplayTitle(List<string> Tables)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableSetDefault"), Tables.Count);
            }

            void DisplayOption(List<string> TableFiles, string t, int index, int i)
            {
                int MaxLength = TableFiles.Select(x => System.IO.Path.GetFileName(x).Length).Max();
                string name;
                try
                {
                    name = Table.Read(TableFiles[index]).Name;
                }
                catch (Exception)
                {
                    name = Lang.GetString("NoData");
                }
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(TableFiles.Count).Length, ' '),
                    System.IO.Path.GetFileName(TableFiles[index]).PadRight(MaxLength, ' ') + " | " + name);
            }

            bool HandleOption(List<string> Tables, int index)
            {
                try
                {
                    Properties.Settings.Default.SourceFile = System.IO.Path.GetFileName(Tables[index]);
                    Properties.Settings.Default.Save();
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("TableSetDefaultSuccess"));
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                }
                catch (Exception)
                {
                    ResetInput(string.Format("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableReadError")));
                }
                return true;
            }

            ListToMenu(tableFiles, HandleOption, DisplayTitle, DisplayOption);
        }

        public static void ModifyTableDefaults()
        {
            // List of options.
            List<string> options = new List<string> { Lang.GetString("GradeMin"), Lang.GetString("GradeMax"), Lang.GetString("TableUseWeight") };

            // Dictionatry mapping options to object properties.
            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { Lang.GetString("GradeMin"), "DefaultMinGrade" },
                { Lang.GetString("GradeMax"), "DefaultMaxGrade" },
                { Lang.GetString("TableUseWeight"), "DefaultUseWeightSystem" },

            };

            // Title.
            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("--- {0} : {1} ---", NewConsoleTitle, Lang.GetString("Defaults"));
            }

            // Getting the maximum length of all strings.
            int MaxLength = options.Select(x => x.Length).Max();
            // Option displaying template.
            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                string display = Properties.Settings.Default.GetType().GetProperty(ValueMap[o]).GetValue(Properties.Settings.Default).ToString();

                if (string.IsNullOrEmpty(display)) { display = Lang.GetString("NoData"); }
                if (display == "True") { display = Lang.GetString("Yes"); }
                if (display == "False") { display = Lang.GetString("No"); }

                Console.WriteLine("[{0}] {1} | {2}", i, o.PadRight(MaxLength, ' '), display);

            }

            // Handle options.
            bool HandleOption(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals(Lang.GetString("DefaultGradeMin")):
                        break;

                    case var i when i.Equals(Lang.GetString("DefaultGradeMax")):
                        break;

                    case var i when i.Equals(Lang.GetString("DefaultUseWeight")):
                        Properties.Settings.Default.DefaultUseWeightSystem = !Properties.Settings.Default.DefaultUseWeightSystem;
                        Properties.Settings.Default.Save();
                        break;

                    default:
                        // Reset the input.
                        ResetInput();
                        break;
                }
                return false;
            }

            // Display the menu.
            ListToMenu(options, HandleOption, DisplayTitle, DisplayOption);
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
        /// <param name="DisplayOption">Pass a function that will display the options here.</param>
        /// <param name="HandleOption">Pass a function that handles any numerical option from the passed list here.</param>
        /// <param name="ZeroOption">Pass a function that will handle the 0-option here, if you want to use it.</param>
        /// <param name="UpdateObjects">Pass a function that will handle updating the objects list here.</param>
        /// <param name="ExitAfterChoice">Defines if the menu should exit after a choice was made.</param>
        /// <param name="ExitAfterZero">Defines if the menu should exit after the 0-option.</param>
        /// <param name="UserCanAbort">Defines if the user can exit the menu.</param>
        /// <param name="ExitOption">The string that is displayed for the option to exit the menu.</param>
        public static void ListToMenu<T>(List<T> Objects, Func<List<T>, int, bool> HandleOption, Action<List<T>> DisplayTitle = null, Action<List<T>, T, int, int> DisplayOption = null, Func<List<T>, bool> ZeroOption = null, Func<List<T>, List<T>> UpdateObjects = null, bool UserCanAbort = true, string ExitOption = null)
        {

            DisplayTitle = DisplayTitle ?? ((List<T> entries) => { });
            DisplayOption = DisplayOption ?? ((List<T> entries, T entry, int index_, int num) => { Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(entries.Count).Length, ' '), entry); });
            UpdateObjects = UpdateObjects ?? ((List<T> entries) => { return entries; });
            ZeroOption = ZeroOption ?? ((List<T> entries) => { ResetInput(); return false; });
            ExitOption = ExitOption ?? string.Empty;


            int index = -1;
            string InputString = "";
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                ClearMenu();
                int printedEntries = 0;
                Objects = UpdateObjects(Objects);
                DisplayTitle(Objects);
                if (Objects.Any())
                {
                    int i = 0;
                    foreach (T x in Objects)
                    {
                        i++;
                        if (InputString == "")
                        {
                            DisplayOption(Objects, x, i - 1, i);
                            printedEntries++;
                        }
                        else
                        {
                            if (Convert.ToString(i).StartsWith(InputString) || Convert.ToString(i) == InputString)
                            {
                                DisplayOption(Objects, x, i - 1, i);
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
                    if (ExitOption == null) { ExitOption = Lang.GetString("Back"); }
                    Console.WriteLine("[{0}] {1}", "q".PadLeft(Convert.ToString(Objects.Count).Length, ' '), ExitOption);
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
                                Console.Write("\n");
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
                            if (InputString != "")
                            {
                                index = Convert.ToInt32(InputString) - 1;
                                InputString = "";
                                IsInputValid = true;
                                if (HandleOption(Objects, index))
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
                                    if (ZeroOption(Objects))
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
                                            if (HandleOption(Objects, index))
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