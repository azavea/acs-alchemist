using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using log4net;
using Newtonsoft.Json.Linq;

namespace Azavea.NijPredictivePolicing.Common
{
    /// <summary>
    /// A super basic config class
    /// </summary>
    public class Config
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected Dictionary<string, object> _data = null;
        protected string _filename;

        public Config()
        {
            _data = new Dictionary<string, object>();
        }

        public Config(string filename)
        {
            Load(filename);
        }

        public bool Load(string filename)
        {
            _filename = filename;
            _data = null;
            if (File.Exists(filename))
            {
                string fileContents = File.ReadAllText(filename);
                if (!string.IsNullOrEmpty(fileContents))
                {
                    JsonSerializerSettings s = new JsonSerializerSettings();
                    s.TypeNameHandling = TypeNameHandling.All;
                    s.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

                    _data = (Dictionary<string, object>)JsonConvert.DeserializeObject(fileContents, s);

                    return (_data != null) && (_data is Dictionary<string, object>);
                }
                else
                {
                    _log.ErrorFormat("File was empty: {0}", filename);
                }
            }
            else
            {
                _log.ErrorFormat("Could not find file: {0}", filename);
            }

            return false;
        }

        public bool Save()
        {
            return Save(string.Empty);
        }

        public bool Save(string filename)
        {
            try
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    _filename = filename;
                }

                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                JsonSerializerSettings s = new JsonSerializerSettings();
                s.TypeNameHandling = TypeNameHandling.All;
                s.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                
                string fileContents = JsonConvert.SerializeObject(this._data, Formatting.Indented, s);
                File.WriteAllText(_filename, fileContents);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error saving configuration", ex);
            }
            return false;
        }

        public string SaveToMemory()
        {
            try
            {
                return JsonConvert.SerializeObject(this._data, Formatting.Indented);
            }
            catch (Exception ex)
            {
                _log.Error("Error saving configuration", ex);
            }
            return string.Empty;
        }

        public T Get<T>(string key, T ifEmpty)
        {
            if ((_data != null) && (_data.ContainsKey(key)))
            {
                return Utilities.GetAs<T>(_data[key], ifEmpty);
            }
            return ifEmpty;
        }

        public List<object> GetList(string key)
        {
            
            if (_data != null) //&& (_data.ContainsKey(key)))
            {
                var o = _data[key];

                //if (o is JArray)
                //{
                    //return (o as JArray);
                    return new List<object>(o as IEnumerable<object>);
                //}
                //else if (o is Array)
                //{
                //    return new List<object>(o as IEnumerable<object>);                    
                //}
            }
            return null;
        }

        public object this[string key]
        {
            get
            {
                return this.Get<object>(key, null);
            }
        }

        public void Set<T>(string key, T value)
        {
            if (_data == null)
            {
                _data = new Dictionary<string, object>();
            }

            if (_data != null)
            {
                _data[key] = value;
            }
        }

        public Dictionary<string, object>.KeyCollection Keys
        {
            get
            {
                return (_data != null) ? _data.Keys : null;
            }
        }

        public bool IsEmpty()
        {
            return ((_data == null) || (_data.Count == 0));
        }
    }
}
