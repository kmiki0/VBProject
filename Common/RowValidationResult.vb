' ==========================================
' RowValidationResult.vb
' 1行分のバリデーション結果を保持するクラス
' 役割: 1行分の検証結果（OK/NG）とエラー情報をまとめて管理
' 使い所: 各行をチェックした結果を格納
' ==========================================
Public Class RowValidationResult
    ''' <summary>行番号</summary>
    Public Property LineNumber As Integer
    
    ''' <summary>元データ（1行分の文字列）</summary>
    Public Property RawData As String
    
    ''' <summary>キー項目の値（キー名と値のペア）</summary>
    Public Property KeyValues As New Dictionary(Of String, String)
    
    ''' <summary>エラーリスト（1行に複数エラーがある場合もある）</summary>
    Public Property Errors As New List(Of ValidationError)
    
    ''' <summary>エラーがないか判定</summary>
    Public ReadOnly Property IsValid As Boolean
        Get
            Return Errors.Count = 0
        End Get
    End Property
    
    ''' <summary>キー項目を文字列で取得（ログ出力用）</summary>
    Public Function GetKeyValueString() As String
        If KeyValues.Count = 0 Then
            Return ""
        End If
        
        Dim parts As New List(Of String)
        For Each kv In KeyValues
            parts.Add($"{kv.Key}: {kv.Value}")
        Next
        
        Return String.Join(", ", parts)
    End Function
    
    Public Sub New()
    End Sub
End Class