' ==========================================
' LengthRule.vb
' 桁数チェックのルール定義
' 役割: 桁数チェックのルールを定義
' 使い所: 「顧客コードは最大10桁」などの設定
' ==========================================
Public Class LengthRule
    ''' <summary>列インデックス（0始まり）</summary>
    Public Property ColumnIndex As Integer
    
    ''' <summary>列名</summary>
    Public Property ColumnName As String
    
    ''' <summary>最大桁数</summary>
    Public Property MaxLength As Integer
    
    ''' <summary>最小桁数（Nothingの場合はチェックしない）</summary>
    Public Property MinLength As Integer?
    
    Public Sub New()
    End Sub
    
    Public Sub New(columnIndex As Integer, columnName As String, maxLength As Integer, Optional minLength As Integer? = Nothing)
        Me.ColumnIndex = columnIndex
        Me.ColumnName = columnName
        Me.MaxLength = maxLength
        Me.MinLength = minLength
    End Sub
End Class