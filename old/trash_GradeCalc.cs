List<grade> gradeList(string subject)
{
    return new List<grade>();
}

/*
if (ClearOnSwitch)
{
	ClearCurrentConsoleLine();
	Console.SetCursorPosition(0, Console.CursorTop - 1);
	ClearCurrentConsoleLine();
}
else
{
	Console.Write("\n"];
}
*/

public static Table.Subject.Grade GetGrade(Table.Subject s)
{
    int index = -1;
    bool HasChosen = false;
    while (!HasChosen)
    {
        ClearMenu();
        Console.WriteLine("--- " + dict["ChooseGrade"] + " ---"];
        Console.WriteLine("[0] ({0})", dict["CreateGrade"]);
        if (s.Grades.Any())
        {
            int i = 0;
            int MaxLength = s.Grades.Select(x => x.value.ToString().Length).Max();
            foreach (Table.Subject.Grade g in s.Grades)
            {
                i++;
                Console.WriteLine("[{0}] {1} | {2}", i, Convert.ToString(g.value).PadRight(MaxLength, ' '), g.weight);
            }
        }
        Console.Write("\n"];

        bool IsInputValid = false;
        while (!IsInputValid)
        {
            Console.Write(dict["Choose"] + "> "];
            string input = Console.ReadKey().KeyChar.ToString();
            new System.Threading.ManualResetEvent(false).WaitOne(20);
            Console.Write("\n"];
            if (input == "\n"] { Console.SetCursorPosition(0, Console.CursorTop - 1); }

            int choice;
            if ((int.TryParse(input, out choice)) && (choice <= s.Grades.Count) && (choice >= 0))
            {
                if (choice == 0)
                {
                    IsInputValid = true;
                    CreateGrade(s);
                }
                else
                {
                    index = choice - 1;
                    IsInputValid = true;
                    HasChosen = true;
                }
            }
            else
            {
                InvalidInput();
            }
        }
    }

    return s.Grades[index];
}

public static Table.Subject GetSubject()
{
    int index = -1;
    bool HasChosen = false;
    while (!HasChosen)
    {
        ClearMenu();
        Console.WriteLine("--- " + dict["ChooseSubject"] + " ---"];
        Console.WriteLine("[0] ({0})", dict["CreateSubject"]);
        for (int i = 0; i < t.Subjects.Count; i++)
        {
            Console.WriteLine("[{0}] {1}", i + 1, t.Subjects[i].name);
        }
        Console.Write("\n"];

        bool IsInputValid = false;
        while (!IsInputValid)
        {
            Console.Write(dict["Choose"] + "> "];
            string input = Console.ReadKey().KeyChar.ToString();
            new System.Threading.ManualResetEvent(false).WaitOne(20);
            Console.Write("\n"];
            if (input == "\n"] { Console.SetCursorPosition(0, Console.CursorTop - 1); }

            int choice;
            if ((int.TryParse(input, out choice)) && (choice <= t.Subjects.Count) && (choice >= 0))
            {
                if (choice == 0)
                {
                    IsInputValid = true;
                    CreateSubject();
                }
                else
                {
                    index = choice - 1;
                    IsInputValid = true;
                    HasChosen = true;
                }
            }
            else
            {
                InvalidInput();
            }
        }
    }

    return t.Subjects[index];
}

public static void ManageMenu()
{
    bool IsMenuExitPending = false;
    while (!IsMenuExitPending)
    {

        ClearMenu();
        Console.WriteLine("--- " + dict["Manage"] + " ---"];
        Console.WriteLine("[1] " + dict["Subjects"]);
        Console.WriteLine("[2] " + dict["Table"]);
        Console.WriteLine("[x] " + dict["Back"]);
        Console.Write("\n"];

        bool IsInputValid = false;
        while (!IsInputValid)
        {
            Console.Write(dict["Choose"] + "> "];
            string input = Console.ReadKey().KeyChar.ToString();
            new System.Threading.ManualResetEvent(false).WaitOne(20);
            Console.Write("\n"];
            if (input == "\n"] { Console.SetCursorPosition(0, Console.CursorTop - 1); }
            switch (input)
            {
                case "1":
                    IsInputValid = true;
                    ChooseSubject();
                    break;

                case "2":
                    IsInputValid = true;
                    ManageTable();
                    break;

                case "x":
                    IsMenuExitPending = true;
                    IsInputValid = true;
                    break;

                default:
                    InvalidInput();
                    break;
            }
        }
    }

}

