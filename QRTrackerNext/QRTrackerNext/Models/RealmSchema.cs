using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using Realms;
using MongoDB.Bson;
using System.Linq;
using TinyPinyin.Core;

namespace QRTrackerNext.Models
{
    class Student : RealmObject {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [Indexed]
        [MapTo("Name")]
        private string name { get; set; }

        [Indexed]
        [MapTo("NamePinyin")]
        private string namePinyin { get; set; }

        public string Name
        {
            get => name;
            set {
                name = value;
                namePinyin = PinyinHelper.GetPinyin(name);
            }
        }

        public string NamePinyin { get => namePinyin; }

        public string StudentNumber { get; set; }

        public Group Group { get; set; }

        [Backlink(nameof(HomeworkStatus.Student))]
        public IQueryable<HomeworkStatus> Homeworks { get; }
    } 

    class Group : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [Indexed]
        [MapTo("Name")]
        private string name { get; set; }

        [Indexed]
        [MapTo("NamePinyin")]
        private string namePinyin { get; set; }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                namePinyin = PinyinHelper.GetPinyin(name);
            }
        }

        public string NamePinyin { get => namePinyin; }

        [Backlink(nameof(Student.Group))]
        public IQueryable<Student> Students { get; }

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
        public string Color { get; set; } = "gray";

        public Homework Homework { get; set; }
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
        
        [Backlink(nameof(HomeworkStatus.Homework))]
        public IQueryable<HomeworkStatus> Status { get; }

        public HomeworkType Type { get; set; }
    }

    class HomeworkType : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [Indexed]
        public string Name { get; set; }
        public IList<string> Colors { get; }
        public string NotCheckedDescription { get; set; } = string.Empty;
        public string NoColorDescription { get; set; } = string.Empty;
        public IDictionary<string, string> ColorDescriptions { get; }
        public bool IsBuiltin { get; set; }

        [Backlink(nameof(Homework.Type))]
        public IQueryable<Homework> Homeworks { get; }
    }
}
