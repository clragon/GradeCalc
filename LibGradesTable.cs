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

        [DataMember(Name = "Subjects")]
        public List<Subject> Subjects = new List<Subject>();

        public void AddSubject(string name)
        {
            Subject x = new Subject();
            x.name = name;
            Subjects.Add(x);
        }

        public void RemSubject(int index)
        {
            Subjects.RemoveAt(index);
        }

        [DataContract(Name = "Subject")]
        public class Subject
        {

            [DataMember(Name = "Name")]
            public string name { get; internal set; }

            internal Subject() { }

            public void EditSubject(string name)
            {
                this.name = name;
            }

            [DataMember(Name = "Grades")]
            public List<Grade> Grades = new List<Grade>();

            public void AddGrade(double value, double weight)
            {
                Grade y = new Grade();
                y.value = value;
                y.weight = weight;
                Grades.Add(y);
            }

            public void RemGrade(int index)
            {
                Grades.RemoveAt(index);
            }

            [DataContract(Name = "Grade")]
            public class Grade
            {
                [DataMember(Name = "Value")]
                public double value { get; internal set; }

                [DataMember(Name = "Weight")]
                public double weight { get; internal set; }

                internal Grade() { }

                public void EditGrade(double value, double weight)
                {
                    this.value = value;
                    this.weight = weight;
                }

            }

        }

        public void Write(string File)
        {
            DataContractSerializer serializer = new DataContractSerializer(this.GetType());
            using(System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(File, new System.Xml.XmlWriterSettings { Indent = true }))
            {
                serializer.WriteObject(writer, this);
            }
        }

        public static Table Read(string File)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Table));
            using(System.Xml.XmlReader reader = System.Xml.XmlReader.Create(File))
            {
                return (Table) serializer.ReadObject(reader);
            }
        }

        public void Clear(string File)
        {
            this.Subjects.Clear();
            this.Subjects.TrimExcess();
            System.IO.File.Delete(File);
        }

    }
}