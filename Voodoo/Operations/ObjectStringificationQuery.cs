﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Voodoo.Messages;

namespace Voodoo.Operations
{
    //http://stackoverflow.com/questions/852181/c-printing-all-properties-of-an-object
    public class ObjectStringificationQuery : Query<object, TextResponse>
    {
        private readonly List<int> hashes;
        private readonly int padding;
        private readonly StringBuilder result;
        private int depth;

        public ObjectStringificationQuery(object request) : base(request)
        {
            padding = 5;
            result = new StringBuilder();
            hashes = new List<int>();
        }

        protected override TextResponse ProcessRequest()
        {
            response.Text = read(request);
            return response;
        }

        private string read(object element)
        {
            if (element == null || element is ValueType || element is string)
            {
                write(format(element));
            }
            else
            {
                var objectType = element.GetType();
                if (!typeof (IEnumerable).IsAssignableFrom(objectType))
                {
                    write("{{{0}}}", objectType.FullName);
                    hashes.Add(element.GetHashCode());
                    depth++;
                }

                var enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {
                    foreach (var item in enumerableElement)
                    {
                        if (item is IEnumerable && !(item is string))
                        {
                            depth++;
                            read(item);
                            depth--;
                        }
                        else
                        {
                            if (!alreadyTouched(item))
                                read(item);
                            else
                                write("{{{0}}} <-- bidirectional reference found", item.GetType().FullName);
                        }
                    }
                }
                else
                {
                    var members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var memberInfo in members)
                    {
                        var fieldInfo = memberInfo as FieldInfo;
                        var propertyInfo = memberInfo as PropertyInfo;

                        if (fieldInfo == null && propertyInfo == null)
                            continue;

                        var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
                        var value = fieldInfo != null
                            ? fieldInfo.GetValue(element)
                            : propertyInfo.GetValue(element, null);

                        if (type.IsValueType || type == typeof (string))
                        {
                            write("{0}: {1}", memberInfo.Name, format(value));
                        }
                        else
                        {
                            var isEnumerable = typeof (IEnumerable).IsAssignableFrom(type);
                            write("{0}: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }");

                            var alreadyTouched = !isEnumerable && this.alreadyTouched(value);
                            depth++;
                            if (!alreadyTouched)
                                read(value);
                            else
                                write("{{{0}}} <-- bidirectional reference found", value.GetType().FullName);
                            depth--;
                        }
                    }
                }

                if (!typeof (IEnumerable).IsAssignableFrom(objectType))
                {
                    depth--;
                }
            }

            return result.ToString();
        }

        private bool alreadyTouched(object value)
        {
            if (value == null)
                return false;

            var hash = value.GetHashCode();
            return hashes.Contains(hash);
            return false;
        }

        private void write(string value, params object[] args)
        {
            var space = new string(' ', depth*padding);

            if (args != null)
                value = string.Format(value, args);

            result.AppendLine(space + value);
        }

        private string format(object o)
        {
            if (o == null)
                return ("null");

            if (o is DateTime)
                return (((DateTime) o).ToShortDateString());

            if (o is string)
                return string.Format("\"{0}\"", o);

            if (o is char && (char) o == '\0')
                return string.Empty;

            if (o is ValueType)
                return (o.ToString());

            if (o is IEnumerable)
                return ("...");

            return ("{ }");
        }
    }
}