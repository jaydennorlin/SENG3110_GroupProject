using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AssignmentTracker {
    public class Term {
        public string termName { get; set; }
        //Add start/end date.
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

        public string directory { get; set; }

        //Investigate other means of storing this.
        public List<Course> courses { get; set; } = new List<Course>();

        public void SaveTerm()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            string jsonString = JsonSerializer.Serialize(this, options);

            GD.Print("Serialied " + termName + " to " + jsonString);

            File.WriteAllText(directory, jsonString);
        }

        public static Term LoadTerm(string directory)
        {
            Term toReturn = JsonSerializer.Deserialize<Term>(File.ReadAllText(directory));

            if (toReturn == null)
            {
                //Throw some error or something.
            }

            //Taskbase.Course is NOT serialied, due to it creating a serialization loop. This manually adds it in afterwards.
            for (int i = 0; i < toReturn.courses.Count; i++)
            {
                for (int j = 0; j < toReturn.courses[i].tasks.Count; j++)
                {
                    toReturn.courses[i].tasks[j].course = toReturn.courses[i];
                }
            }

            return toReturn;
        }
    }

    public class Course {
        //Consider adding an additional variable, "Course Code" or something like that.
        //So we can refer to the couse either by it's fill name (ex "Algorithms and Data Structures") or by its Code (ex "SENG 4530)
        public string courseName { get; set; }
        //Consider making this some sort of two-part string.
        //First part is the type of data being stored (Professor Name, Phone Number, etc), second part is the actual data.
        //public string[] additonalData;
        public string notes { get; set; }

        public List<PrimaryTask> tasks { get; set; } = new List<PrimaryTask>();

        public Course(string courseName, string addData)
        {
            this.courseName = courseName;
            this.notes = addData;
            //this.additonalData = new string[] { addData };
        }

        public Course()
        {

        }
    }

    public class TaskBase {
        public short completion { get; set; } = 0;
        public float value { get; set; }
        public string taskDescription { get; set; }

        //Task Type could be Quiz, Midterm, Aiisgments, Project or Others
        public string taskType { get; set; } = "";

        //Two way connection, so a task alone knows what course its part of.
        public Course course;

        public TaskBase(string desc, string taskType)
        {
            this.taskDescription = desc;
            this.taskType = taskType;
        }

        public TaskBase()
        {

        }
    }

    public class PrimaryTask : TaskBase {
        public string taskName { get; set; }

        public DateTime dueDate { get; set; }

        public List<TaskBase> subtasks { get; set; } = new List<TaskBase>();

        public PrimaryTask(string taskName, Course course)
        {
            this.taskName = taskName;
            this.course = course;
        }

        public PrimaryTask()
        {

        }
    }

    /*public class Subtask : TaskBase {
        //Have primary task to look up. Do we need this one?
        public PrimaryTask primaryTask;
    }*/
}