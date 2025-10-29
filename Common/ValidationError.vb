' ==========================================
' ValidationError.vb
' エラー情報を保持するクラス
' 役割: 1つのエラー情報を保持
' 使い所: バリデーションで引っかかった時にこのオブジェクトを生成
' ==========================================
Public Class ValidationError
    ''' <summary>行番号</summary>
    Public Property LineNumber As Integer
    
    ''' <summary>列インデックス（0始まり）</summary>
    Public Property ColumnIndex As Integer
    
    ''' <summary>列名</summary>
    Public Property ColumnName As String
    
    ''' <summary>エラー種別（必須、桁数、日付形式、一意性、形式、範囲、論理など）</summary>
    Public Property ErrorType As String
    
    ''' <summary>エラーメッセージ</summary>
    Public Property ErrorMessage As String
    
    ''' <summary>エラーが発生した値</summary>
    Public Property RawValue As String
    
    Public Sub New()
    End Sub
End Class