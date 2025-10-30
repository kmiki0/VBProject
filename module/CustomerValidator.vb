' ==========================================
' CustomerValidator.vb
' 顧客マスタ用のバリデータクラス
' GetFileDefinition(): CustomerLayoutから設定を取得
' ==========================================
Imports System.Text.RegularExpressions

Public Class CustomerValidator
    Inherits FileValidatorBase
    
    ' ==========================================
    ' ファイル定義を取得
    ' ==========================================
    Protected Overrides Function GetFileDefinition() As FileDefinition
        Return CustomerLayout.GetDefinition()
    End Function
    
    ' ==========================================
    ' スキップする場合(Defalt: False)
    ' ==========================================
    Protected Overrides Function ShouldSkipValidation(fields() As String) As Boolean
        ' Enumから列インデックスを取得
        Dim skipIndex = Convert.ToInt32(FileLayout.削除フラグ)

        ' 削除フラグ（7列目）が"X"の場合はスキップ
        Dim deleteFlag = fields(skipIndex).Trim()
        If deleteFlag = "X" Then
            Return True
        End If
        
        Return False
    End Function
    
    ' ==========================================
    ' カスタムバリデーション
    ' ==========================================
    Protected Overrides Sub ValidateCustomRules(fields() As String, result As RowValidationResult)
        ValidateDateFormat(fields, result, FileLayout.登録日)
    End Sub
    
    ''' <summary>
    ''' 日付形式チェック（yyyyMMdd形式）
    ''' </summary>
    Private Sub ValidateDateFormat(fields() As String, result As RowValidationResult, columnEnum As [Enum])
        Dim columnIndex = Convert.ToInt32(columnEnum)
        Dim columnName = [Enum].GetName(columnEnum.GetType(), columnEnum)
        
        If columnIndex >= fields.Length Then Return
        
        Dim value = fields(columnIndex)
        If String.IsNullOrWhiteSpace(value) Then Return
        
        Dim dateValue As Date
        ' yyyyMMdd形式で厳密にチェック
        If Not Date.TryParseExact(value, "yyyyMMdd", Nothing, DateTimeStyles.None, dateValue) Then
            result.Errors.Add(New ValidationError With {
                .LineNumber = result.LineNumber,
                .ColumnIndex = columnIndex,
                .ColumnName = columnName,
                .ErrorType = "日付形式",
                .ErrorMessage = "日付形式が不正です（形式: yyyyMMdd 例: 20250115）",
                .RawValue = value
            })
        End If
    End Sub

End Class