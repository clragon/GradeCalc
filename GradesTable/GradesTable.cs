using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Grades
{
    [DataContract(Name = "Table")]
    public class Table
    {

        [DataMember(Name = "Name")]
        public string name;
        [DataMember(Name = "MinGrade")]
        public double MinGrade;
        [DataMember(Name = "MaxGrade")]
        public double MaxGrade;
        [DataMember(Name = "EnableGradeLimits")]
        public bool EnableGradeLimits = true;
        [DataMember(Name = "EnableWeightSystem")]
        public bool EnableWeightSystem = true;

        [DataMember(Name = "Subjects")]
        public List<Subject> Subjects { get; internal set; } = new List<Subject>();

        public void AddSubject(string name)
        {
            Subject x = new Subject
            {
                Name = name,
                OwnerTable = this
            };
            Subjects.Add(x);
        }

        public void AddSubject(Subject s)
        {
            Subjects.Add(s);
        }

        public void RemSubject(int index)
        {
            Subjects.RemoveAt(index);
        }

        public void RemSubject(Subject s)
        {
            Subjects.RemoveAt(Subjects.IndexOf(s));
        }

        [DataContract(Name = "Subject")]
        public class Subject
        {

            [DataMember(Name = "Name")]
            public string Name { get; internal set; }

            [DataMember(Name = "OwnerTable")]
            public Table OwnerTable { get; internal set; }

            internal Subject() { }

            public void EditSubject(string name)
            {
                Name = name;
            }

            public void MoveSubject(Table t)
            {
                OwnerTable.RemSubject(this);
                t.AddSubject(this);
            }

            [DataMember(Name = "Grades")]
            public List<Grade> Grades { get; internal set; } = new List<Grade>();

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

            public void AddGrade(Grade g)
            {
                Grades.Add(g);
            }

            public void RemGrade(int index)
            {
                Grades.RemoveAt(index);
            }

            public void RemGrade(Grade g)
            {
                Grades.RemoveAt(Grades.IndexOf(g));
            }

            [DataContract(Name = "Grade")]
            public class Grade
            {
                [DataMember(Name = "Value")]
                public double Value { get; internal set; }

                [DataMember(Name = "Weight")]
                public double Weight { get; internal set; }

                [DataMember(Name = "OwnerSubject")]
                public Subject OwnerSubject { get; internal set; }

                internal Grade() { }

                public void EditGrade(double value, double weight)
                {
                    Value = value;
                    Weight = weight;
                }

                public void MoveSubject(Subject s)
                {
                    OwnerSubject.RemGrade(this);
                    s.AddGrade(this);

                }

            }

        }

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

        public static Table Read(string File)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Table));
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(File))
            {
                return (Table)serializer.ReadObject(reader);
            }
        }

        public void Clear(string File)
        {
            Subjects.Clear();
            Subjects.TrimExcess();
            System.IO.File.Delete(File);
        }

    }
}