using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Celin
{
    public class StorageService
    {
        string StoragePath { get; }
        string StorageDataPath { get; }
        public StreamWriter Create(string fname) => File.CreateText(Path.Combine(StoragePath, fname));
        public StreamWriter CreateData(string fname) => File.CreateText(Path.Combine(StorageDataPath, fname));
        public IEnumerable<string> ListFiles => Directory.EnumerateFiles(StoragePath);
        public StreamReader Open(string fname) => File.OpenText(Path.Combine(StoragePath, fname));
        public StreamReader OpenData(string fname) => File.OpenText(Path.Combine(StorageDataPath, fname));
        public bool Delete(string fname)
        {
            try
            {
                File.Delete(Path.Combine(StoragePath, fname));
                return true;
            }
            catch { }
            return false;
        }
        public bool DeleteData(string fname)
        {
            try
            {
                File.Delete(Path.Combine(StorageDataPath, fname));
                return true;
            }
            catch { }
            return false;
        }
        public StorageService(IConfiguration config)
        {
            StoragePath = config["storagePath"];
            StorageDataPath = Path.Combine(StoragePath, "data");
        }
    }
}
