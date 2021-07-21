using System;
using Realms;

namespace QRTrackerNext.Models
{
    public class Item: RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
    }
}