/*
 * Author: Anıl Doğru
 */

using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace IniIO
{
    public class IniFile : IList<Module>
    {
        private Module[] modules;
        private bool isLoaded;

        public bool IsLoaded
        {
            get
            {
                return isLoaded;
            }
        }

        #region Constructors

        public IniFile()
        {
            modules = new Module[0];
            isLoaded = false;
        }

        public IniFile(String filePath)
        {
            ReadFromFile(filePath);
        }

        public IniFile(Module[] array) : this((ICollection<Module>)array)
        {
            ;
        }

        public IniFile(List<Module> modules) : this((ICollection<Module>)modules)
        {
            ;
        }

        public IniFile(DataSet dataSet)
        {
            isLoaded = false;

            foreach (DataTable dataTable in dataSet.Tables)
                Add(new Module(dataTable));

            isLoaded = true;
        }

        public IniFile(ICollection<Module> collection)
        {
            isLoaded = false;

            foreach (Module module in collection)
                Add(module);

            isLoaded = true;
        }

        #endregion

        #region Converts

        public String ToText()
        {
            String text = "";
            foreach (Module module in modules)
                text += module.ToText() + "\n\n";

            return text.Trim();
        }

        public Module[] ToArray()
        {
            return modules;
        }

        public List<Module> ToList()
        {
            List<Module> list = new List<Module>();

            foreach (Module module in modules)
                list.Add(module);

            return list;
        }

        public DataSet ToDataSet()
        {
            DataSet dataSet = new DataSet("IniFile");

            foreach (Module module in modules)
                dataSet.Tables.Add(module.ToDataTable());

            return dataSet;
        }

        public String ToJSON()
        {
            String jsonText = "";

            foreach (Module module in modules)
            {
                if (jsonText == string.Empty)
                    jsonText += module.ToJSONObject();
                else
                    jsonText += ",\n" + module.ToJSONObject();
            }

            return "{\n" + jsonText + "\n}";
        }

        #endregion

        #region FileOperations

        public void ReadFromFile(String filePath)
        {
            isLoaded = false;
            Clear();

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            StreamReader reader = new StreamReader(filePath);
            String text = reader.ReadToEnd();
            reader.Close();

            ReadFromText(text);
        }

        public void ReadFromText(String text)
        {
            isLoaded = false;
            Clear();

            String[] lines = text.Split('\n');
            Module module = null;

            foreach (String line in lines)
                ParseLine(ref module, line);

            if (module != null)
                Add(module);

            isLoaded = true;
        }

        private void ParseLine(ref Module module, String line)
        {
            if (line.StartsWith(Module.MODULE_START.ToString()) && line.EndsWith(Module.MODULE_END.ToString()))
            {
                if (module != null)
                    Add(module);

                String moduleName = line.Substring(1, line.Length - 1);
                module = new Module(moduleName);
            }

            if (line.Contains(Key.EQUALS) && module != null)
            {
                int index = line.IndexOf(Key.EQUALS);
                String keyName = line.Substring(0, index - 1);
                String keyValue = line.Substring(index + 1);

                module.Add(new Key(keyName, keyValue));
            }
        }

        public void SaveToFile(String filePath)
        {
            String text = ToText();
            StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine(text);
            writer.Close();
        }

        public static IniFile ParseText(String text)
        {
            IniFile iniFile = new IniFile();
            iniFile.ReadFromText(text);

            return iniFile;
        }

        #endregion

        #region Operations

        public Module this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                return modules[index];
            }

            set
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                if (value.Name.Equals(modules[index].Name))
                    modules[index] = value;
                else if (Contains(value.Name))
                    throw new DuplicateNameException("Module names must be unique");
                else
                    modules[index] = value;
            }
        }

        public Module this[String name]
        {
            get
            {
                int index = IndexOf(name);
                if (index != -1)
                    return modules[index];

                throw new KeyNotFoundException();
            }

            set
            {
                int index = IndexOf(name);
                if (index == -1)
                    throw new KeyNotFoundException();

                if (value.Name.Equals(name))
                    modules[index] = value;
                else if (Contains(value.Name))
                    throw new DuplicateNameException("Module names must be unique");
                else
                    modules[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return modules.Length;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(Module item)
        {
            Insert(Count, item);
        }

        public void Insert(int index, Module item)
        {
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            if (Contains(item.Name))
                throw new DuplicateNameException("Module names must be unique");

            Array.Resize(ref modules, Count + 1);

            for (int i = Count - 1; i > index; i--)
                modules[i] = modules[i - 1];

            modules[index] = item;
        }

        public bool Contains(Module item)
        {
            return IndexOf(item) != -1;
        }

        public bool Contains(String moduleName)
        {
            return IndexOf(moduleName) != -1;
        }

        public void Clear()
        {
            modules = new Module[0];
        }

        public IEnumerator<Module> GetEnumerator()
        {
            List<Module> list = new List<Module>();
            foreach (Module module in modules)
                list.Add(module);

            return list.GetEnumerator();
        }

        public bool Remove(Module item)
        {
            int index = IndexOf(item);
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        public bool Remove(String moduleName)
        {
            int index = IndexOf(moduleName);
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        public int IndexOf(Module item)
        {
            for (int i = 0; i < Count; i++)
                if (item.Equals(modules[i]))
                    return i;

            return -1;
        }

        public int IndexOf(String moduleName)
        {
            for (int i = 0; i < Count; i++)
                if (modules[i].Name.Equals(moduleName))
                    return i;

            return -1;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            for (int i = index; i < Count - 1; i++)
                modules[i] = modules[i + 1];

            Array.Resize(ref modules, Count - 1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return modules.GetEnumerator();
        }

        public void CopyTo(Module[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("Array cannot be null");
            if (arrayIndex < 0 || array.Length - arrayIndex < Count)
                throw new ArgumentOutOfRangeException();
            if (array.Rank > 1)
                throw new ArgumentException("Array is multidimensional");

            for (int i = 0; i < Count; i++)
                array.SetValue(this[i], arrayIndex + i);
        }

        public void ChangeModuleName(String oldName, String newName)
        {
            ChangeModuleName(this[oldName], newName);
        }

        public void ChangeModuleName(Module module, String newName)
        {
            if (Contains(newName))
                throw new DuplicateNameException("Module names must be unique");

            if (Contains(module))
                throw new KeyNotFoundException();

            this[module.Name].Name = newName;
        }

        #endregion

        #region Override

        public override bool Equals(object obj)
        {
            if (!(obj is IniFile))
                return false;

            IniFile iniFile = (IniFile)(obj);

            if (Count != iniFile.Count)
                return false;

            for (int i = 0; i < Count; i++)
                if (!this[i].Equals(iniFile[i]))
                    return false;

            return true;
        }

        public override string ToString()
        {
            return ToText();
        }

        public override int GetHashCode()
        {
            String text = "";
            foreach (Module module in modules)
            {
                if (text.Equals(""))
                    text += module.Name;
                else
                    text += "-" + module.Name;
            }

            return text.GetHashCode();
        }

        #endregion
    }
}
