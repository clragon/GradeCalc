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
            // Load app settings and set language.
            settings.Reload();
            if (settings.OverrideLanguage)
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = settings.Language;
            }

            // Catching CTRL+C event to reset the console title on interruption.
            Console.CancelKeyPress += new ConsoleCancelEventHandler(IsCliExitPendingHandler);

            // Setting the console and menu title for the runtime of the application.
            NewConsoleTitle = Lang.GetString("Title");
            Console.Title = NewConsoleTitle;

            // menu routing options.
            Dictionary<string, Action> options = new Dictionary<string, Action>()
            {
                { Lang.GetString("Subjects"), ChooseSubject },
                { Lang.GetString("Overview"), OverviewMenu },
                { Lang.GetString("Table"), ManageTable },
                { Lang.GetString("Settings"), Settings },
            };


            // Displaying the main menu.
            new ListMenu<string>(options.Keys.ToList())
            {
                // Displaying the main menu title.
                DisplayTitle = (entries) => 
                {
                    Console.WriteLine("--- {0} ---", Lang.GetString("Title"));
                },

                HandleEntry = (entries, index) => 
                {
                    // Handling the options.
                    if (options.ContainsKey(entries[index]))
                    {
                        options[entries[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },

                ExitEntry = Lang.GetString("Exit"),

            }.Show();


            // Exit the app when the main menu closes.
            ExitCli();
        }

        // Deprecated. Visual Studio is now providing the ResourceManager. Kept here for future reference.
        // public static ResourceManager Lang = new ResourceManager("language", typeof(Cli).Assembly);

        /// <summary>
        /// The resource manager provides (almost) all strings for the application so they can adapt to the device language.
        /// </summary>
        public static ResourceManager Lang = language.ResourceManager;

        /// <summary>
        /// The settings manager as short variable.
        /// </summary>
        private static readonly Properties.Settings settings = Properties.Settings.Default;

        /// <summary>
        /// The file in which the currently open table resides in.
        /// </summary>
        public static string SourceFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "/" + settings.SourceFile);

        /// <summary>
        /// The currently open Table as Table object.
        /// </summary>
        public static Table t = LoadTable();

        /// <summary>
        /// Loads the Table found in the current SourceFile.
        /// Will create a new one if there is an issue with reaching the SourceFile.
        /// </summary>
        public static Table LoadTable()
        {
            // Checks if the SourceFile exists.
            if (System.IO.File.Exists(SourceFile))
            {
                // Tries to load the Table from it.
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
                        // Delete the SourceFile to clear it if it is corrupted.
                        new Table().Clear(SourceFile);
                    }
                    catch (Exception)
                    {
                        // We do not have permissions to delete the SourceFile. Give up and display an error.
                        Console.WriteLine("[{0}] {1} : {2}", Lang.GetString("Error"), System.IO.Path.GetFileName(SourceFile), Lang.GetString("TableDeniedAccess"));
                        Console.WriteLine(Lang.GetString("PressAnything"));
                        Console.ReadKey();
                        // Return an empty Table to ensure an uninterrupted workflow of the application.
                        // This might result in the failure of being able to save the Table.
                        // Consider closing the app at this point, or making a new SourceFile.
                        return GetEmptyTable();
                    }
                    // Return a new empty Table since loading the existing one failed.
                    return GetEmptyTable();
                }
            }
            else
            {
                // SourceFile doesnt exist, return new empty Table.
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
                // Set default values for the Table. These are provided by the settings of the app.
                Name = "terminal_" + DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss"),
                MinGrade = settings.DefaultMinGrade,
                MaxGrade = settings.DefaultMaxGrade,
                UseWeight = settings.DefaultUseWeight
            };
            return t;
        }

        /// <summary>
        /// Displays a menu to manage tables and their respective files.
        /// </summary>
        public static void ManageTable()
        {
            // Make sure the current table is saved.
            // Might remove this in the future, since Tables are always saved after any changes.
            t.Save();

            // List of options.
            Dictionary<string, Action> options = new Dictionary<string, Action>()
            {
                { Lang.GetString("TableRead"), () => { ChooseTable(); } },
                { Lang.GetString("TableWrite"), () => 
                    { 
                    // Saving the table.
                    // This option is basically just placebo.
                    // It does save, but Tables are saved always anyways.
                    t.Save(true);
                    Wait(20);
                    }
                },
                { Lang.GetString("TableSetDefault"), () => 
                    {
                    // Set the current table as new default table on startup.
                    settings.SourceFile = System.IO.Path.GetFileName(SourceFile);
                    settings.Save();
                    // Signaling success of to the user.
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("TableSetDefaultSuccess"));
                    Wait(500);
                    }
                },
                { Lang.GetString("TableEdit"), () => 
                    { 
                        // Calling the menu for editing a table.
                        ModifyTable();
                        t.Save();
                    } 
                },
                { Lang.GetString("TableDelete"), () =>
                    {
                        // Delete the Table if the user confirms the prompt.
                        void Yes ()                         {
                            t.Clear(SourceFile);
                            t = LoadTable();
                            ChooseTable(false);
                        }
                        // Call a Yes/No menu for ensuring the user wants to delete the table.
                        YesNoMenu("TableDelete", Yes, () => { });
                    }  
                },
            };

            // Display the menu.
            new ListMenu<string>(options.Keys.ToList())
            {

                DisplayTitle = (entries) => 
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableManage"), t.Name);
                },

                HandleEntry = (entries, index) =>
                {
                    if (options.ContainsKey(entries[index]))
                    {
                        options[entries[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },

            }.Show();

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
                    // Fetching all files in the app directory that match the pattern.
                    tables = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "grades*.json").ToList();
                }
                catch (Exception)
                {
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableDeniedAccess"));
                    Wait(500);
                }

                // Sort list alphabetically.
                tables.Sort((a, b) => a.CompareTo(b));

                return tables;
            }

            // Display the menu.
            new ListMenu<string>(Tables)
            {
                DisplayTitle = (tables) => 
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableChoose"), tables.Count);
                    Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(tables.Count).Length, ' '), Lang.GetString("TableCreate"));
                },

                DisplayEntry = (tables, table, index, num) => 
                {
                    // Maxlength for padding.
                    int MaxLength = tables.Select(x => System.IO.Path.GetFileName(x).Length).Max();
                    // Name of the table. If it fails to load, display the NoData string.
                    string name;
                    try
                    {
                        name = Table.Read(tables[index]).Name;
                        settings.SourceFile = System.IO.Path.GetFileName(tables[index]);
                        settings.Save();
                    }
                    catch (Exception)
                    {
                        name = Lang.GetString("NoDataAvailable");
                    }
                    // Display the table as option.
                    Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(tables.Count).Length, ' '),
                        System.IO.Path.GetFileName(tables[index]).PadRight(MaxLength, ' ') + " | " + name);
                },

                HandleEntry = (tables, index) => 
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
                },

                ZeroEntry = (tables) => 
                {
                    // Call the menu for creating a new table.
                    CreateTable();
                    return false;
                },

                RefreshEntries = (tables) => 
                {
                    // Update the list of Tables by checking for new files.
                    return GetTableFiles();
                },

                UserCanExit = UserCanAbort,

            }.Show();

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
            // Files will be automatically named grades.json with an increasing number in front of them.
            if (System.IO.File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "grades.json")))
            {
                // Names start at 2.
                int i = 1;
                while (true)
                {
                    // Increase the number at the end of the file's name.
                    i++;
                    // Check if that file name is available.
                    if (!(System.IO.File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + string.Format("/grades_{0}.json", i)))))
                    {
                        // Create the new file.
                        x.Write(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + string.Format("/grades_{0}.json", i)));
                        break;
                    }
                }
            }
            else
            {
                // Instead of creating a file with th number 1, create a file without number. (Like with Windows copies)
                x.Write(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "grades.json"));
            }

        }

        /// <summary>
        /// Displays a menu for renaming the currently loaded table.
        /// </summary>
        public static void RenameTable()
        {
            // Get the new name for the table through user input.
            // Pass the current name in the title.
            t.Name = GetTable(string.Format("--- {0} : {1} ---", Lang.GetString("TableRename"), t.Name));
            // Save the table to it's file.
            t.Save();
        }

        /// <summary>
        /// Unified method to create or edit the name of a Table.
        /// The menu returns the new name.
        /// </summary>
        /// <param name="title">Title of the menu. Usually create or rename.</param>  
        public static string GetTable(string title)
        {
            string input = "";
            bool IsInputValid = false;
            while (!IsInputValid)
            {

                Console.Clear();
                Console.WriteLine(title);
                Console.Write("\n");
                Console.Write("{0}> ", Lang.GetString("TableName"));
                input = Console.ReadLine();

                // Trim whitespace from the input.
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

        /// <summary>
        /// A menu to set the grade limits of the currently loaded table.
        /// </summary>
        /// <param name="UseMinGrade">Indicates if the minimum grade or the maximum grade is to be changed.</param>
        /// <param name="old">The old value.</param>
        public static double GetTableGradeLimits(bool UseMinGrade, double old)
        {
            string input;
            double value = -1;
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                Console.Clear();
                string limit = UseMinGrade ? "GradeMin" : "GradeMax";

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

            return value;
        }

        /// <summary>
        /// A menu to edit the currently loaded Table.
        /// </summary>
        public static void ModifyTable()
        {
            // Make sure the current table is saved.
            t.Save();

            // List of options.
            Dictionary<string, Action> options = new Dictionary<string, Action>()
            {
                { Lang.GetString("TableName"), RenameTable },
                { Lang.GetString("GradeMin"), () => { t.MinGrade = GetTableGradeLimits(true, t.MinGrade); t.Save(); } },
                { Lang.GetString("GradeMax"), () => { t.MaxGrade = GetTableGradeLimits(false, t.MaxGrade); t.Save(); } },
                { Lang.GetString("TableUseWeight"), () => { t.UseWeight = !t.UseWeight; t.Save(); } },
            };

            // List of settig properties
            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { Lang.GetString("TableName"), "Name" },
                { Lang.GetString("GradeMin"), "MinGrade" },
                { Lang.GetString("GradeMax"), "MaxGrade" },
                { Lang.GetString("TableUseWeight"), "UseWeight" },
            };

            // Getting the maximum length of all strings.
            int MaxLength = options.Keys.ToList().Select(x => x.Length).Max();

            new ListMenu<string>(options.Keys.ToList())
            {
                DisplayTitle = (entries) => 
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableEdit"), t.Name);
                },

                DisplayEntry = (entries, entry, index, num) => 
                {
                    string display = t.GetType().GetProperty(ValueMap[entry]).GetValue(t).ToString();

                    if (string.IsNullOrEmpty(display)) { display = Lang.GetString("NoData"); }
                    if (display == "True") { display = Lang.GetString("Yes"); }
                    if (display == "False") { display = Lang.GetString("No"); }

                    Console.WriteLine("[{0}] {1} | {2}", num, entry.PadRight(MaxLength, ' '), display);
                },

                HandleEntry = (entries, index) => 
                {
                    if (options.ContainsKey(entries[index]))
                    {
                        options[entries[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },

            }.Show();
        }

        /// <summary>
        /// Displays a menu for managing subjects.
        /// </summary>
        /// <param name="subject">The subject that is to be managed.</param>
        public static void ManageSubject(Table.Subject subject)
        {

            Dictionary<string, Func<bool>> options = new Dictionary<string, Func<bool>>()
            {
                { Lang.GetString("Grades"), () => { ChooseGrade(subject); return false;  } },
                { Lang.GetString("SubjectRename"), () => { RenameSubject(subject); return false; } },
                { Lang.GetString("SubjectDelete"), () => { t.RemSubject(t.Subjects.IndexOf(subject)); t.Save(); return true; } },
            };

            new ListMenu<string>(options.Keys.ToList())
            {
                DisplayTitle = (entries) => 
                {
                    if (subject.Grades.Any())
                    {
                        Console.WriteLine("--- {0} : {1} : {2} ---", Lang.GetString("Subject"), subject.Name, subject.CalcAverage());
                    }
                    else
                    {
                        Console.WriteLine("--- {0} : {1} ---", Lang.GetString("Subject"), subject.Name);
                    }
                },

                HandleEntry = (entries, index) => 
                {
                    if (options.ContainsKey(entries[index]))
                    {
                        return options[entries[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },

            }.Show();

        }

        /// <summary>
        /// Displays a menu for choosing a subject.
        /// </summary>
        public static void ChooseSubject()
        {
            new ListMenu<Table.Subject>(t.Subjects)
            {
                DisplayTitle = (subjects) =>
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("SubjectChoose"), subjects.Count);
                    Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(t.Subjects.Count).Length, ' '), Lang.GetString("SubjectCreate"));
                },

                DisplayEntry = (subjects, subject, index, num) => 
                {
                    Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(subjects.Count).Length, ' '), subject.Name);
                },

                HandleEntry = (subjects, index) => 
                {
                    ManageSubject(t.Subjects[index]);
                    return false;
                },

                ZeroEntry = (subjects) => 
                {
                    CreateSubject();
                    return false;
                },

            }.Show();
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

        /// <summary>
        /// unified menu to get the name for a new or for the edit of a subject.
        /// </summary>
        /// <param name="title">The tilte of the menu. Usually, create or edit.</param>
        /// <returns></returns>
        public static string GetSubject(string title)
        {
            string input = "";
            bool IsInputValid = false;
            while (!IsInputValid)
            {

                Console.Clear();
                Console.WriteLine(title);
                Console.Write("\n");
                Console.Write("{0}> ", Lang.GetString("SubjectName"));
                input = Console.ReadLine();

                input.Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    // Counteracting sneaky users.
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

            Dictionary<string, Func<bool>> options = new Dictionary<string, Func<bool>>()
            {
                { Lang.GetString("GradeEdit"), () => { ModifyGrade(grade); return false; } },
                { Lang.GetString("GradeDelete"), () => 
                    {
                        // Remove the grade by using the OwnerSubject attribute.
                        // Effectively bypassing the need to pass the subject in which the grade is in.
                        grade.OwnerSubject.RemGrade(grade);
                        t.Save();
                        return true;
                    } 
                },
            };

            new ListMenu<string>(options.Keys.ToList())
            {
                DisplayTitle = (entries) =>
                {
                    if (grade.OwnerSubject.OwnerTable.UseWeight)
                    {
                        Console.WriteLine("--- {0} : {1} | {2} ---", Lang.GetString("Grade"), grade.Value, grade.Weight);
                    }
                    else
                    {
                        Console.WriteLine("--- {0} : {1} ---", Lang.GetString("Grade"), grade.Value);
                    }
                },

                DisplayEntry = (entries, entry, index, num) =>
                {
                    Console.WriteLine("[{0}] {1}", num, entry);
                },

                HandleEntry = (entries, index) =>
                {
                    if (options.ContainsKey(entries[index]))
                    {
                        return options[entries[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },


            }.Show();

        }

        /// <summary>
        /// Displays a menu for choosing a grade.
        /// </summary>
        /// <param name="subject">The subject which grades can be chosen from.</param>
        public static void ChooseGrade(Table.Subject subject)
        {

            new ListMenu<Table.Subject.Grade>(subject.Grades)
            {
                DisplayTitle = (grades) =>
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("GradeChoose"), grades.Count);
                    Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(grades.Count).Length, ' '), Lang.GetString("GradeCreate"));
                },

                DisplayEntry = (grades, grade, index, num) =>
                {
                    int MaxLength = grades.Select(x => x.Value.ToString().Length).Max();
                    if (subject.OwnerTable.UseWeight)
                    {
                        Console.WriteLine("[{0}] {1} | {2}", Convert.ToString(num).PadLeft(Convert.ToString(grades.Count).Length, ' '), Convert.ToString(grade.Value).PadRight(MaxLength, ' '), grade.Weight);
                    }
                    else
                    {
                        Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(grades.Count).Length, ' '), Convert.ToString(grade.Value).PadRight(MaxLength, ' '));
                    }
                },

                HandleEntry = (grades, index) =>
                {
                    ManageGrade(grades[index]);
                    return false;
                },

                ZeroEntry = (grades) =>
                {
                    CreateGrade(subject);
                    return false;
                },

            }.Show();

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
            if (grade.OwnerSubject.OwnerTable.UseWeight)
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
            string input;
            double value = -1;
            double weight = -1;
            bool IsFirstInputValid = false;
            while (!IsFirstInputValid)
            {
                Console.Clear();
                Console.WriteLine(title);

                Console.Write("\n");
                Console.Write("{0}> ", Lang.GetString("Grade"));
                input = Console.ReadLine();

                if (double.TryParse(input, out value))
                {
                    // Check if the table has grade limits enabled.
                    if (settings.EnableGradeLimits)
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
            if (subject.OwnerTable.UseWeight)
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
            Console.Clear();
            if (t.Subjects.Any() && t.Subjects.SelectMany(s => s.Grades).Any())
            {

                // Prevents overview for wrong systems.
                if (settings.DefaultMinGrade < 0 || settings.DefaultMaxGrade < 0)
                {
                    Console.WriteLine("{0} : {1}", Lang.GetString("Overview"), Lang.GetString("OverviewDataError"));
                    return;
                }

                // Calculate the maximum length of any word in front of the bar diagramm.
                int MaxLength = t.Subjects.Select(x => x.Name.Length).Max();
                if (MaxLength < Lang.GetString("Overview").Length) { MaxLength = Lang.GetString("Overview").Length; }
                if (MaxLength < Lang.GetString("Total").Length) { MaxLength = Lang.GetString("Total").Length; }
                if (settings.DisplayCompensation) { if (MaxLength < Lang.GetString("Compensation").Length) { MaxLength = Lang.GetString("Compensation").Length; } }
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

                foreach (Table.Subject s in settings.SortOverview ? t.Subjects.OrderByDescending(x => x.CalcAverage()).ToList() : t.Subjects)
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
                if (settings.DisplayCompensation && isSwiss)
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
                    Wait(500);
                }
                return true;
            }
            // Catch any error and write an error if details are enabled.
            catch (UnauthorizedAccessException)
            {
                if (verbose)
                {
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableDeniedAccess"));
                    Wait(500);
                }
                return false;
            }
            catch (Exception)
            {
                if (verbose)
                {
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableWriteError"));
                    Wait(500);
                }
                return false;
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
            Wait(150);
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
            Console.Clear();
        }

        /// <summary>
        /// Handler subscribed to the closing of the console.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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

            Dictionary<string, Action> options = new Dictionary<string, Action>()
            {
                { Lang.GetString("SettingsOptions"), ModifySettings },
                { Lang.GetString("Tables"), ModifyTableDefaults },
                { Lang.GetString("SettingsReset"), () => 
                    {
                        void Yes()
                        {
                            settings.Reset();
                            try { System.IO.File.Delete(System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath); }
                            catch { Console.WriteLine("[Error] Access to settings denied"); }
                            Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("SettingsResetSuccess"));
                        }
                        YesNoMenu("SettingsReset", Yes, () => { });
                        Wait(500);
                    }
                },
                { Lang.GetString("Credits"), () =>
                    {
                        Console.Clear();
                        Console.WriteLine(Lang.GetString("CreditsApp"));
                        Console.WriteLine();
                        Console.WriteLine(Lang.GetString("CreditsIcon"));
                        Console.WriteLine();
                        Console.WriteLine(Lang.GetString("PressAnything"));
                        Console.ReadKey();
                    }
                },
            };

            new ListMenu<string>(options.Keys.ToList())
            {

                DisplayTitle = (entries) =>
                {
                    Console.WriteLine("--- {0} : {1} ---", NewConsoleTitle, Lang.GetString("Settings"));
                },

                DisplayEntry = (entries, entry, index, num) =>
                {
                    Console.WriteLine("[{0}] {1}", num, entry);
                },

                HandleEntry = (entries, index) =>
                {
                    if (options.ContainsKey(entries[index]))
                    {
                        options[entries[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },

            }.Show();

        }

        /// <summary>
        /// A menu for changing the app settings.
        /// </summary>
        public static void ModifySettings()
        {

            Dictionary<string, Action> options = new Dictionary<string, Action>
            {
                { Lang.GetString("TableDefault"), SetDefaultTable },
                { Lang.GetString("LanguageChoose"), ChooseLang },
                { Lang.GetString("SettingsEnableGradeLimits"), () => 
                    {
                        settings.DisplayCompensation = !settings.DisplayCompensation;
                        settings.Save();
                    } 
                },
                { Lang.GetString("SettingsShowCompensation"),  () =>
                    {
                        settings.EnableGradeLimits = !settings.EnableGradeLimits;
                        settings.Save();
                    } 
                },
                { Lang.GetString("OverviewSortByHighest"),  () =>
                    {
                        settings.SortOverview = !settings.SortOverview;
                        settings.Save();
                    } 
                },
            };

            // Dictionatry mapping options to object properties.
            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { Lang.GetString("TableDefault"), "SourceFile" },
                { Lang.GetString("LanguageChoose"), "Language" },
                { Lang.GetString("SettingsEnableGradeLimits"), "EnableGradeLimits" },
                { Lang.GetString("SettingsShowCompensation"), "DisplayCompensation" },
                { Lang.GetString("OverviewSortByHighest"), "SortOverview" },

            };

            // Getting the maximum length of all strings.
            int MaxLength = options.Keys.ToList().Select(x => x.Length).Max();

            new ListMenu<string>(options.Keys.ToList())
            {

                DisplayTitle = (entries) =>
                {
                    Console.WriteLine("--- {0} : {1} ---", NewConsoleTitle, Lang.GetString("SettingsOptions"));
                },

                DisplayEntry = (entries, entry, index, num) =>
                {
                    string display = settings.GetType().GetProperty(ValueMap[entry]).GetValue(settings).ToString();

                    if (string.IsNullOrEmpty(display)) { display = Lang.GetString("NoData"); }
                    if (display == "True") { display = Lang.GetString("Yes"); }
                    if (display == "False") { display = Lang.GetString("No"); }

                    Console.WriteLine("[{0}] {1} | {2}", num, entry.PadRight(MaxLength, ' '), display);
                },

                HandleEntry = (entries, index) =>
                {
                    if (options.ContainsKey(entries[index]))
                    {
                        options[entries[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },

            }.Show();
        }

        /// <summary>
        /// Displays a menu for choosing a language.
        /// </summary>
        public static void ChooseLang()
        {
            new ListMenu<System.Globalization.CultureInfo>(GetAvailableCultures(Lang))
            {

                DisplayTitle = (langs) =>
                {
                    if (string.IsNullOrEmpty(settings.Language.Name))
                    {
                        Console.WriteLine("--- {0} : {1} ---", Lang.GetString("LanguageChoose"), langs.Count);
                    }
                    else
                    {
                        Console.WriteLine("--- {0} : {1} ---", Lang.GetString("LanguageChoose"), settings.Language.Name);
                    }
                    Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(langs.Count).Length, ' '), Lang.GetString("LanguageDefault"));
                },

                DisplayEntry = (langs, lang, index, num) =>
                {
                    Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(langs.Count).Length, ' '), lang.Name);
                },

                HandleEntry = (langs, index) =>
                {
                    settings.Language = langs[index];
                    settings.OverrideLanguage = true;
                    settings.Save();
                    Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("LanguageSetSuccess"));
                    return true;
                },

                ZeroEntry = (langs) =>
                {
                    try
                    {
                        settings.Language = System.Globalization.CultureInfo.InvariantCulture;
                        settings.OverrideLanguage = false;
                        settings.Save();
                        Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("LanguageResetSuccess"));
                    } catch
                    {
                        // LanguageResetFailure
                    }
                    
                    return true;
                },

            }.Show();

        }

        /// <summary>
        /// Displays a menu for choosing the default table.
        /// </summary>
        public static void SetDefaultTable()
        {
            List<string> Tables = new List<string>();
            try
            {
                // Fetching all files in the app directory that have the "grades.json" ending.
                Tables = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "grades*.json").ToList();
                Tables.Sort((a, b) => b.CompareTo(a));
            }
            catch (Exception)
            {
                Console.WriteLine("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableDeniedAccess"));
                Wait(500);
            }

            new ListMenu<string>(Tables)
            {
                // Displaying the main menu title.
                DisplayTitle = (tables) => 
                {
                    Console.WriteLine("--- {0} : {1} ---", Lang.GetString("TableSetDefault"), tables.Count);
                },

                DisplayEntry = (tables, table, index, num) =>
                {
                    int MaxLength = tables.Select(x => System.IO.Path.GetFileName(x).Length).Max();
                    string name;
                    try
                    {
                        name = Table.Read(tables[index]).Name;
                    }
                    catch (Exception)
                    {
                        name = Lang.GetString("NoData");
                    }
                    Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(tables.Count).Length, ' '),
                        System.IO.Path.GetFileName(tables[index]).PadRight(MaxLength, ' ') + " | " + name);
                },

                HandleEntry = (tables, index) => 
                {
                    try
                    {
                        settings.SourceFile = System.IO.Path.GetFileName(tables[index]);
                        settings.Save();
                        Console.WriteLine("[{0}] {1}", Lang.GetString("Log"), Lang.GetString("TableSetDefaultSuccess"));
                        Wait(500);
                    }
                    catch (Exception)
                    {
                        ResetInput(string.Format("[{0}] {1}", Lang.GetString("Error"), Lang.GetString("TableReadError")));
                    }
                    return true;
                },

            }.Show();

        }

        /// <summary>
        /// A menu for changing the default settings for new Tables.
        /// </summary>
        public static void ModifyTableDefaults()
        {
            Dictionary<string, Action> options = new Dictionary<string, Action>()
            {
                { Lang.GetString("GradeMin"), () => 
                    {
                        settings.DefaultMinGrade = GetTableGradeLimits(true, settings.DefaultMinGrade);
                        settings.Save();
                    } 
                },
                { Lang.GetString("GradeMax"), () =>
                    {
                        settings.DefaultMaxGrade = GetTableGradeLimits(false, settings.DefaultMaxGrade);
                        settings.Save();
                    } 
                },
                { Lang.GetString("TableUseWeight"), () => 
                    {
                        settings.DefaultUseWeight = !settings.DefaultUseWeight;
                        settings.Save();
                    } 
                },
            };

            // Dictionatry mapping options to object properties.
            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { Lang.GetString("GradeMin"), "DefaultMinGrade" },
                { Lang.GetString("GradeMax"), "DefaultMaxGrade" },
                { Lang.GetString("TableUseWeight"), "DefaultUseWeight" },

            };

            int MaxLength = options.Keys.ToList().Select(x => x.Length).Max();

            new ListMenu<string>(options.Keys.ToList())
            {
                DisplayTitle = (entries) => 
                {
                    Console.WriteLine("--- {0} : {1} ---", NewConsoleTitle, Lang.GetString("Defaults"));
                },

                DisplayEntry = (entries, entry, index, num) =>
                {
                    string display = settings.GetType().GetProperty(ValueMap[entry]).GetValue(settings).ToString();

                    if (string.IsNullOrEmpty(display)) { display = Lang.GetString("NoData"); }
                    if (display == "True") { display = Lang.GetString("Yes"); }
                    if (display == "False") { display = Lang.GetString("No"); }

                    Console.WriteLine("[{0}] {1} | {2}", num, entry.PadRight(MaxLength, ' '), display);
                },

                HandleEntry = (entries, index) => 
                {
                    if (options.ContainsKey(entries[index]))
                    {
                        options[entries[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },

            }.Show();
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
                catch (System.Globalization.CultureNotFoundException)
                {
                    Console.WriteLine("[Error] No cultures found");
                }
            }
            return result;
        }

        /// <summary>
        /// Unified function for waiting in miliseconds.
        /// </summary>
        /// <param name="ms">Amount of miliseconds to be waited for.</param>
        public static void Wait(int ms)
        {
            using (System.Threading.ManualResetEvent wait = new System.Threading.ManualResetEvent(false))
            {
                wait.WaitOne(ms);
            }
        }


        public class ListMenu<T>
        {

            /// <summary>
            /// CLOE (Command-line List Options Enumerator).
            /// <para>Turns a list into a menu of options. Each list item is asigned a number. The chosen one will be handled by passed functions.</para>
            /// </summary>
            /// <param name="entries">List of objects you want to display</param>
            public ListMenu(List<T> entries) {
                Entries = entries;
            }

            /// <summary>
            /// The prompt that is displayed to the user.
            /// </summary>
            public string Prompt = Lang.GetString("Choose");

            /// <summary>
            /// The string to be displayed for the option to exit the menu.
            /// </summary>
            public string ExitEntry = Lang.GetString("Back");

            /// <summary>
            /// The key the user has to press to exit the menu.
            /// </summary>
            public char ExitKey = 'q';

            /// <summary>
            /// Wether or not the user can exit the menu.
            /// </summary>
            public bool UserCanExit = true;


            private List<T> Entries;

            /// <summary>
            /// The function that processes the chosen menu entries.
            /// </summary>
            public Func<List<T>, int, bool> HandleEntry = (entries, index) => {
                Console.Clear();
                Console.WriteLine(entries[index]);
                Wait(200);
                return false;
            };

            /// <summary>
            /// The function that displays the menu title.
            /// </summary>
            public Action<List<T>> DisplayTitle = (entries) => { };

            /// <summary>
            /// The function that displays the entry to the user.
            /// </summary>
            public Action<List<T>, T, int, int> DisplayEntry = (entries, entry, index, num) =>
            {
                Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(entries.Count).Length, ' '), entry);
            };

            /// <summary>
            /// The function to update the list of entries.
            /// </summary>
            public Func<List<T>, List<T>> RefreshEntries = (entries) =>
            {
                return entries;
            };

            /// <summary>
            /// The function that is called when 0th entry in the list is chosen.
            /// <para>Display this entry with the title function.</para>
            /// </summary>
            public Func<List<T>, bool> ZeroEntry = (entries) =>
            {
                ResetInput();
                return false;
            };

            /// <summary>
            /// Display the menu.
            /// </summary>
            /// <returns></returns>
            public ListMenu<T> Show()
            {
                string readInput = string.Empty;
                bool MenuExitIsPending = false;
                while (!MenuExitIsPending)
                {
                    Console.Clear();
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
                        Wait(20);
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
                                        readInput = new System.Text.StringBuilder().Append(readInput).Append(Convert.ToString(choiceNum)).ToString();
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
                return this;
            }
        }

        /// <summary>
        /// A simple template to create Yes or No menus.
        /// </summary>
        /// <param name="title">The title of the menu.</param>
        /// <param name="Yes">The function to be called upon Yes</param>
        /// <param name="No">The function to be called upon No</param>
        public static void YesNoMenu(string title, Action Yes, Action No)
        {
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                // Ask the user for confirmation of deleting the current table.
                // This is language dependent.
                Console.Write("{0}? [{1}]> ", Lang.GetString(title), Lang.GetString("YesOrNo"));
                string Input = Console.ReadKey().KeyChar.ToString();
                Wait(20);
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