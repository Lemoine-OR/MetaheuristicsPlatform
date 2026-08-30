using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MetaheuristicsPlatform.Tests;

internal static class V1PublicApiSurface
{
    internal static string[] Capture(Assembly assembly)
    {
        var signatures = new List<string>();

        foreach (var type in assembly.GetExportedTypes().OrderBy(FormatType, StringComparer.Ordinal))
        {
            var typeName = FormatType(type);
            var kind =
                type.IsInterface ? "interface" :
                type.IsEnum ? "enum" :
                typeof(MulticastDelegate).IsAssignableFrom(type.BaseType) ? "delegate" :
                type.IsValueType ? "struct" :
                "class";

            signatures.Add(
                "TYPE|" + typeName +
                "|kind=" + kind +
                "|base=" + (type.BaseType is null ? "" : FormatType(type.BaseType)));

            foreach (var implemented in type.GetInterfaces().OrderBy(FormatType, StringComparer.Ordinal))
            {
                signatures.Add("IMPLEMENTS|" + typeName + "|" + FormatType(implemented));
            }

            foreach (var constraint in FormatGenericConstraints(type.GetGenericArguments()))
            {
                signatures.Add("TYPECONSTRAINT|" + typeName + "|" + constraint);
            }

            var flags =
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly;

            foreach (var constructor in type.GetConstructors(flags).OrderBy(FormatConstructor, StringComparer.Ordinal))
            {
                signatures.Add("CTOR|" + typeName + "|" + FormatConstructor(constructor));
            }

            foreach (var method in type.GetMethods(flags).OrderBy(FormatMethod, StringComparer.Ordinal))
            {
                if (method.IsSpecialName &&
                    (method.Name.StartsWith("get_", StringComparison.Ordinal) ||
                     method.Name.StartsWith("set_", StringComparison.Ordinal) ||
                     method.Name.StartsWith("add_", StringComparison.Ordinal) ||
                     method.Name.StartsWith("remove_", StringComparison.Ordinal)))
                {
                    continue;
                }

                var methodKey = FormatMethod(method);
                signatures.Add("METHOD|" + typeName + "|" + methodKey);

                if (method.IsStatic)
                {
                    signatures.Add("METHODSTATIC|" + typeName + "|" + methodKey);
                }
                else
                {
                    signatures.Add("METHODINSTANCE|" + typeName + "|" + methodKey);
                }

                if (method.IsVirtual)
                {
                    signatures.Add("METHODVIRTUAL|" + typeName + "|" + methodKey);
                }

                if (method.IsAbstract)
                {
                    signatures.Add("METHODABSTRACT|" + typeName + "|" + methodKey);
                }

                foreach (var constraint in FormatGenericConstraints(method.GetGenericArguments()))
                {
                    signatures.Add("METHODCONSTRAINT|" + typeName + "|" + methodKey + "|" + constraint);
                }
            }

            foreach (var property in type.GetProperties(flags).OrderBy(FormatProperty, StringComparer.Ordinal))
            {
                var getter = property.GetMethod;
                var setter = property.SetMethod;

                if ((getter is null || !getter.IsPublic) &&
                    (setter is null || !setter.IsPublic))
                {
                    continue;
                }

                var propertyKey = FormatProperty(property);
                signatures.Add("PROPERTY|" + typeName + "|" + propertyKey);

                if (getter is not null && getter.IsPublic)
                {
                    signatures.Add("PROPERTYGET|" + typeName + "|" + propertyKey);
                }

                if (setter is not null && setter.IsPublic)
                {
                    signatures.Add("PROPERTYSET|" + typeName + "|" + propertyKey);
                }
            }

            foreach (var eventInfo in type.GetEvents(flags).OrderBy(e => e.Name, StringComparer.Ordinal))
            {
                var add = eventInfo.AddMethod;
                var remove = eventInfo.RemoveMethod;

                if ((add is null || !add.IsPublic) &&
                    (remove is null || !remove.IsPublic))
                {
                    continue;
                }

                signatures.Add(
                    "EVENT|" + typeName + "|" + eventInfo.Name + ":" +
                    FormatType(eventInfo.EventHandlerType!));
            }

            foreach (var field in type.GetFields(flags).OrderBy(f => f.Name, StringComparer.Ordinal))
            {
                var fieldKey = field.Name + ":" + FormatType(field.FieldType);
                signatures.Add("FIELD|" + typeName + "|" + fieldKey);
                signatures.Add(
                    (field.IsStatic ? "FIELDSTATIC|" : "FIELDINSTANCE|") +
                    typeName + "|" + fieldKey);

                if (field.IsLiteral)
                {
                    signatures.Add(
                        "FIELDCONST|" + typeName + "|" + fieldKey + "|" +
                        EscapeValue(field.GetRawConstantValue()));
                }
                else if (field.IsInitOnly)
                {
                    signatures.Add("FIELDREADONLY|" + typeName + "|" + fieldKey);
                }
                else
                {
                    signatures.Add("FIELDMUTABLE|" + typeName + "|" + fieldKey);
                }
            }
        }

        return signatures
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
    }

