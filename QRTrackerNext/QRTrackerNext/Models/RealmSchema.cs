using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using Realms;
using MongoDB.Bson;
using System.Linq;

namespace QRTrackerNext.Models
{
    class Student : RealmObject {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [Indexed]
        public string Name { get; set; }

        [Backlink(nameof(Models.Group.Students))]
        public IQueryable<Group> Group { get; }

        [Backlink(nameof(HomeworkStatus.Student))]
        public IQueryable<HomeworkStatus> Homeworks { get; }
    } 

    class Group : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [Indexed]
        public string Name { get; set; }

        public IList<Student> Students { get; }

        [Backlink(nameof(Homework.Groups))]
        public IQueryable<Homework> Homeworks { get; }
    }

    class HomeworkStatus : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public Student Student { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Color { get; set; } = "grey";
        [Indexed]
        public ObjectId HomeworkId { get; set; }
        [Indexed]
        public bool HasScanned { get; set; } = false;
    }

    class Homework : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [Indexed]
        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

        [Indexed]
        public string Name { get; set; }
        public string Notes { get; set; } = string.Empty;
        public IList<Group> Groups { get; }
        public IList<HomeworkStatus> Status { get; }

        public IList<string> Colors { get; }
    }
}
