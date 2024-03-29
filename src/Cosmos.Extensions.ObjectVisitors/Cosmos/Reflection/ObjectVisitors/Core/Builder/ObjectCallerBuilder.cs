﻿#if !NETFRAMEWORK

using System.Reflection;
using System.Text;
using BTFindTree;
using Cosmos.Reflection.ObjectVisitors.Metadata;
using Natasha.CSharp.Compiler;

namespace Cosmos.Reflection.ObjectVisitors.Core.Builder
{
    public class ObjectCallerBuilder
    {
        public static Type InitType(Type type, AlgorithmKind kind = AlgorithmKind.Hash)
        {
            if (!ObjectMemberAccessibilityMan._accessCache.TryGetValue(type, out var accessLevel))
                accessLevel = ObjectMemberAccessibilityLevel.Default;

            var isStatic = type.IsSealed && type.IsAbstract;
            var callType = typeof(ObjectCallerBase);

            var body = new StringBuilder();
            var setByObjectCache = new Dictionary<string, string>();
            var getByObjectCache = new Dictionary<string, string>();
            var getByStrongTypeCache = new Dictionary<string, string>();
            var getByObjMembersCache = new Dictionary<string, ObjectMember>();
            var getByObjMembersScriptCache = new Dictionary<string, string>();

            var getByReadOnlyStaticScriptBuilder = new StringBuilder();
            var getByReadOnlySettingScriptBuilder = new StringBuilder();
            var getByInternalNamesScriptBuilder = new StringBuilder();

            #region Field

            var flag = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;
            if (accessLevel == ObjectMemberAccessibilityLevel.AllowNoPublic)
                flag |= BindingFlags.NonPublic;

            var fields = type.GetFields(flag);

            foreach (var field in fields)
            {
                if (field.IsSpecialName || field.Name.Contains("<"))
                {
                    continue;
                }

                var caller = "Instance";

                if (field.IsStatic)
                {
                    caller = type.GetDevelopName();
                }

                var fieldName = field.Name;
                var fieldType = field.FieldType.GetDevelopName();

                //set
                if (!field.IsLiteral)
                {
                    var fieldScript = $"{caller}.{fieldName}";

                    if (field.IsInitOnly)
                    {
                        fieldScript = fieldScript.ToReadonlyScript();
                    }

                    setByObjectCache[fieldName] = $"{fieldScript} = ({fieldType})value;";
                }


                //get
                getByObjectCache[fieldName] = $"return {caller}.{fieldName};";
                getByStrongTypeCache[fieldName] = $"return (T)(object)({caller}.{fieldName});";

                //member metadata
                getByObjMembersCache[fieldName] = field;
                getByObjMembersScriptCache[fieldName] = $"return __metadata_ObjectMember_{fieldName};";
                getByReadOnlyStaticScriptBuilder.AppendLine($@"private static readonly ObjectMember __metadata_ObjectMember_{fieldName};");
                getByInternalNamesScriptBuilder.Append($@"""{fieldName}"",");
                getByReadOnlySettingScriptBuilder.Append($"__metadata_ObjectMember_{fieldName}".ToReadonlyScript());
                getByReadOnlySettingScriptBuilder.Append($@" = objMembersCache[""{fieldName}""];");
            }

            #endregion

            #region Property

            var props = type.GetProperties(flag);

            foreach (var property in props)
            {
                var method = property.CanRead ? property.GetGetMethod(true) : property.GetSetMethod(true);

                var caller = "Instance";

                if (method.IsStatic)
                {
                    caller = type.GetDevelopName();
                }

                var propertyName = property.Name;
                var propertyType = property.PropertyType.GetDevelopName();
                var propertyScript = $"{caller}.{propertyName}";

                //set
                if (property.CanWrite)
                {
                    setByObjectCache[propertyName] = $"{propertyScript} = ({propertyType})value;";
                }


                //get
                if (property.CanRead)
                {
                    getByObjectCache[propertyName] = $"return {caller}.{propertyName};";
                    getByStrongTypeCache[propertyName] = $"return (T)(object)({caller}.{propertyName});";
                }

                //member metadata
                getByObjMembersCache[propertyName] = property;
                getByObjMembersScriptCache[propertyName] = $"return __metadata_ObjectMember_{propertyName};";
                getByReadOnlyStaticScriptBuilder.AppendLine($@"private static readonly ObjectMember __metadata_ObjectMember_{propertyName};");
                getByInternalNamesScriptBuilder.Append($@"""{propertyName}"",");
                getByReadOnlySettingScriptBuilder.Append($"__metadata_ObjectMember_{propertyName}".ToReadonlyScript());
                getByReadOnlySettingScriptBuilder.Append($@" = objMembersCache[""{propertyName}""];");
            }

            #endregion

            string setObjectBody = default;
            string getObjectBody = default;
            string getStrongTypeBody = default;
            string getObjMemberBody = default;

            switch (kind)
            {
                case AlgorithmKind.Fuzzy:
                    setObjectBody = BTFTemplate.GetGroupFuzzyPointBTFScript(setByObjectCache, "name");
                    getObjectBody = BTFTemplate.GetGroupFuzzyPointBTFScript(getByObjectCache, "name");
                    getStrongTypeBody = BTFTemplate.GetGroupFuzzyPointBTFScript(getByStrongTypeCache, "name");
                    getObjMemberBody = BTFTemplate.GetGroupFuzzyPointBTFScript(getByObjMembersScriptCache, "name");
                    break;
                case AlgorithmKind.Hash:
                    setObjectBody = BTFTemplate.GetHashBTFScript(setByObjectCache, "name");
                    getObjectBody = BTFTemplate.GetHashBTFScript(getByObjectCache, "name");
                    getStrongTypeBody = BTFTemplate.GetHashBTFScript(getByStrongTypeCache, "name");
                    getObjMemberBody = BTFTemplate.GetHashBTFScript(getByObjMembersScriptCache, "name");
                    break;
                case AlgorithmKind.Precision:
                    setObjectBody = BTFTemplate.GetGroupPrecisionPointBTFScript(setByObjectCache, "name");
                    getObjectBody = BTFTemplate.GetGroupPrecisionPointBTFScript(getByObjectCache, "name");
                    getStrongTypeBody = BTFTemplate.GetGroupPrecisionPointBTFScript(getByStrongTypeCache, "name");
                    getObjMemberBody = BTFTemplate.GetGroupPrecisionPointBTFScript(getByObjMembersScriptCache, "name");
                    break;
            }


            //To add readonly metadata (LeoMember) properties.
            body.AppendLine(getByReadOnlyStaticScriptBuilder.ToString());


            body.AppendLine("public unsafe override void Set(string name,object value){");
            body.AppendLine(setObjectBody);
            body.Append('}');

#if NET5_0_OR_GREATER
            body.AppendLine("[SkipLocalsInit]");
#endif
            body.AppendLine("public unsafe override T Get<T>(string name){");
            body.AppendLine(getStrongTypeBody);
            body.Append("return default;}");

#if NET5_0_OR_GREATER
            body.AppendLine("[SkipLocalsInit]");
#endif
            body.AppendLine("public unsafe override object GetObject(string name){");
            body.AppendLine(getObjectBody);
            body.Append("return default;}");


            body.AppendLine("public unsafe override ObjectMember GetMember(string name){");
            body.AppendLine(getObjMemberBody);
            body.Append("return default;}");


            body.AppendLine("protected override HashSet<string> InternalMemberNames { get; } = new HashSet<string>(){");
            body.AppendLine(getByInternalNamesScriptBuilder.ToString());
            body.Append("};");


            body.AppendLine("public static void InitMetadataMapping(Dictionary<string, ObjectMember> objMembersCache){");
            body.AppendLine(getByReadOnlySettingScriptBuilder.ToString());
            body.Append('}');


            if (!isStatic)
            {
                callType = typeof(ObjectCallerBase<>).With(type);
                body.Append($@"public override void New(){{ Instance = new {type.GetDevelopName()}();}}");
            }
            else
            {
                body.Append($@"public override void SetObjInstance(object obj){{ }}");
                body.Append($@"public override object GetObjInstance(){{ return default(object); }}");
            }

            var clsBuilder = NClass.UseDomain(type.GetDomain())
                                   .Public()
                                   .Using(type)
                                   .Namespace("Cosmos.Reflection.NCallerDynamic")
                                   .InheritanceAppend(callType)
                                   .Body(body.ToString());

            if (accessLevel == ObjectMemberAccessibilityLevel.AllowNoPublic)
            {
                clsBuilder.AllowPrivate(type.Assembly);
                clsBuilder.AssemblyBuilder.ConfigCompilerOption(item => item.SetCompilerFlag(CompilerBinderFlags.IgnoreCorLibraryDuplicatedTypes | CompilerBinderFlags.IgnoreAccessibility));
            }
            else
            {
                clsBuilder.AssemblyBuilder.ConfigCompilerOption(item => item.RemoveIgnoreAccessibility());
            }

            var tempClass = clsBuilder.GetType();

            InitMetadataMappingCaller(tempClass)(getByObjMembersCache);

            return tempClass;
        }

        private static Action<Dictionary<string, ObjectMember>> InitMetadataMappingCaller(Type runtimeProxyType)
        {
            return NDelegate
                   .UseDomain(runtimeProxyType.GetDomain())
                   .Action<Dictionary<string, ObjectMember>>($"{runtimeProxyType.GetDevelopName()}.InitMetadataMapping(obj);");
        }
    }
}

#endif