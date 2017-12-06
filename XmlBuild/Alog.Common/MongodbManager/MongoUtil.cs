using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASPNetPortal.DB;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Alog.Common.MongodbManager
{
    public class MongoUtil<T> where T : class
    {
        //private static string conn = ClsLog.GetAppSettings("mongoConn");

        private static string connText;
        private static string ConnText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(connText))
                {
                    string text = ClsLog.GetAppSettings("mongoConn");
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        Task.Factory.StartNew(() => ClsLog.AppendDbLog("MongoConn Empty", "Mongodb", ErrorLevel.Error, ClsLog.ProjectName));
                        connText = "";
                    }

                    connText = DBAccess.DesDecrypt(text);
                }

                return connText;
            }
            set { connText = value; }
        }

        private static string dbName;
        private static string DbName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(dbName))
                {
                    string dbText = ClsLog.GetAppSettings("mongoDB");
                    if (string.IsNullOrWhiteSpace(dbText))
                    {
                        Task.Factory.StartNew(() => ClsLog.AppendDbLog("MongoDB Name Empty", "Mongodb", ErrorLevel.Error, ClsLog.ProjectName));
                        dbName = "";
                    }
                    dbName = dbText;
                }


                return dbName;
            }
            set { dbName = value; }
        }

        private readonly string CollectionName;
        private readonly string ConnKey;

        public MongoUtil(string collectionName)
        {
            this.CollectionName = collectionName;
        }

        public MongoUtil(string collectionName, string connKey)
        {
            ConnKey = connKey;
            CollectionName = collectionName;
            string text = ClsLog.GetAppSettings(connKey);

            
            if (!string.IsNullOrWhiteSpace(text))
            {
                ConnText = DBAccess.DesDecrypt(text);    
            }
            
        }

        public MongoUtil(string collectionName, string connKey, string databaseName)
        {
            ConnKey = connKey;
            CollectionName = collectionName;
            string text = ClsLog.GetAppSettings(connKey);
            
            if (!string.IsNullOrWhiteSpace(text))
            {
                ConnText = DBAccess.DesDecrypt(text);
            }

            if (!string.IsNullOrWhiteSpace(databaseName))
                DbName = databaseName;

        }

        private bool ParameterValid()
        {
            return !string.IsNullOrWhiteSpace(ConnText)
                   && !string.IsNullOrWhiteSpace(DbName)
                   && !string.IsNullOrWhiteSpace(CollectionName);
        }

        private MongoCollection GetCollection()
        {
            try
            {
                if (!ParameterValid()) return null;
                MongoClient client = new MongoClient(ConnText);
                MongoServer server = client.GetServer();
                MongoDatabase db = server.GetDatabase(DbName);

                
                MongoCollection col = db.GetCollection(CollectionName);
                return col;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => ClsLog.AppendDbLog(ex.Message, "GetCollection", ErrorLevel.Error, ClsLog.ProjectName));
                return null;
            }
            
        }

        public static string[] GetCollectionNames(string connKey, string databaseName)
        {
            try
            {
                string text = ClsLog.GetAppSettings(connKey);
                if (!string.IsNullOrWhiteSpace(connKey) && !string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(databaseName))
                {
                    string conn = DBAccess.DesDecrypt(text);
                    
                    MongoClient client = new MongoClient(conn);
                    
                    MongoServer server = client.GetServer();
                    
                    MongoDatabase db = server.GetDatabase(databaseName);
                    
                    var s = db.GetCollectionNames();
                    
                    
                    return s.ToArray();
                }

                ClsLog.AppendDbLog("connKey or dababaseName empty", "GetCollectionNamesEmpty", ErrorLevel.Error, ClsLog.ProjectName);
                return new string[0];
            }
            catch (Exception ex)
            {
                ClsLog.AppendDbLog(ex.Message, "GetCollectionNamesException", ErrorLevel.Error, ClsLog.ProjectName);
                return new string[0];
            }   
        }

        public static string[] GetCollectionNames()
        {
            try
            {

                MongoClient client = new MongoClient(ConnText);
                MongoServer server = client.GetServer();

                MongoDatabase db = server.GetDatabase(DbName);
                var s = db.GetCollectionNames();
                return s.ToArray();
            }
            catch (Exception ex)
            {
                ClsLog.AppendDbLog(ex.Message, "GetCollectionNamesWithoutParameters", ErrorLevel.Error, ClsLog.ProjectName);
                return new string[0];
            }
            
        }

        private MongoDatabase GetDatabase()
        {
            MongoClient client = new MongoClient(ConnText);
            MongoServer server = client.GetServer();
            return server.GetDatabase(DbName);
        }

        public List<T> Find(IMongoQuery query)
        {
            MongoCollection col = GetCollection();
            if (col == null) return null;
            MongoCursor<T> curs = col.FindAs<T>(query);

            if (curs != null && curs.Any()) return curs.ToList<T>();
            return null;
        }

        public List<T> Find(IMongoQuery query, int count)
        {
            MongoCollection col = GetCollection();
            if (col == null) return null;
            MongoCursor<T> curs = col.FindAs<T>(query);

            if (curs != null && curs.Any()) return curs.Take(count).ToList<T>();
            return null;
        }

        public List<T> Find(IMongoQuery query, int skipSize, int pageCount)
        {
            MongoCollection col = GetCollection();
            if (col == null) return null;
            MongoCursor<T> curs = col.FindAs<T>(query);

            if (curs != null && curs.Any()) return curs.Skip(skipSize).Take(pageCount).ToList<T>();
            return null;
        }

        public List<T> Find(IMongoQuery query, IMongoSortBy sortBy, string indexName)
        {
            MongoCollection col = GetCollection();
            if (col == null) return null;
            MongoCursor<T> curs;
            if (sortBy != null)
                curs = !string.IsNullOrWhiteSpace(indexName) ? col.FindAs<T>(query).SetHint(indexName).SetSortOrder(sortBy) : col.FindAs<T>(query).SetSortOrder(sortBy);
            else
                curs = !string.IsNullOrWhiteSpace(indexName) ? col.FindAs<T>(query).SetHint(indexName) : col.FindAs<T>(query);
            if (curs != null && curs.Any()) return curs.ToList<T>();
            return null;
        }

        public bool Update(IMongoQuery query, IMongoUpdate update, bool isMutile = false)
        {
            MongoCollection col = GetCollection();
            if (col == null) return false;
            CommandResult result;
            if (!isMutile) result = col.FindAndModify(query, SortBy.Null, update);
            else result = col.Update(query, update, UpdateFlags.Multi);

            return result != null && result.Ok;
        }

        public bool Delete(IMongoQuery query)
        {
            MongoCollection col = GetCollection();
            if (col == null) return false;
            WriteConcernResult result = col.Remove(query);
            return result != null && result.Ok;
        }

        public bool Drop()
        {
            MongoCollection col = GetCollection();
            if (col == null) return false;
            var result = col.Drop();
            return result != null && result.Ok;
        }

        public bool Insert(T item, string[] uniqueIndexNames, string[] notUniqueIndexNames)
        {
            MongoCollection col = GetCollection();
            if (col == null) return false;

            WriteConcernResult result = col.Insert(item);

            CollectionStatsResult.IndexSizesResult index = col.GetStats().IndexSizes;

            
            if (index.Count <= 1)
            {
                if (uniqueIndexNames != null)
                {
                    foreach (var uniqueIndexName in uniqueIndexNames)
                    {
                        if (!col.IndexExistsByName(uniqueIndexName))
                        {
                            IMongoIndexKeys keys = new IndexKeysBuilder().Ascending(uniqueIndexName);
                            col.EnsureIndex(keys, IndexOptions.SetUnique(true));
                        }
                    }
                }

                if (notUniqueIndexNames != null)
                {
                    foreach (var notUniqueIndexName in notUniqueIndexNames)
                    {
                        if (!col.IndexExistsByName(notUniqueIndexName))
                        {
                            IMongoIndexKeys keys = new IndexKeysBuilder().Ascending(notUniqueIndexName);
                            col.EnsureIndex(keys, IndexOptions.SetUnique(false));
                        }
                    }
                }
            }
            return (result != null && result.Ok);
        }

        //public bool Insert(T item, string indexName, params string[] key)
        //{
        //    MongoCollection col = GetCollection();
        //    if (col == null) return false;

        //    WriteConcernResult result = col.Insert(item);

        //    CollectionStatsResult.IndexSizesResult index = col.GetStats().IndexSizes;
        //    //if (indexName.Length > 0 && !col.IndexExistsByName(indexName))
        //    if (key.Length > 0 && index.Count <= 1)
        //    {
        //        foreach (var keyName in key)
        //        {
        //            if (!col.IndexExistsByName(keyName))
        //            {
        //                IMongoIndexKeys keys = new IndexKeysBuilder().Ascending(keyName);
        //                col.EnsureIndex(keys);
        //            }
        //        }
                
        //        //    col.EnsureIndex(IndexKeys.Descending(indexName), IndexOptions.SetName(indexName));
        //    }

        //    return (result != null && result.Ok);
        //}

        public bool InsertBatch(List<T> items)
        {
            MongoCollection col = GetCollection();
            if (col == null) return false;
            col.InsertBatch(items);
            return true;
        }
    }
}
