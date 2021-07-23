using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Bson;

namespace QRTrackerNext.Models
{
    static class UriHelper
    {
        public static string GetStudentUri(Student student)
        {
            return $"https://qrt.duanyll.com/stu?id={student.Id}";
        }

        public static ObjectId ParseStudentUri(string str)
        {
            if (str.StartsWith("https://qrt.duanyll.com/stu") || str.StartsWith("qrt://stu"))
            {
                return ObjectId.Parse(str.Split('=')[1]);
            }
            else if (ObjectId.TryParse(str, out ObjectId id))
            {
                return id;
            }
            else
            {
                return ObjectId.Empty;
            }
        }

        public static string GetStudentUriShort(Student student)
        {
            return $"qrt://stu?id={student.Id}";
        }
    }
}
