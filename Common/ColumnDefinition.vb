' ==========================================
' ColumnDefinition.vb
' 列の定義（インデックスと名前）
' 役割: 「何列目に何という項目があるか」を定義
' 使い所: キー項目、必須項目、日付項目などの設定
' ==========================================
Public Class ColumnDefinition
    ''' <summary>列インデックス（0始まり）</summary>
    Public Property Index As Integer
    
    ''' <summary>列名（日本語名でOK）</summary>
    Public Property Name As String
    
    Public Sub New()
    End Sub
    
    Public Sub New(index As Integer, name As String)
        Me.Index = index
        Me.Name = name
    End Sub
End Class