public static void QuickAddMenu()
{
    CreateGrade(GetSubject());
}

public static Dictionary<string, string> dict = GetLang();

public static Dictionary<string, string> GetLang()
{
    Dictionary<string, string> dict = new Dictionary<string, string>();

    switch (System.Threading.Thread.CurrentThread.CurrentCulture.Name)
    {
        case "de-CH":
        case "de-DE":
            dict["Title"] = "Notenrechner";
            dict["Overview"] = "Zusammenfassung";
            dict["Manage"] = "Daten verwalten";
            dict["Exit"] = "Schliessen";
            dict["Choose"] = "Auswahl";
            dict["Back"] = "Zurück";

            dict["Table"] = "Tabelle";
            dict["ManageTable"] = "Tabelle verwalten";
            dict["ReadTable"] = "Öffnen";
            dict["WriteTable"] = "Speichern";
            dict["DeleteTable"] = "Löschen";

            dict["Subject"] = "Fach";
            dict["Subjects"] = "Fächer";
            dict["ChooseSubject"] = "Fach wählen";
            dict["CreateSubject"] = "Neues Fach";
            dict["SubjectName"] = "Name";
            dict["RenameSubject"] = "Umbenennen";
            dict["DeleteSubject"] = "Löschen";

            dict["Grade"] = "Note";
            dict["Grades"] = "Noten";
            dict["Weight"] = "Gewicht";
            dict["ChooseGrade"] = "Note wählen";
            dict["CreateGrade"] = "Neue Note";
            dict["EditGrade"] = "bearbeiten";
            dict["DeleteGrade"] = "Löschen";

            dict["Overview"] = "Übersicht";
            dict["Total"] = "Total";
            dict["NoData"] = "Keine Daten vorhanden";
            dict["Average"] = "Durchschnitt";
            break;

        case "en-GB":
        case "en-US":
        default:
            dict["Title"] = "Grade Calc";
            dict["QuickAdd"] = "Add grade";
            dict["Overview"] = "Overview";
            dict["Manage"] = "Manage data";
            dict["Exit"] = "Exit";
            dict["Choose"] = "Choose";
            dict["Back"] = "Back";

            dict["Table"] = "Table";
            dict["ManageTable"] = "Manage table";
            dict["ReadTable"] = "Open";
            dict["WriteTable"] = "Save";
            dict["DeleteTable"] = "Delete";

            dict["Subjects"] = "Subjects";
            dict["Subject"] = "Subject";
            dict["ChooseSubject"] = "Choose subject";
            dict["CreateSubject"] = "New subject";
            dict["SubjectName"] = "Name";
            dict["RenameSubject"] = "Rename";
            dict["DeleteSubject"] = "Delete";

            dict["Grade"] = "Grade";
            dict["Grades"] = "Grades";
            dict["Weight"] = "Weight";
            dict["ChooseGrade"] = "Choose grade";
            dict["CreateGrade"] = "New grade";
            dict["EditGrade"] = "Edit";
            dict["DeleteGrade"] = "Delete";

            dict["Overview"] = "Overview";
            dict["Total"] = "Total";
            dict["NoData"] = "No data available";
            dict["Average"] = "Average";
            break;

    }

    return dict;
}

if (input == "q")
{
    IsInputValid = true;
    IsMenuExitPending = true;
}
else
{
    if ((input == "\b"))
    {
        if (!(InputString == ""))
        {
            Console.Write("\b");
            InputString = InputString.Remove(InputString.Length - 1);
        }
        IsInputValid = true;
    }
    else
    {
        if (input == "\n" && InputString != "")
        {
            InputString = "";
            index = Convert.ToInt32(InputString) - 1;
            IsInputValid = true;
            ManageSubject(t.Subjects[index]);
        }
        else
        {
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
                    InputString = InputString + Convert.ToString(choice);
                    if (Convert.ToInt32(InputString) <= t.Subjects.Count)
                    {
                        if (InputString.Length == Convert.ToString(t.Subjects.Count).Length)
                        {
                            InputString = "";
                            index = Convert.ToInt32(InputString);
                            index--;
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
                        InputString = InputString.Remove(InputString.Length - 1);
                        ResetInput();
                    }
                }
            }
            else
            {
                ResetInput();
            }
        }

    }
}