using QRTrackerNext.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Realms;

namespace QRTrackerNext.Services
{
    class RealmDataStore : IDataStore<Item>
    {
        Realm realm;

        public RealmDataStore()
        {
            realm = Realm.GetInstance();
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            realm.Write(() => realm.Add(item));
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            realm.Write(() => realm.Add(item));
            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            realm.Write(() =>
            {
                var item = realm.Find<Item>(id);
                realm.Remove(item);
            });
            return await Task.FromResult(true);
        }

        public async Task<Item> GetItemAsync(string id)
        {
            return await Task.FromResult(realm.Find<Item>(id));
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(realm.All<Item>());
        }
    }
}