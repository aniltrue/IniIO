/*
 * Author: Anıl Doğru
 */

using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace IniIO
{
    public class Key
    {
        public static readonly char EQUALS = '=';

        private String name;
        private Object value;
        private Type valueType;

        public String Name
        {
            get
            {
                return name;
            }
            internal set
            {
                if (name != null)
                    name = value;
                else
                    name = "";
            }
        }

        public Object Value
        {
            get
            {
                return value;
            }
            set
            {
                if (value == null)
                {
                    this.value = "";
                    valueType = typeof(string);
                    return;
                }

                valueType = GetValueType(value);
                this.value = value;
            }
        }

        public static Type GetValueType(Object value)
        {
            if (value is string)
                return typeof(string);

            if (value is char)
                return typeof(char);

            if (value is int)
                return typeof(int);

            if (value is long)
                return typeof(long);

            if (value is float)
                return typeof(float);

            if (value is double)
                return typeof(double);

            throw new ArgumentException("Invalid value type");
        }

        public Type Type
        {
            get
            {
                return valueType;
            }
        }

        #region Constructors

        public Key(String name, Object value)
        {
            if (name == null)
                throw new ArgumentNullException("Name cannot be null");

            this.name = name;
            Value = value;
        }

        public Key(String line)
        {
            if (!line.Contains(EQUALS))
                throw new ArgumentException("The line is not a Key line");

            int index = line.IndexOf(EQUALS);
            name = line.Substring(0, index - 1).Trim();

            String valuePart = line.Substring(index + 1).Trim();
            int numericValue = 0;
            double floatValue = 0.0D;

            if (valuePart.StartsWith("\"") && valuePart.EndsWith("\""))
                Value = valuePart.Substring(1, valuePart.Length - 1);

            else if (valuePart.StartsWith("\'") && valuePart.EndsWith("\'"))
                Value = valuePart.Substring(1, valuePart.Length - 1)[0];

            else if (int.TryParse(valuePart, out numericValue))
                Value = numericValue;

            else if (double.TryParse(valuePart, out floatValue))
                Value = numericValue;

            else
                throw new ArgumentException("Invalid value type");
        }

        public Key(Object[] array)
        {
            if (array.Length != 2)
                throw new ArgumentException("Array's length must be 2");

            name = array[0].ToString();
            Value = array[1];
        }

        public Key(KeyValuePair<String, Object> keyValuePair)
        {
            name = keyValuePair.Key;
            if (name == null)
                throw new ArgumentNullException("Name cannot be null");

            Value = keyValuePair.Value;
        }

        public Key(DataRow dataRow)
        {
            if (dataRow == null)
                throw new ArgumentNullException("Datarow cannot be null");

            if (!dataRow.Table.Columns.Contains("Name") || !dataRow.Table.Columns.Contains("Value"))
                throw new ArgumentException("Datarow has not Name or Value columns");

            name = dataRow["Name"].ToString();
            Value = dataRow["Value"].ToString();
        }

        #endregion

        #region Converts

        public String ToLine()
        {
            String valueText = value.ToString();
            if (valueType == typeof(string))
                valueText = "\"" + value.ToString() + "\"";

            if (valueType == typeof(char))
                valueText = "\'" + value.ToString() + "\'";

            return Name + " " + EQUALS + " " + valueText;
        }

        public String[] ToArray()
        {
            String[] array = new String[2];
            array[0] = Name;
            array[1] = Value.ToString();

            return array;
        }

        public KeyValuePair<String, Object> ToKeyValuePair()
        {
            return new KeyValuePair<string, object>(Name, Value);
        }

        public DataRow ToDataRow(DataTable moduleDataTable)
        {
            if (moduleDataTable == null)
                throw new ArgumentNullException("DataTable cannot be null");

            if (!moduleDataTable.Columns.Contains("Name") || !moduleDataTable.Columns.Contains("Value"))
                throw new ArgumentException("Datarow has not Name or Value columns");

            DataRow dataRow = moduleDataTable.NewRow();
            dataRow["Name"] = Name;
            dataRow["Value"] = Value;

            return dataRow;
        }

        public String ToJSONObject()
        {
            return "\"" + name + "\" : \"" + value.ToString() + "\"";
        }

        #endregion

        #region Override
        public override bool Equals(object obj)
        {
            if (!(obj is Key))
                return false;

            Key key = (Key)obj;

            return Name.Equals(key.Name) && Value.Equals(key.Value);
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override int GetHashCode()
        {
            return String.Format("{0}={1}", name, value).GetHashCode();
        }
        #endregion
    }
}
