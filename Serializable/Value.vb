Namespace Serializable
    Class Value
        Inherits ObjectTreeNode
        Public Property Literal As String
        Public Property Type As Type
        
        Public Sub New(parent As Constructor)
            Me.parent = parent
        End Sub
        
        Public Overrides ReadOnly Property Children As IEnumerable(Of ObjectTreeNode)
            Get
                Return New ObjectTreeNode(){}
            End Get
        End Property
    End Class
End Namespace