Imports System.Runtime.CompilerServices

Public Module Utilities
    
    <Extension>
    Public Function RelativeTo(path As String, basePath As String)
        If Not basePath.EndsWith("/") Then
            basePath += "/"
        End If
        
        Dim baseUri As New Uri(basePath)
        Dim fullUri As New Uri(path)
        
        Dim relativeUri = baseUri.MakeRelativeUri(fullUri)
        
        Return relativeUri.ToString
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
    
    Public Function Constant(Of T, TResult)(value As TResult) As Func(Of T, TResult)
        Return Function(ignored) value
    End Function
    
    Public Function Identity(Of T)(value As T) As T
        Return value
    End Function
    
    Public Sub WithForegroundColor(color As ConsoleColor, action As Action)
        Dim currentColor As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = color
        action.Invoke
        Console.ForegroundColor = currentColor
    End Sub
    
    Public Sub Done()
        WithForegroundColor(ConsoleColor.Green, Sub() Console.WriteLine("Done"))
    End Sub
    
    Public Sub Failed(reason As String)
        WithForegroundColor(ConsoleColor.Red, Sub() Console.Write("Failed: "))
        Console.WriteLine(reason)
    End Sub
End Module