using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Grades
{
    /// <summary>
    /// The table class. It stores subjects and grades.
    /// </summary>
    [DataContract(Name = "Table")]
    public class Table
    {

        /// <summary>
        /// The name of the table.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }
        /// <summary>
        /// The minimum grade value for this table and system.
        /// This value does not alter the behaviour of the table class.
        /// </summary>
        [DataMember(Name = "minGrade")]
        public double MinGrade { get; set; }
        /// <summary>
        /// The maximum grade value for this table and system.
        /// This value does not alter the behaviour of the table class.
        /// </summary>
        [DataMember(Name = "maxGrade")]
        public double MaxGrade { get; set; }
        /// <summary>
        /// If the weight system should be used.
        /// This value does not alter the behaviour of the table class.
        /// </summary>
        [DataMember(Name = "useWeight")]
        public bool UseWeight { get; set; } = true;


        /// <summary>
        /// The list of subjects assigned to this table.
        /// </summary>
        [DataMember(Name = "Subjects")]
        public List<Subject> Subjects { get; internal set; } = new List<Subject>();

        /// <summary>
        /// Add an empty new subject.
        /// </summary>
        /// <param name="name">The name of the subject.</param>
        public void AddSubject(string name)
        {
            Subject x = new Subject
            {
                Name = name,
                OwnerTable = this
            };
            Subjects.Add(x);
        }

        /// <summary>
        /// Add an existing subject.
        /// </summary>
        /// <param name="s">The subject instance that is to be added.</param>
        public void AddSubject(Subject s)
        {
            Subjects.Add(s);
        }

        /// <summary>
        /// Remove a subject by it's index.
        /// </summary>
        /// <param name="index">The index of the subject in the subject list.</param>
        public void RemSubject(int index)
        {
            Subjects.RemoveAt(index);
        }

        /// <summary>
        /// Remove a subject by it's instance.
        /// </summary>
        /// <param name="s">The instance of the subject.</param>
        public void RemSubject(Subject s)
        {
            Subjects.RemoveAt(Subjects.IndexOf(s));
        }

        /// <summary>
        /// Method to calculate the average grade of a table.
        /// Uses the override method to calculate the average of a subject to function.
        /// </summary>
        public double CalcAverage()
        {
            if (Subjects.Any())
            {
                double averages = 0;
                int count = 0;
                foreach (Subject s in Subjects)
                {
                    if (s.Grades.Any())
                    {
                        averages += s.CalcAverage();
                        count++;
                    }
                }
                // Rounded to 0.5
                // Average of the table is calculated by all averages of the subjects divided by the amounts of subjects.
                return Math.Round((averages / count) * 2, MidpointRounding.ToEven) / 2;
            }
            else { return 0; }
        }

        /// <summary>
        /// Method to calculate the compensation needed for a table.
        /// Uses the override method to calculate the compensation needed for a subject to function.
        /// </summary>
        public double CalcCompensation()
        {
            if (Subjects.Any())
            {
                double points = 0;
                foreach (Subject s in Subjects)
                {
                    if (s.Grades.Any())
                    {
                        points += s.CalcCompensation();
                    }
                }
                return points;
            }
            else { return 0; }
        }

        /// <summary>
        /// The subject class. It stores grades.
        /// </summary>
        [DataContract(Name = "Subject")]
        public class Subject
        {
            /// <summary>
            /// The name of the subject.
            /// </summary>
            [DataMember(Name = "name")]
            public string Name { get; internal set; }

            /// <summary>
            /// The instance of the table this subject is assigned to.
            /// </summary>
            // [DataMember(Name = "OwnerTable")]
            // owner property is runtime only to make xml more readable
            public Table OwnerTable { get; internal set; }

            internal Subject() { }

            /// <summary>
            /// Edit the name of the subject.
            /// </summary>
            /// <param name="name">The new name of the subject.</param>
            public void EditSubject(string name)
            {
                Name = name;
            }

            /// <summary>
            /// Move the subject to a different table.
            /// </summary>
            /// <param name="t">The instance of the target table.</param>
            public void MoveSubject(Table t)
            {
                OwnerTable.RemSubject(this);
                t.AddSubject(this);
            }

            /// <summary>
            /// Method to calculate the average grade of a subject.
            /// </summary>
            public double CalcAverage()
            {
                if (Grades.Any())
                {
                    double values = 0, weights = 0;
                    foreach (Grade g in Grades)
                    {
                        // If the weight system is enabled, get the weight of the grade.
                        if (OwnerTable.UseWeight)
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
            /// Method to calculate the compensation needed for a subject.
            /// </summary>
            public double CalcCompensation()
            {
                double points;
                // Compensation points are calculated by subtracting 4 of the grade value.
                // Positive points have to outweight negative ones by a factor of two.
                if (CalcAverage() != 0)
                {
                    points = (CalcAverage() - 4);
                    if (points < 0) { points = (points * 2); }
                }
                else
                {
                    points = 0;
                }

                return points;
            }

            /// <summary>
            /// The list of grades assigned to this subject.
            /// </summary>
            [DataMember(Name = "Grades")]
            public List<Grade> Grades { get; internal set; } = new List<Grade>();

            /// <summary>
            /// Add a new grade.
            /// </summary>
            /// <param name="value">The value of the grade.</param>
            /// <param name="weight">The weight of the grade.</param>
            public void AddGrade(double value, double weight)
            {
                Grade y = new Grade
                {
                    Value = value,
                    Weight = weight,
                    OwnerSubject = this
                };
                Grades.Add(y);
            }

            /// <summary>
            /// Add an existing grade.
            /// </summary>
            /// <param name="g">The instance of the grade</param>
            public void AddGrade(Grade g)
            {
                Grades.Add(g);
            }

            /// <summary>
            /// Remove a grade by it's index.
            /// </summary>
            /// <param name="index">The index of the grade in the grades list.</param>
            public void RemGrade(int index)
            {
                Grades.RemoveAt(index);
            }

            /// <summary>
            /// Remove a grade by it's instance.
            /// </summary>
            /// <param name="g">The instance of the grade.</param>
            public void RemGrade(Grade g)
            {
                Grades.RemoveAt(Grades.IndexOf(g));
            }

            /// <summary>
            /// The grades class. It stores values.
            /// </summary>
            [DataContract(Name = "grade")]
            public class Grade
            {

                /// <summary>
                /// The name of the grade.
                /// </summary>
                [DataMember(Name = "name")]
                public string Name { get; set; }

                /// <summary>
                /// The value of the grade.
                /// </summary>
                [DataMember(Name = "value")]
                public double Value { get; internal set; }

                /// <summary>
                /// The weight of the grade.
                /// </summary>
                [DataMember(Name = "weight")]
                public double Weight { get; internal set; }

                /// <summary>
                /// The creation date of the grade.
                /// </summary>
                [DataMember(Name = "creation")]
                public LocalDate LocalCreation { get; internal set; }

                [DataContract(Name = "creation")]
                public class LocalDate
                {
                    [DataMember(Name = "year")]
                    public int Year { get; internal set; }
                    [DataMember(Name = "month")]
                    public int Month { get; internal set; }
                    [DataMember(Name = "day")]
                    public int Day { get; internal set; }

                    internal LocalDate() { }

                    public DateTime GetDate()
                    {
                        return new DateTime(Year, Month, Day);
                    }

                    public void SetDate(DateTime dateTime)
                    {
                        Year = dateTime.Year;
                        Month = dateTime.Month;
                        Day = dateTime.Day;
                    }

                }

                /// <summary>
                /// The instance of the subject this grade is assigned to.
                /// </summary>
                // [DataMember(Name = "OwnerSubject")]
                // owner property is runtime only to make xml more readable
                public Subject OwnerSubject { get; internal set; }

                internal Grade() { }

                /// <summary>
                /// Edit the value and weight of the grade.
                /// </summary>
                /// <param name="value">The new value of the grade.</param>
                /// <param name="weight">The new weight of the grade.</param>
                public void EditGrade(double value, double weight)
                {
                    Value = value;
                    Weight = weight;
                }

                /// <summary>
                /// Move the grade to a different subject.
                /// </summary>
                /// <param name="s">The instance of the target subject.</param>
                public void MoveGrade(Subject s)
                {
                    OwnerSubject.RemGrade(this);
                    s.AddGrade(this);

                }

            }

        }

        /// <summary>
        /// Save the table to a json file.
        /// </summary>
        /// <param name="File">The target file path as string.</param>
        public void Write(string File)
        {
            System.IO.File.WriteAllText(File, JsonConvert.SerializeObject(this, Formatting.Indented));            
        }

        /// <summary>
        /// Read a table from a json file.
        /// </summary>
        /// <param name="File">The target file path as string.</param>
        public static Table Read(string File)
        {
            Table T = JsonConvert.DeserializeObject<Table>(System.IO.File.ReadAllText(File));
            // Set the owner properties for runtime.
            foreach (Subject s in T.Subjects)
            {
                s.OwnerTable = T;
                foreach (Subject.Grade g in s.Grades)
                {
                    g.OwnerSubject = s;
                }
            }
            return T;
        }

        /// <summary>
        /// Clear all subjects in this table and delete it's save file.
        /// </summary>
        /// <param name="File">The target file path as string.</param>
        public void Clear(string File)
        {
            Subjects.Clear();
            Subjects.TrimExcess();
            System.IO.File.Delete(File);
        }

    }
}