' ==========================================
' FileDefinition.vb
' ファイルのレイアウト定義をまとめたクラス
' 役割: 1つのファイルに対するすべての設定を保持
' 使い所: 各バリデータのGetFileDefinition()で返すオブジェクト
' ==========================================
Public Class FileDefinition
    ''' <summary>ファイル名（表示用）</summary>
    Public Property Name As String
    
    ''' <summary>出力ファイル名のプレフィックス（例: "customer" → customer_OK_data.csv）</summary>
    Public Property OutputFileName As String
    
    ''' <summary>キー項目の定義（エラーログに表示される項目）</summary>
    Public Property KeyColumns As ColumnDefinition()
    
    ''' <summary>一意性制約の対象列（複合キーの場合は複数指定）</summary>
    Public Property UniqueColumns As Integer()
    
    ''' <summary>必須項目の定義</summary>
    Public Property RequiredColumns As ColumnDefinition()
    
    ''' <summary>桁数チェックのルール</summary>
    Public Property LengthRules As LengthRule()
    
    ''' <summary>日付項目の定義</summary>
    Public Property DateColumns As ColumnDefinition()
    
    Public Sub New()
    End Sub
End Class