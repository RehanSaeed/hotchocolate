using System;
using System.Linq;
using System.Threading.Tasks;
using StrawberryShake.CodeGeneration.CSharp.Builders;
using StrawberryShake.CodeGeneration.Extensions;

namespace StrawberryShake.CodeGeneration.CSharp
{
    public partial class JsonResultBuilderGenerator
    {
        private void AddUpdateEntityArrayMethod(ListTypeDescriptor listTypeDescriptor)
        {
            var updateEntityMethod = MethodBuilder.New()
                .SetAccessModifier(AccessModifier.Private)
                .SetName(DeserializerMethodNameFromTypeName(listTypeDescriptor))
                .SetReturnType($"IList<{WellKnownNames.EntityId}>")
                .AddParameter(
                    ParameterBuilder.New()
                        .SetType(jsonElementParamName)
                        .SetName(objParamName)
                )
                .AddParameter(
                    ParameterBuilder.New()
                        .SetType($"ISet<{WellKnownNames.EntityId}>")
                        .SetName(EntityIdsParam)
                );

            updateEntityMethod.AddCode(
                EnsureJsonValueIsNotNull(),
                !listTypeDescriptor.IsNullable
            );

            var listVarName = listTypeDescriptor.Name.WithLowerFirstChar() + "s";
            updateEntityMethod.AddCode(
                $"var {listVarName} = new List<{(listTypeDescriptor.IsEntityType ? WellKnownNames.EntityId : listTypeDescriptor.InnerType.Name)}>();"
            );

            updateEntityMethod.AddCode(
                ForEachBuilder.New()
                    .SetLoopHeader(
                        $"JsonElement child in {objParamName}.EnumerateArray()"
                    )
                    .AddCode(
                        MethodCallBuilder.New()
                            .SetPrefix($"{listVarName}.")
                            .SetMethodName("Add")
                            .AddArgument(BuildUpdateMethodCall(listTypeDescriptor.InnerType, "child"))
                    )
            );

            updateEntityMethod.AddEmptyLine();
            updateEntityMethod.AddCode($"return {listVarName};");

            ClassBuilder.AddMethod(updateEntityMethod);

            AddDeserializeMethod(listTypeDescriptor.InnerType);
        }
    }
}