    private static string FormatConstructor(ConstructorInfo constructor)
    {
        return ".ctor(" +
            string.Join(",", constructor.GetParameters().Select(FormatParameter)) +
            ")";
    }

    private static string FormatMethod(MethodInfo method)
    {
        var generic =
            method.IsGenericMethodDefinition
                ? "<" + string.Join(",", method.GetGenericArguments().Select(argument => argument.Name)) + ">"
                : "";

        return method.Name +
            generic +
            "(" +
            string.Join(",", method.GetParameters().Select(FormatParameter)) +
            ")->" +
            FormatType(method.ReturnType);
    }

    private static string FormatProperty(PropertyInfo property)
    {
        var indexParameters = property.GetIndexParameters();
        var index =
            indexParameters.Length == 0
                ? ""
                : "[" + string.Join(",", indexParameters.Select(FormatParameter)) + "]";

        return property.Name + index + ":" + FormatType(property.PropertyType);
    }

    private static string FormatParameter(ParameterInfo parameter)
    {
        var parameterType = parameter.ParameterType;
        var modifier = "";

        if (parameterType.IsByRef)
        {
            if (parameter.IsOut)
            {
                modifier = "out ";
            }
            else if (parameter.IsIn)
            {
                modifier = "in ";
            }
            else
            {
                modifier = "ref ";
            }

            parameterType = parameterType.GetElementType()!;
        }

        var optional =
            parameter.IsOptional
                ? "=optional:" + EscapeValue(parameter.DefaultValue)
                : "";

        return modifier +
            FormatType(parameterType) +
            " " +
            parameter.Name +
            optional;
    }

    private static IEnumerable<string> FormatGenericConstraints(IEnumerable<Type> parameters)
    {
        foreach (var parameter in parameters.Where(value => value.IsGenericParameter))
        {
            var parts = new List<string>();
            var attributes =
                parameter.GenericParameterAttributes &
                GenericParameterAttributes.SpecialConstraintMask;

            if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            {
                parts.Add("class");
            }

            if ((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
                parts.Add("struct");
            }

            foreach (var constraint in parameter.GetGenericParameterConstraints().OrderBy(FormatType, StringComparer.Ordinal))
            {
                parts.Add(FormatType(constraint));
            }

            if ((attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
            {
                parts.Add("new()");
            }

            yield return parameter.Name + ":" + string.Join(",", parts);
        }
    }

    private static string FormatType(Type type)
    {
        if (type.IsByRef)
        {
            return FormatType(type.GetElementType()!) + "&";
        }

        if (type.IsPointer)
        {
            return FormatType(type.GetElementType()!) + "*";
        }

        if (type.IsArray)
        {
            return FormatType(type.GetElementType()!) +
                "[" + new string(',', type.GetArrayRank() - 1) + "]";
        }

        if (type.IsGenericParameter)
        {
            return "!" + type.Name;
        }

        if (type.IsGenericType)
        {
            var definition =
                type.IsGenericTypeDefinition
                    ? type
                    : type.GetGenericTypeDefinition();

            var name =
                (definition.FullName ?? definition.Name).Replace('+', '.');

            return name +
                "<" +
                string.Join(",", type.GetGenericArguments().Select(FormatType)) +
                ">";
        }

        return (type.FullName ?? type.Name).Replace('+', '.');
    }

    private static string EscapeValue(object? value)
    {
        if (value is null)
        {
            return "null";
        }

        var text =
            Convert.ToString(value, CultureInfo.InvariantCulture) ??
            "";

        return text
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
    }
}