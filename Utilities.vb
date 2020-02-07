Public Module Utilities
    Public Function NoOp(Of T) As Action(Of T)
        Return Sub(ignored)
        End Sub
    End Function
End Module