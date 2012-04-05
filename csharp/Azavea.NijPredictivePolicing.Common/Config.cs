/*
  Copyright (c) 2012 Azavea, Inc.
 
  This file is part of _SOLUTIONNAME_.

  _SOLUTIONNAME_ is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  _SOLUTIONNAME_ is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with _SOLUTIONNAME_.  If not, see <http://www.gnu.org/licenses/>.
*/

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

        public string GetFilename()
        {
            return _filename;
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
            catch (UnauthorizedAccessException cantWriteEx)
            {
                _log.Error("The importer couldn't write its config file.  Please run this application as an Administrator");
                _log.Fatal("The importer cannot continue.  Exiting...");
                Environment.Exit(-1);
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
                return new List<object>(o as IEnumerable<object>);
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
