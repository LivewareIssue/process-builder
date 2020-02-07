Imports System.Reflection
Imports System.Runtime.CompilerServices

Public Module Extensions
    <Extension>
    Public Iterator Function Flatten(Of T)(root As T, children As Func(Of T, IEnumerable(Of T))) As IEnumerable(Of T)
        Yield root
        For Each child As T In children(root).SelectMany(Function (c) c.Flatten(children))
            Yield child
        Next
    End Function
    
    <Extension>
    Public Function Fold(Of T, A)(root As T, children As Func(Of T, IEnumerable(Of T)), f As Func(Of IEnumerable(Of A), T, A)) As A
        Return f(children(root).Select(Function(child) child.Fold(children, f)), root)
    End Function
    
    <Extension>
    Public Function Unfold(Of T, A)(seed As A, addChild As Action(Of T, T), f As Func(Of A, Tuple(Of T, IEnumerable(Of A)))) As T
        Dim result = f(seed)
        Dim node As T = result.Item1
        For Each childSeed As A In result.Item2
            addChild(node, childSeed.Unfold(addChild, f))
        Next
        
        Return node
    End Function

    <Extension>
    Public Function DisplayName(type As Type) As String
        If type.IsGenericType Then
            Return $"{type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.Ordinal))}(Of {String.Join(", ", type.GenericTypeArguments.Select(AddressOf DisplayName))})"
        Else
            Return type.Name
        End If
    End Function

    <Extension>
    Public Function QualifiedDisplayName(type As Type) As String
        Return $"{type.Namespace}.{type.DisplayName}"
    End Function

    <Extension()>
    Public Function DisplayName(constructor As ConstructorInfo)
        If Not constructor.GetParameters.Any Then
            Return "New()"
        Else
            Return $"New({String.Join(", ", constructor.GetParameters.Select(Function(parameter) $"{parameter.Name} As {parameter.ParameterType.DisplayName}"))})"
        End If
    End Function

    <Extension()>
    Public Function DisplayName(parameter As ParameterInfo) As String
        Return $"{parameter.Name} As {parameter.ParameterType.DisplayName}"
    End Function
End Module