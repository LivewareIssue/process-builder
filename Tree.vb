MustInherit Class Node(Of TBranch, TLeaf)
    Public MustOverride ReadOnly Property Children As ICollection(Of Node(Of TBranch, TLeaf))
    
    Public Function Fold(Of TResult)(fBranch As Func(Of IEnumerable(Of TResult), TBranch, TResult), fLeaf As Func(Of TLeaf, TResult)) As TResult
        Select Case Me.GetType()
            Case GetType(Branch)
                Dim branch = CType(Me, Branch)
                Return fBranch(branch.Children.Select(Function (child) child.Fold(fBranch, fLeaf)), branch.Value)
            Case GetType(Leaf)
                Dim leaf = CType(Me, Leaf)
                Return fLeaf(leaf.Value)
        End Select
    End Function
    
    Public Shared Function Unfold(Of TSeed)(seed As TSeed, f As Func(Of TSeed, IEither(Of TLeaf, Tuple(Of TBranch, IEnumerable(Of TSeed))))) As Node(Of TBranch, TLeaf)
        Dim leftCase = Function(value As TLeaf) As Node(Of TBranch, TLeaf)
            Return New Leaf(value)
        End Function
        
        Dim rightCase = Function(pair As Tuple(Of TBranch, IEnumerable(Of TSeed))) As Node(Of TBranch, TLeaf)
            Dim parent = New Branch(pair.Item1)
            For Each childSeed In pair.Item2
                parent.Children.Add(Unfold(childSeed, f))
            Next
            Return parent
        End Function
        
        Return f(seed).Either(leftCase, rightCase)
    End Function
    
    Public Class Branch 
        Inherits Node(Of TBranch, TLeaf)

        Public Sub New(value As TBranch)
            Me.Value = value
        End Sub

        Public Property Value As TBranch
        
        Public Overrides ReadOnly Property Children As ICollection(Of Node(Of TBranch,TLeaf)) _
            = New List(Of Node(Of TBranch,TLeaf))

        Public Overrides Function ToString() As String
            Return $"{{{NameOf(Value)}: {Value}, {NameOf(Children)}: [{String.Join(",", Children)}]"
        End Function
    End Class
    
    Public Class Leaf
        Inherits Node(Of TBranch, TLeaf)
        
        Public Property Value As TLeaf
        
        Public Sub New(value As TLeaf)
            Me.Value = value
        End Sub

        Public Overrides ReadOnly Property Children As ICollection(Of Node(Of TBranch,TLeaf))
            Get
                Return New Node(Of TBranch, TLeaf)(){}
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function
    End Class
End Class