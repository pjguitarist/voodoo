﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Voodoo
{
    public static class ReflectionExtensions
    {
        public static bool IsScalar(this Type t)
        {
            const string types = "string,guid,datetime,timespan";
            if (t.Name.ToLower().Contains("nullable"))
                return true;

            if (types.Contains(t.Name.ToLower()))
                return true;

            if (t.IsPrimitive)
                return true;

            if (t.IsEnum)
                return true;

            return false;
        }

        public static string GetParameters(this MethodInfo methodInfo)
        {
            var result = string.Empty;
            foreach (var info in methodInfo.GetParameters())
            {
                result += info.ParameterType.FixUpTypeName();
                result += " ";
                result += info.Name;
                result += ", ";
            }

            result = result.TrimEnd(' ').TrimEnd(',');
            return result;
        }

        public static string FixUpType(this Type t)
        {
            var type = t.Name;
            type = type.Replace("System.", "");

            switch (type)
            {
                case "String":
                    return "string";
                case "Byte":
                    return "byte";
                case "Byte[]":
                    return "byte[]";
                case "Int16":
                    return "short";
                case "Int32":
                    return "int";
                case "Int64":
                    return "long";
                case "Char":
                    return "char";
                case "Single":
                    return "float";
                case "Double":
                    return "double";
                case "Boolean":
                    return "bool";
                case "Decimal":
                    return "decimal";
                case "SByte":
                    return "sbyte";
                case "UInt16":
                    return "ushort";
                case "UInt32":
                    return "uint";
                case "UInt64":
                    return "ulong";
                case "Object":
                    return "object";
                default:

                    return type;
            }
        }

        public static string FixUpTypeName(this Type type)
        {
            var result = type.FixUpType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
            {
                result = string.Format("{0}?", Nullable.GetUnderlyingType(type).FixUpType());
            }

            else if (type.IsGenericType)
            {
                var inner = string.Empty;
                foreach (var t in type.GetGenericArguments())
                {
                    if (t.IsGenericType)
                    {
                        var outer1 = t.GetGenericTypeDefinition().Name;
                        var ary1 = outer1.Split(@"`".ToCharArray());
                        outer1 = ary1[0];

                        var inner1 = string.Empty;
                        foreach (var t1 in t.GetGenericArguments())
                        {
                            inner1 += t1.Name;
                            inner1 += ",";
                        }
                        inner1 = inner1.TrimEnd(",".ToCharArray());
                        inner += string.Format("{1}<{0}>", inner1, outer1);
                    }
                    else
                    {
                        inner += t.Name;
                        inner += ",";
                    }
                }
                inner = inner.TrimEnd(",".ToCharArray());
                var name = type.GetGenericArguments()[0].FixUpType();
                var outer = type.GetGenericTypeDefinition().Name;
                var ary = outer.Split(@"`".ToCharArray());
                outer = ary[0];
                result = string.Format("{1}<{0}>", inner, outer);
            }
            else
            {
                return result;
            }
            return result;
        }

        public static string GetMethodName(this MethodBase input)
        {
            return input.Name;
        }

        public static bool DoesImplementInterfaceOf(this Type type, Type interfaceType)
        {
            return type.GetInterfaces().Contains(interfaceType);
        }

        public static bool IsGenericCollectionTypeOf(this Type type, Type typeDefinition)
        {
            if (type.IsGenericType)
            {
                var collectionTypeInterfaces = new[] {typeof (IEnumerable), typeof (IList), typeof (ICollection)};
                var isCollectionType = type.GetInterfaces().Intersect(collectionTypeInterfaces).Any();
                var canConstructTypeDefinition =
                    type.GetGenericArguments().Any(c => c.GetInterfaces().Contains(typeDefinition));

                if (isCollectionType && canConstructTypeDefinition)
                    return true;
            }
            return false;
        }

        /* Unity Stuff
        public string GetCallingSignature(IMethodInvocation input)
        {
            string className = GetTypeName(input.MethodBase.DeclaringType);
            string methodName = GetMethodName(input);
            string arguments = GetArgumentList(input.Arguments);

            string preMethodMessage = string.Format("{0}.{1}({2})", className, methodName, arguments);
            return preMethodMessage;
        }
        public string GetResultSignature(IMethodInvocation input, IMethodReturn msg)
        {

            string className = GetTypeName(input.MethodBase.DeclaringType);
            string methodName = GetMethodName(input);
            string postMethodMessage = string.Format("result={0}.{1}() -> {2}", className, methodName, msg.ReturnValue);
            return postMethodMessage;
        }

        public IMethodReturn TraceInvoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
        {
            string className = GetTypeName(input.MethodBase.DeclaringType);
            string methodName = GetMethodName(input);
            string arguments = GetArgumentList(input.Arguments);

            string preMethodMessage = string.Format("call={0}.{1}({2})", className, methodName, arguments);
            System.Diagnostics.Debug.WriteLine(preMethodMessage);
            IMethodReturn msg = null;

            // Call the method that was intercepted 
            msg = getNext()(input, getNext);

            string postMethodMessage = string.Format("result={0}.{1}() -> {2}", className, methodName, msg.ReturnValue);
            System.Diagnostics.Debug.WriteLine(postMethodMessage);

            return msg;
        }
       
        public void DecorateLog(ref Log log, IMethodInvocation input, string category, Exception ex = null)
        {
            string className = GetTypeName(input.MethodBase.DeclaringType);
            string methodName = GetMethodName(input);
            string arguments = GetArgumentList(input.Arguments);

            log.Time = DateTime.Now;
            log.Category = category;
            log.MethodCall = string.Format("{0}.{1}({2})", className, methodName, arguments);
            log.Action = methodName;
            if (ex != null)
                log.Exception = ex.ToString();

            List<AppParameterInformation> objects = new List<AppParameterInformation>();
            ParameterInfo[] paramNames = input.MethodBase.GetParameters();
            List<Type> parameterTypes = new List<Type>();
            int cnt = 0;

            foreach (var o in input.Arguments)
            {


                if (o != null)
                {
                    Type t = o.GetType();

                    if (CanSerialize(t))
                    {
                        parameterTypes.Add(t);
                        objects.Add(new AppParameterInformation() { ParamaterName = paramNames[cnt].Name, ParamaterValue = o });
                    }
                    else
                    {
                        objects.Add(new AppParameterInformation() { ParamaterName = paramNames[cnt].Name, ParamaterValue = null });
                    }
                }
                else
                {
                    objects.Add(new AppParameterInformation() { ParamaterName = paramNames[cnt].Name, ParamaterValue = null });
                }
                cnt += 1;
            }

            log.Parameters = objects.ToDataContractXml(objects.GetType(), parameterTypes.ToArray());



        }*/
    }
}