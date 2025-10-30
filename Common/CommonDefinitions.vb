' ==========================================
' バリデーションで使用するデータクラス
' ==========================================

' ==========================================
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
End Class

' ==========================================
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
        If KeyValues.Count = 0 Then Return ""
        
        Dim parts As New List(Of String)
        For Each kv In KeyValues
            parts.Add($"{kv.Key}: {kv.Value}")
        Next
        
        Return String.Join(", ", parts)
    End Function

End Class

' ==========================================
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

    ' Enumを受け取り、名前を自動取得
    Public Sub New(columnEnum As [Enum])
        Me.Index = Convert.ToInt32(columnEnum)
        Me.Name = [Enum].GetName(columnEnum.GetType(), columnEnum)
    End Sub
End Class

' ==========================================
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
    
    ' Enumを受け取り、名前を自動取得
    Public Sub New(columnEnum As [Enum], maxLength As Integer, Optional minLength As Integer? = Nothing)
        Me.ColumnIndex = Convert.ToInt32(columnEnum)
        Me.ColumnName = [Enum].GetName(columnEnum.GetType(), columnEnum)
        Me.MaxLength = maxLength
        Me.MinLength = minLength
    End Sub
End Class

' ==========================================
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
End Class

' ==========================================
' CSV/TSVなどのファイル形式情報
' 役割: 入力ファイルがCSVかTSVかを判定した結果を保持
' 使い所: ファイル読み込み時とOKデータ出力時
' ==========================================
Public Class FileFormatInfo
    ''' <summary>拡張子（例: ".csv", ".tsv"）</summary>
    Public Property Extension As String
    ''' <summary>デリミタ（例: "," or vbTab）</summary>
    Public Property Delimiter As String
    ''' <summary>形式名（例: "CSV", "TSV"）</summary>
    Public Property Name As String
End Class