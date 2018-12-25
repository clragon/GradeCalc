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
            List<string> Entries = new List<string> { Lang.GetString("Subjects"), Lang.GetString("Overview"), Lang.GetString("Table"), Lang.GetString("Settings") };

            // Displaying the main menu title which is the same as the console title.
            void DisplayTitle(List<string> entries)
            {
                Console.WriteLine("--- {0} ---", NewConsoleTitle);
            }

            // Handling the options.
            bool HandleEntries(List<string> entries, int index)
            {
                switch (entries[index])
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
            ListToMenu(Entries, HandleEntries, DisplayTitle, ExitEntry: Lang.GetString("Exit"));

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
            List<string> Entries = new List<string> { Lang.GetString("TableRead"), Lang.GetString("TableWrite"), Lang.GetString("TableEdit"), Lang.GetString("TableDelete") };

            // Title.
            void DisplayTitle(List<string> entries)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableManage"), t.Name);
            }

            // Handle options.
            bool HandleEntries(List<string> entries, int index)
            {
                switch (entries[index])
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
            ListToMenu(Entries, HandleEntries, DisplayTitle);

        }

        /// <summary>
        /// Displays a menu for choosing and loading a table.
        /// </summary>
        /// <param name="UserCanAbort">Wether the user can exit the menu without choosing a table or not.</param>
        public static void ChooseTable(bool UserCanAbort = true)
        {
            // Create a list for storing names of files that hold tables.
            List<string> Tables = GetTableFiles();

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
            void DisplayTitle(List<string> tables)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableChoose"), tables.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(tables.Count).Length, ' '), Lang.GetString("TableCreate"));
            }

            // Method for displaying the options.
            void DisplayEntry(List<string> tables, string t, int index, int i)
            {
                // Maxlength for padding.
                int MaxLength = tables.Select(x => System.IO.Path.GetFileName(x).Length).Max();
                // Name of the table. If it fails to load, display the NoData string.
                string name;
                try
                {
                    name = Table.Read(tables[index]).Name;
                }
                catch (Exception)
                {
                    name = Lang.GetString("NoDataAvailable");
                }
                // Display the table as option.
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(tables.Count).Length, ' '),
                    System.IO.Path.GetFileName(tables[index]).PadRight(MaxLength, ' ') + " | " + name);
            }

            // handling Zero Method.
            bool ZeroEntry(List<string> tables)
            {
                // Call the menu for creating a new table.
                CreateTable();
                return false;
            }

            // Handling the options.
            bool HandleEntry(List<string> tables, int index)
            {
                try
                {
                    t = Table.Read(tables[index]);
                    SourceFile = tables[index];
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
            ListToMenu(Tables, HandleEntry, DisplayTitle, DisplayEntry, ZeroEntry, UpdateObjects, UserCanAbort);

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
            List<string> Entries = new List<string> { Lang.GetString("TableName"), Lang.GetString("GradeMin"), Lang.GetString("GradeMax"), Lang.GetString("TableUseWeight") };

            // Dictionatry mapping options to object properties.
            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { Lang.GetString("TableName"), "Name" },
                { Lang.GetString("GradeMin"), "MinGrade" },
                { Lang.GetString("GradeMax"), "MaxGrade" },
                { Lang.GetString("TableUseWeight"), "UseWeightSystem" }
            };

            // Title.
            void DisplayTitle(List<string> entries)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableEdit"), t.Name);
            }

            // Getting the maximum length of all strings.
            int MaxLength = Entries.Select(x => x.Length).Max();
            // Option displaying template.
            void DisplayEntry(List<string> entries, string entry, int index, int num)
            {
                string display = t.GetType().GetProperty(ValueMap[entry]).GetValue(t).ToString();

                if (string.IsNullOrEmpty(display)) { display = Lang.GetString("NoData"); }
                if (display == "True") { display = Lang.GetString("Yes"); }
                if (display == "False") { display = Lang.GetString("No"); }

                Console.WriteLine("[{0}] {1} | {2}", num, entry.PadRight(MaxLength, ' '), display);

            }

            // Handle options.
            bool HandleEntry(List<string> entries, int index)
            {
                switch (entries[index])
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
            ListToMenu(Entries, HandleEntry, DisplayTitle, DisplayEntry);
        }

        /// <summary>
        /// Displays a menu for managing subjects.
        /// </summary>
        /// <param name="subject">The subject that is to be managed.</param>
        public static void ManageSubject(Table.Subject subject)
        {
            List<string> Entries = new List<string> { Lang.GetString("Grades"), Lang.GetString("SubjectRename"), Lang.GetString("SubjectDelete") };

            void DisplayTitle(List<string> entries)
            {
                if (subject.Grades.Any())
                {
                    Console.WriteLine("--- {0} : {1} : {2} ---", Lang.GetString("Subject"), subject.Name, subject.CalcAverage());
                }
                else
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("Subject"), subject.Name);
                }
            }

            bool HandleEntry(List<string> entries, int index)
            {
                switch (entries[index])
                {
                    case var i when i.Equals(Lang.GetString("Grades")):
                        ChooseGrade(subject);
                        break;

                    case var i when i.Equals(Lang.GetString("SubjectRename")):
                        RenameSubject(subject);
                        break;

                    case var i when i.Equals(Lang.GetString("SubjectDelete")):
                        t.RemSubject(t.Subjects.IndexOf(subject));
                        t.Save();
                        return true;

                    default:
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(Entries, HandleEntry, DisplayTitle);

        }

        /// <summary>
        /// Displays a menu for choosing a subject.
        /// </summary>
        public static void ChooseSubject()
        {

            void DisplayTitle(List<Table.Subject> subjects)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("SubjectChoose"), subjects.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), Lang.GetString("SubjectCreate"));
            }

            void DisplayEntry(List<Table.Subject> subjects, Table.Subject subject, int index, int num)
            {
                Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(subjects.Count).Length, ' '), subject.Name);
            }

            bool ZeroEntry(List<Table.Subject> subjects)
            {
                CreateSubject();
                return false;
            }

            bool HandleEntry(List<Table.Subject> subjects, int index)
            {
                ManageSubject(t.Subjects[index]);
                return false;
            }

            List<Table.Subject> UpdateEntries(List<Table.Subject> grades)
            {
                return t.Subjects;
            }

            ListToMenu(t.Subjects, HandleEntry, DisplayTitle, DisplayEntry, ZeroEntry, UpdateEntries);

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
        /// <param name="subject">The subject that is to be renamed.</param>
        public static void RenameSubject(Table.Subject subject)
        {
            subject.EditSubject(GetSubject(string.Format("--- {0} : {1} ---", Lang.GetString("SubjectRename"), subject.Name)));
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
        /// <param name="grade">The grade that is to be managed.</param>
        public static void ManageGrade(Table.Subject.Grade grade)
        {
            List<string> Entries = new List<string> { Lang.GetString("GradeEdit"), Lang.GetString("GradeDelete") };

            void DisplayTitle(List<string> entries)
            {
                if (grade.OwnerSubject.OwnerTable.UseWeightSystem)
                {
                    Console.WriteLine("--- {0} : {1} | {2} ---", Lang.GetString("Grade"), grade.Value, grade.Weight);
                }
                else
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("Grade"), grade.Value);
                }
            }

            void DisplayEntry(List<string> entries, string entry, int index, int num)
            {
                Console.WriteLine("[{0}] {1}", num, entry);
            }

            bool HandleEntry(List<string> entries, int index)
            {
                switch (entries[index])
                {
                    case var i when i.Equals(Lang.GetString("GradeEdit")):
                        ModifyGrade(grade);
                        break;

                    case var i when i.Equals(Lang.GetString("GradeDelete")):
                        // Remove the grade by using the OwnerSubject attribute.
                        // Effectively bypassing the need to pass the subject in which the grade is in.
                        grade.OwnerSubject.RemGrade(grade);
                        t.Save();
                        return true;

                    default:
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(Entries, HandleEntry, DisplayTitle, DisplayEntry);

        }

        /// <summary>
        /// Displays a menu for choosing a grade.
        /// </summary>
        /// <param name="subject">The subject which grades can be chosen from.</param>
        public static void ChooseGrade(Table.Subject subject)
        {
            void DisplayTitle(List<Table.Subject.Grade> Grades)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("GradeChoose"), Grades.Count);
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(Grades.Count).Length, ' '), Lang.GetString("GradeCreate"));
            }

            void DisplayEntry(List<Table.Subject.Grade> grades, Table.Subject.Grade grade, int index, int num)
            {
                int MaxLength = grades.Select(x => x.Value.ToString().Length).Max();
                if (subject.OwnerTable.UseWeightSystem)
                {
                    Console.WriteLine("[{0}] {1} | {2}", Convert.ToString(num).PadLeft(Convert.ToString(grades.Count).Length, ' '), Convert.ToString(grade.Value).PadRight(MaxLength, ' '), grade.Weight);
                }
                else
                {
                    Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(grades.Count).Length, ' '), Convert.ToString(grade.Value).PadRight(MaxLength, ' '));
                }
            }

            bool ZeroEntry(List<Table.Subject.Grade> grades)
            {
                CreateGrade(subject);
                return false;
            }

            bool HandleEntry(List<Table.Subject.Grade> grades, int index)
            {
                ManageGrade(grades[index]);
                return false;
            }

            List<Table.Subject.Grade> UpdateEntries(List<Table.Subject.Grade> grades)
            {
                return subject.Grades;
            }

            ListToMenu(subject.Grades, HandleEntry, DisplayTitle, DisplayEntry, ZeroEntry, UpdateEntries);

        }

        /// <summary>
        /// Displays a menu for creating a new grade.
        /// </summary>
        /// <param name="subject">The subject in which a grade is to be created in.</param>
        public static void CreateGrade(Table.Subject subject)
        {
            // Gets the values needed to create a grade from the menu template as tuple.
            Tuple<double, double> g = GetGrade(subject, string.Format("--- {0} ---", Lang.GetString("GradeCreate")));
            // Adds a new grade with the received values.
            subject.AddGrade(g.Item1, g.Item2);
            t.Save();

        }

        /// <summary>
        /// Displays a menu for editing an existing grade.
        /// </summary>
        /// <param name="grade">The grade which is to be renamed.</param>
        public static void ModifyGrade(Table.Subject.Grade grade)
        {
            // Gets the values needed to edit a grade from the menu template as tuple.
            Tuple<double, double> n;
            if (grade.OwnerSubject.OwnerTable.UseWeightSystem)
            {
                n = GetGrade(grade.OwnerSubject, string.Format("--- {0} : {1} | {2} ---", Lang.GetString("GradeEdit"), grade.Value, grade.Weight));
            }
            else
            {
                n = GetGrade(grade.OwnerSubject, string.Format("--- {0} : {1} ---", Lang.GetString("GradeEdit"), grade.Value));
            }
            // Edits the grade with the received values.
            grade.EditGrade(n.Item1, n.Item2);
            t.Save();

        }

        /// <summary>
        /// Basic underlying menu scheme for creating or editing a grade.
        /// </summary>
        /// <param name="title">Title of the menu. Usually create or edit.</param>
        /// <param name="subject">The subject in which grades are to be edited in.</param>
        public static Tuple<double, double> GetGrade(Table.Subject subject, string title)
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
                        if (subject.OwnerTable.MaxGrade > subject.OwnerTable.MinGrade)
                        {
                            // Check if the grade is within the table limits.
                            if ((value >= subject.OwnerTable.MinGrade) && (value <= subject.OwnerTable.MaxGrade))
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
                            if ((value <= subject.OwnerTable.MinGrade) && (value >= subject.OwnerTable.MaxGrade))
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
            if (subject.OwnerTable.UseWeightSystem)
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

                // Prevents overview for wrong systems.
                if (Properties.Settings.Default.DefaultMinGrade < 0 || Properties.Settings.Default.DefaultMaxGrade < 0)
                {
                    Console.WriteLine("{0} : {1}", Lang.GetString("Overview"), Lang.GetString("OverviewDataError"));
                    return;
                }

                // Calculate the maximum length of any word in front of the bar diagramm.
                int MaxLength = t.Subjects.Select(x => x.Name.Length).Max();
                if (MaxLength < Lang.GetString("Overview").Length) { MaxLength = Lang.GetString("Overview").Length; }
                if (MaxLength < Lang.GetString("Total").Length) { MaxLength = Lang.GetString("Total").Length; }
                if (Properties.Settings.Default.DisplayCompensation) { if (MaxLength < Lang.GetString("Compensation").Length) { MaxLength = Lang.GetString("Compensation").Length; } }
                int BarLength;
                bool isSwiss;

                // Overview for the swiss grade system.
                if (t.MinGrade == 1 && t.MaxGrade == 6)
                {
                    isSwiss = true;
                    BarLength = 12;
                    Console.WriteLine("{0} : 1 2 3 4 5 6: {1}", Lang.GetString("Overview").PadRight(MaxLength, ' '), Lang.GetString("Average"));
                }
                // Basic overview for any system. Will not work with systems that use numbers below zero.
                else
                {
                    isSwiss = false;
                    BarLength = 20;
                    Console.WriteLine("{0} :1        50      100: {1}", Lang.GetString("Overview").PadRight(MaxLength, ' '), Lang.GetString("Average"));
                }

                foreach (Table.Subject s in Properties.Settings.Default.SortOverview ? t.Subjects.OrderByDescending(x => x.CalcAverage()).ToList() : t.Subjects)
                {
                    if (s.Grades.Any())
                    {
                        if (isSwiss)
                        {
                            Console.WriteLine("{0} :{1}: {2}", s.Name.PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(s.CalcAverage() * 2)).PadRight(BarLength, ' ').Truncate(BarLength), s.CalcAverage());
                        }
                        else
                        {
                            Console.WriteLine("{0} :{1}: {2}", s.Name.PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(BarLength / (t.MaxGrade - t.MinGrade) * s.CalcAverage())).PadRight(BarLength, ' '), s.CalcAverage());
                        }
                    }
                    else
                    {
                        Console.WriteLine("{0} :{1}: {2}", s.Name.PadRight(MaxLength, ' '), new string(' ', BarLength), Lang.GetString("NoData"));
                    }
                }

                // Print total average grade.
                Console.Write("\n");
                if (isSwiss)
                {
                    Console.WriteLine("{0} :{1}: {2}", Lang.GetString("Total").PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(t.CalcAverage() * 2)).PadRight(BarLength, ' ').Truncate(BarLength), t.CalcAverage());
                }
                else
                {
                    Console.WriteLine("{0} :{1}: {2}", Lang.GetString("Total").PadRight(MaxLength, ' '), new string('=', Convert.ToInt32(BarLength / (t.MaxGrade - t.MinGrade) * t.CalcAverage())).PadRight(BarLength, ' '), t.CalcAverage());
                }
                Console.Write("\n");

                // Print compensation points, if enabled.
                if (Properties.Settings.Default.DisplayCompensation && isSwiss)
                {
                    Console.WriteLine("{0} {1}: {2}", Lang.GetString("Compensation").PadRight(MaxLength, ' '), new string(' ', BarLength + 1), t.CalcCompensation());
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
            List<string> Entries = new List<string> { Lang.GetString("SettingsOptions"), Lang.GetString("Tables"), Lang.GetString("SettingsReset"), Lang.GetString("Credits"), };

            void DisplayTitle(List<string> entries)
            {
                Console.WriteLine("--- {0} : {1} ---", NewConsoleTitle, Lang.GetString("Settings"));
            }

            void DisplayEntry(List<string> entries, string entry, int index, int num)
            {
                Console.WriteLine("[{0}] {1}", num, entry);
            }

            bool HandleEntry(List<string> entries, int index)
            {
                switch (entries[index])
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

            ListToMenu(Entries, HandleEntry, DisplayTitle, DisplayEntry);

        }

        public static void ModifySettings()
        {
            // List of options.
            List<string> Entries = new List<string> { Lang.GetString("TableDefault"), Lang.GetString("LanguageChoose"), Lang.GetString("SettingsClearMenus"), Lang.GetString("SettingsEnableGradeLimits"), Lang.GetString("SettingsShowCompensation"), Lang.GetString("OverviewSortByHighest") };

            // Dictionatry mapping options to object properties.
            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { Lang.GetString("TableDefault"), "SourceFile" },
                { Lang.GetString("LanguageChoose"), "Language" },
                { Lang.GetString("SettingsClearMenus"), "ClearOnSwitch" },
                { Lang.GetString("SettingsEnableGradeLimits"), "EnableGradeLimits" },
                { Lang.GetString("SettingsShowCompensation"), "DisplayCompensation" },
                { Lang.GetString("OverviewSortByHighest"), "SortOverview" },

            };

            // Title.
            void DisplayTitle(List<string> entries)
            {
                Console.WriteLine("--- {0} : {1} ---", NewConsoleTitle, Lang.GetString("SettingsOptions"));
            }

            // Getting the maximum length of all strings.
            int MaxLength = Entries.Select(x => x.Length).Max();
            // Option displaying template.
            void DisplayEntry(List<string> entries, string entry, int index, int num)
            {
                string display = Properties.Settings.Default.GetType().GetProperty(ValueMap[entry]).GetValue(Properties.Settings.Default).ToString();

                if (string.IsNullOrEmpty(display)) { display = Lang.GetString("NoData"); }
                if (display == "True") { display = Lang.GetString("Yes"); }
                if (display == "False") { display = Lang.GetString("No"); }

                Console.WriteLine("[{0}] {1} | {2}", num, entry.PadRight(MaxLength, ' '), display);

            }

            // Handle options.
            bool HandleEntry(List<string> entries, int index)
            {
                switch (entries[index])
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

                    case var i when i.Equals(Lang.GetString("OverviewSortByHighest")):
                        Properties.Settings.Default.SortOverview = !Properties.Settings.Default.SortOverview;
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
            ListToMenu(Entries, HandleEntry, DisplayTitle, DisplayEntry);
        }

        /// <summary>
        /// Displays a menu for choosing a language.
        /// </summary>
        public static void ChooseLang()
        {

            List<System.Globalization.CultureInfo> Langs = GetAvailableCultures(Lang);

            void DisplayTitle(List<System.Globalization.CultureInfo> langs)
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.Language.Name))
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("LanguageChoose"), langs.Count);
                }
                else
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("LanguageChoose"), Properties.Settings.Default.Language.Name);
                }
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(langs.Count).Length, ' '), Lang.GetString("LanguageDefault"));
            }

            void DisplayEntry(List<System.Globalization.CultureInfo> langs, System.Globalization.CultureInfo lang, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(langs.Count).Length, ' '), lang.Name);
            }

            bool ZeroEntry(List<System.Globalization.CultureInfo> langs)
            {
                Properties.Settings.Default.Language = System.Globalization.CultureInfo.InvariantCulture;
                Properties.Settings.Default.OverrideLanguage = false;
                Properties.Settings.Default.Save();
                Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("LanguageResetSuccess"));
                return true;
            }

            bool HandleEntry(List<System.Globalization.CultureInfo> langs, int index)
            {
                Properties.Settings.Default.Language = langs[index];
                Properties.Settings.Default.OverrideLanguage = true;
                Properties.Settings.Default.Save();
                Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("LanguageSetSuccess"));
                return true;
            }

            List<System.Globalization.CultureInfo> UpdateObjects(List<System.Globalization.CultureInfo> Objects) { return Objects; }

            ListToMenu(Langs, HandleEntry, DisplayTitle, DisplayEntry, ZeroEntry, UpdateObjects);

        }

        /// <summary>
        /// Displays a menu for choosing the default table.
        /// </summary>
        public static void SetDefaultTable()
        {
            List<string> TableFiles = new List<string>();
            try
            {
                // Fetching all files in the app directory that have the "grades.xml" ending.
                TableFiles = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*grades.xml").ToList();
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableDeniedAccess"));
                new System.Threading.ManualResetEvent(false).WaitOne(500);
            }
            catch (Exception) { }

            TableFiles.Sort((a, b) => b.CompareTo(a));


            void DisplayTitle(List<string> Tables)
            {
                Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableSetDefault"), Tables.Count);
            }

            void DisplayEntry(List<string> tableFiles, string t, int index, int i)
            {
                int MaxLength = tableFiles.Select(x => System.IO.Path.GetFileName(x).Length).Max();
                string name;
                try
                {
                    name = Table.Read(tableFiles[index]).Name;
                }
                catch (Exception)
                {
                    name = Lang.GetString("NoData");
                }
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(tableFiles.Count).Length, ' '),
                    System.IO.Path.GetFileName(tableFiles[index]).PadRight(MaxLength, ' ') + " | " + name);
            }

            bool HandleEntry(List<string> tableFiles, int index)
            {
                try
                {
                    Properties.Settings.Default.SourceFile = System.IO.Path.GetFileName(tableFiles[index]);
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

            ListToMenu(TableFiles, HandleEntry, DisplayTitle, DisplayEntry);

        }

        public static void ModifyTableDefaults()
        {
            // List of options.
            List<string> Entries = new List<string> { Lang.GetString("GradeMin"), Lang.GetString("GradeMax"), Lang.GetString("TableUseWeight") };

            // Dictionatry mapping options to object properties.
            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { Lang.GetString("GradeMin"), "DefaultMinGrade" },
                { Lang.GetString("GradeMax"), "DefaultMaxGrade" },
                { Lang.GetString("TableUseWeight"), "DefaultUseWeightSystem" },

            };

            // Title.
            void DisplayTitle(List<string> entries)
            {
                Console.WriteLine("--- {0} : {1} ---", NewConsoleTitle, Lang.GetString("Defaults"));
            }

            // Getting the maximum length of all strings.
            int MaxLength = Entries.Select(x => x.Length).Max();
            // Option displaying template.
            void DisplayEntry(List<string> entries, string entry, int index, int num)
            {
                string display = Properties.Settings.Default.GetType().GetProperty(ValueMap[entry]).GetValue(Properties.Settings.Default).ToString();

                if (string.IsNullOrEmpty(display)) { display = Lang.GetString("NoData"); }
                if (display == "True") { display = Lang.GetString("Yes"); }
                if (display == "False") { display = Lang.GetString("No"); }

                Console.WriteLine("[{0}] {1} | {2}", num, entry.PadRight(MaxLength, ' '), display);

            }

            // Handle options.
            bool HandleEntry(List<string> entries, int index)
            {
                switch (entries[index])
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
            ListToMenu(Entries, HandleEntry, DisplayTitle, DisplayEntry);
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
        /// A function that displays a list as an enumerated menu on the Cli. Items can be chosen and will be processed by passed functions.
        /// <para>Rest in peace, Cloe.</para>
        /// </summary>
        /// <param name="Entries">A list of objects that will be displayed as choices.</param>
        /// <param name="DisplayTitle">The function that displays the title. It should include displaying the 0th entry if you want to use it.</param>
        /// <param name="DisplayEntry">The function that displays an entry. The default function can display strings, for any other objects you will have to pass a custom one.</param>
        /// <param name="HandleEntry">The function that handles the chosen entry.</param>
        /// <param name="ZeroEntry">The 0th entry. It is different from the passed list and can be used for example to create new entries.</param>
        /// <param name="RefreshEntries">Pass a function that will handle updating the list of objects here.</param>
        /// <param name="UserCanAbort">Defines if the user can exit the menu.</param>
        /// <param name="ExitEntry">The string that is displayed for the entry that closes the menu.</param>
        public static void ListToMenu<T>(List<T> Entries, Func<List<T>, int, bool> HandleEntry, Action<List<T>> DisplayTitle = null, Action<List<T>, T, int, int> DisplayEntry = null, Func<List<T>, bool> ZeroEntry = null, Func<List<T>, List<T>> RefreshEntries = null, bool UserCanAbort = true, string ExitEntry = null)
        {

            DisplayTitle = DisplayTitle ?? ((List<T> entries) => { });
            DisplayEntry = DisplayEntry ?? ((List<T> entries, T entry, int index_, int num) => { Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(entries.Count).Length, ' '), entry); });
            RefreshEntries = RefreshEntries ?? ((List<T> entries) => { return entries; });
            ZeroEntry = ZeroEntry ?? ((List<T> entries) => { ResetInput(); return false; });
            ExitEntry = ExitEntry ?? Lang.GetString("Back");

            char ExitKey = 'q';
            string Prompt = Lang.GetString("Choose");


            string readInput = string.Empty;
            bool MenuExitIsPending = false;
            while (!MenuExitIsPending)
            {
                ClearMenu();
                int printedEntries = 0;
                Entries = RefreshEntries(Entries);
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

                if (UserCanAbort)
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
                            if (UserCanAbort)
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
                            }
                            InputIsValid = true;
                            break;

                        case var key when int.TryParse(key.KeyChar.ToString(), out choiceNum):
                            Console.WriteLine();
                            if (string.IsNullOrEmpty(readInput) && choiceNum.Equals(0))
                            {
                                InputIsValid = true;
                                if (ZeroEntry(Entries))
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