Public Class TestForm
    
    Private ReadOnly _test As Test
    
    Public Sub New(test As Test)
        _test = test
    End Sub
    
    Public Overrides Function ToString() As String
        Return $"TestForm {{{NameOf(_test)}: {_test}}}"
    End Function
End Class
    
Public Class Test
    Private ReadOnly _s As String
    Private ReadOnly _n As Integer
    
    Public Sub New(s As String, n As Integer)
        _s = s
        _n = n
    End Sub

    Public Overrides Function ToString() As String
        Return $"Test {{{NameOf(_s)}: {_s}, {NameOf(_n)}: {_n}}}"
    End Function
End Class