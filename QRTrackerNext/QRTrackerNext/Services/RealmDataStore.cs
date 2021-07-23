using QRTrackerNext.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Realms;

namespace QRTrackerNext.Services
{
    class RealmDataStore<T> : IDataStore<T> where T : RealmObject
    {
        Realm realm;

        public RealmDataStore()
        {
            realm = Realm.GetInstance();
        }

        public async Task<bool> AddItemAsync(T item)
        {
            realm.Write(() => realm.Add(item));
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(T item)
        {
            realm.Write(() => realm.Add(item));
            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            realm.Write(() =>
            {
                var item = realm.Find<T>(id);
                realm.Remove(item);
            });
            return await Task.FromResult(true);
        }

        public async Task<T> GetItemAsync(string id)
        {
            return await Task.FromResult(realm.Find<T>(id));
        }

        public async Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(realm.All<T>());
        }
    }
}