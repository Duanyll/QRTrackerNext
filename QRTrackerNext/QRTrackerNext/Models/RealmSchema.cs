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

    class ScanLog : RealmObject
    {
        public Student student { get; set; }
        public DateTimeOffset time { get; set; } = DateTimeOffset.Now;
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
        public IList<Group> Groups { get; }
        public IList<ScanLog> Scans { get; }
    }
}
