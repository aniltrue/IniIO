/*
 * Author: Anıl Doğru
 */

using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

namespace IniIO
{
    public class Module : IList<Key>
    {
        public readonly static char MODULE_START = '[';
        public readonly static char MODULE_END = ']';

        private Key[] keys;
        private String name;

        public String Name
        {
            get
            {
                return name;
            }
            internal set
            {
                if (value != null)
                    name = value;
                else
                    name = "";
            }
        }

        #region Constructors

        public Module(String name)
        {
            Name = name;
            keys = new Key[0];
        }

        public Module(String name, Key[] array) : this(name, (ICollection<Key>)array)
        {
            ;
        }

        public Module(String name, List<Key> keys) : this(name, (ICollection<Key>)keys)
        {
            ;
        }

        public Module(DataTable dataTable) : this(dataTable.TableName)
        {
            foreach (DataRow dataRow in dataTable.Rows)
                Add(new Key(dataRow));
        }

        public Module(String name, ICollection<Key> collection) : this(name)
        {
            foreach (Key key in collection)
                Add(key);
        }

        #endregion

        #region Converts 

        public String ToText()
        {
            String text = MODULE_START + Name + MODULE_END;

            foreach (Key key in keys)
                text += '\n' + key.ToLine();

            return text;
        }

        public Key[] ToArray()
        {
            return keys;
        }

        public List<Key> ToList()
        {
            List<Key> list = new List<Key>();

            foreach (Key key in keys)
                list.Add(key);

            return list;
        }

        public DataTable ToDataTable()
        {
            DataTable dataTable = EmptyModule(Name);

            foreach (Key key in keys)
                dataTable.Rows.Add(key.ToDataRow(dataTable));

            return dataTable;
        }

        public String ToJSONObject()
        {
            String jsonText = "";
            
            foreach (Key key in keys)
            {
                if (jsonText == string.Empty)
                    jsonText += key.ToJSONObject();
                else
                    jsonText += ",\n" + key.ToJSONObject();
            }

            return "\"" + name + "\" : {\n" + jsonText + "\n}";
        }

        public static Module ParseText(String text)
        {
            if (text == null)
                throw new ArgumentNullException();
            if (text.Equals(""))
                throw new ArgumentException("Text is empty");

            String[] lines = text.Split('\n');
            String name = lines[0].Trim();
            if (!name.StartsWith(MODULE_START.ToString()) || !name.EndsWith(MODULE_END.ToString()))
                throw new ArgumentException("Text is not an module");

            name = name.Substring(1, name.Length - 1);

            List<Key> keys = new List<Key>();
            for (int i = 1; i < lines.Length; i++)
                keys.Add(new Key(lines[i]));

            return new Module(name, keys);
        }

        public static DataTable EmptyModule(String moduleName)
        {
            DataTable dataTable = new DataTable(moduleName);
            dataTable.Columns.Add(new DataColumn("Name", typeof(string), "Name of Key"));
            dataTable.Columns.Add(new DataColumn("Value", typeof(string), "Value of Key"));

            return dataTable;
        }

        #endregion

        #region Operations

        public int Count
        {
            get
            {
                return keys.Length;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public Key this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                return keys[index];
            }

            set
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                if (value.Name.Equals(keys[index].Name))
                    keys[index] = value;
                else if (Contains(value.Name))
                    throw new DuplicateNameException("Key names must be unique");
                else
                    keys[index] = value;
            }
        }

        public Key this[String name]
        {
            get
            {
                int index = IndexOf(name);
                if (index != -1)
                    return keys[index];

                throw new KeyNotFoundException();
            }

            set
            {
                int index = IndexOf(name);
                if (index == -1)
                    throw new KeyNotFoundException();

                if (value.Name.Equals(name))
                    keys[index] = value;
                else if (Contains(value.Name))
                    throw new DuplicateNameException("Key names must be unique");
                else
                    keys[index] = value;
            }
        }

        public void Add(Key item)
        {
            Insert(Count, item);
        }

        public void Insert(int index, Key item)
        {
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            if (Contains(item.Name))
                throw new DuplicateNameException("Key names must be unique");

            Array.Resize(ref keys, Count + 1);

            for (int i = Count - 1; i > index; i--)
                keys[i] = keys[i - 1];

            keys[index] = item;
        }

        public bool Contains(Key item)
        {
            return IndexOf(item) != -1;
        }

        public bool Contains(String keyName)
        {
            return IndexOf(keyName) != -1;
        }

        public void Clear()
        {
            keys = new Key[0];
        }

        public IEnumerator<Key> GetEnumerator()
        {
           List<Key> list = new List<Key>();
            foreach (Key key in keys)
                list.Add(key);

            return list.GetEnumerator();
        }

        public bool Remove(Key item)
        {
            int index = IndexOf(item);
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        public bool Remove(String keyName)
        {
            int index = IndexOf(keyName);
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        public int IndexOf(Key item)
        {
            for (int i = 0; i < Count; i++)
                if (item.Equals(keys[i]))
                    return i;

            return -1;
        }

        public int IndexOf(String keyName)
        {
            for (int i = 0; i < Count; i++)
                if (keys[i].Name.Equals(keyName))
                    return i;

            return -1;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            for (int i = index; i < Count - 1; i++)
                keys[i] = keys[i + 1];

            Array.Resize(ref keys, Count - 1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return keys.GetEnumerator();
        }

        public void CopyTo(Key[] array, int arrayIndex)
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


        public void ChangeKeyName(String oldName, String newName)
        {
            ChangeKeyName(this[oldName], newName);
        }

        public void ChangeKeyName(Key key, String newName)
        {
            if (Contains(newName))
                throw new DuplicateNameException("Key names must be unique");

            if (Contains(key))
                throw new KeyNotFoundException();

            this[key.Name].Name = newName;
        }
        #endregion

        #region Override

        public override bool Equals(object obj)
        {
            if (!(obj is Module))
                return false;

            Module module = (Module)obj;

            if (!module.Name.Equals(Name))
                return false;

            if (module.Count != Count)
                return false;

            for (int i = 0; i < Count; i++)
                if (!module[i].Equals(this[i]))
                    return false;

            return true;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return string.Format("{0}-{1}", Name, Count).GetHashCode();
        }

        #endregion
    }
}
