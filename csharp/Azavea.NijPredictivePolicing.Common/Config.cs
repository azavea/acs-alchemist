using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using log4net;

namespace Azavea.NijPredictivePolicing.Common
{
    /// <summary>
    /// A super basic config class
    /// </summary>
    public class Config
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //public Dictionary<string, object> _storage = new Dictionary<string, object>();
        protected JavaScriptObject _data = null;
        protected string _filename;

        public Config()
        {
            _data = new JavaScriptObject();
        }

        public Config(string filename)
        {
            Load(filename);
        }

        public bool Load(string filename)
        {
            _filename = filename;
            //_storage = new Dictionary<string, object>();
            _data = null;
            if (File.Exists(filename))
            {
                string fileContents = File.ReadAllText(filename);
                if (!string.IsNullOrEmpty(fileContents))
                {
                    _data = (JavaScriptObject)JavaScriptConvert.DeserializeObject(fileContents);
                    return (_data != null) && (_data is JavaScriptObject);
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

        public bool Save(string filename)
        {
            try
            {
                _filename = filename;

                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                string fileContents = JavaScriptConvert.SerializeObject(this._data, null);
                File.WriteAllText(filename, fileContents);
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
                return JavaScriptConvert.SerializeObject(this._data);
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
            if ((_data != null) && (_data.ContainsKey(key)))
            {
                var o = _data[key];
                if (o is JavaScriptArray)
                {
                    return (o as JavaScriptArray);
                }
                else if (o is Array)
                {
                    return new List<object>(o as IEnumerable<object>);
                    
                }
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
            if (_data != null)
            {
                _data[key] = value;
            }
        }



    }
}
