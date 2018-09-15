using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
        [DataMember(Name = "Name")]
        public string name;
        /// <summary>
        /// The minimum grade value for this table and system.
        /// This value does not alter the behaviour of the table class.
        /// </summary>
        [DataMember(Name = "MinGrade")]
        public double MinGrade;
        /// <summary>
        /// The maximum grade value for this table and system.
        /// This value does not alter the behaviour of the table class.
        /// </summary>
        [DataMember(Name = "MaxGrade")]
        public double MaxGrade;
        /// <summary>
        /// If minimum grade and maximum grade limits should be checked on creation.
        /// This value does not alter the behaviour of the table class.
        /// </summary>
        [DataMember(Name = "EnableGradeLimits")]
        public bool EnableGradeLimits = true;
        /// <summary>
        /// If the weight system should be used.
        /// This value does not alter the behaviour of the table class.
        /// </summary>
        [DataMember(Name = "EnableWeightSystem")]
        public bool EnableWeightSystem = true;


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
        /// The subject class. It stores grades.
        /// </summary>
        [DataContract(Name = "Subject")]
        public class Subject
        {
            /// <summary>
            /// The name of the subject.
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; internal set; }

            /// <summary>
            /// The instance of the table this subject is assigned to.
            /// </summary>
            [DataMember(Name = "OwnerTable")]
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
            [DataContract(Name = "Grade")]
            public class Grade
            {
                /// <summary>
                /// The value of the grade.
                /// </summary>
                [DataMember(Name = "Value")]
                public double Value { get; internal set; }

                /// <summary>
                /// The weight of the grade.
                /// </summary>
                [DataMember(Name = "Weight")]
                public double Weight { get; internal set; }

                /// <summary>
                /// The instance of the subject this grade is assigned to.
                /// </summary>
                [DataMember(Name = "OwnerSubject")]
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
        /// Save the table to an xml file.
        /// </summary>
        /// <param name="File">The target file path as string.</param>
        public void Write(string File)
        {
            using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(File, new System.Xml.XmlWriterSettings { Indent = true }))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Table), null,
                    0x7FFF /*maxItemsInObjectGraph*/,
                    false /*ignoreExtensionDataObject*/,
                    true /*preserveObjectReferences : important option! */,
                    null /*dataContractSurrogate*/);
                serializer.WriteObject(writer, this);
            }
        }

        /// <summary>
        /// Read a table from an xml file.
        /// </summary>
        /// <param name="File">The target file path as string.</param>
        public static Table Read(string File)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Table));
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(File))
            {
                return (Table)serializer.ReadObject(reader);
            }
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