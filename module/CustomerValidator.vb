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
    ' スキップ条件（カスタムロジック1）
    ' ==========================================
    Protected Overrides Function ShouldSkipValidation(fields() As String) As Boolean
        ' 想定レイアウト:
        ' 0:顧客コード, 1:支店コード, 2:顧客名, 3:住所, 4:電話番号, 
        ' 5:登録日, 6:更新日, 7:削除フラグ
        
        ' 削除フラグ（7列目）が"X"または"削除"の場合はスキップ
        If fields.Length > 7 Then
            Dim deleteFlag = fields(7).Trim()
            If deleteFlag = "X" OrElse deleteFlag = "削除" Then
                Return True
            End If
        End If
        
        Return False
    End Function
    
    ' ==========================================
    ' カスタムバリデーション（カスタムロジック2）
    ' ==========================================
    Protected Overrides Sub ValidateCustomRules(fields() As String, result As RowValidationResult)
        ' 顧客コードのフォーマットチェック
        ValidateCustomerCodeFormat(fields, result)
        
        ' 支店コードのフォーマットチェック
        ValidateBranchCodeFormat(fields, result)
        
        ' 電話番号のフォーマットチェック
        ValidatePhoneNumberFormat(fields, result)
    End Sub
    
    ' 顧客コードのフォーマットチェック（例: "C"で始まる必要がある）
    Private Sub ValidateCustomerCodeFormat(fields() As String, result As RowValidationResult)
        If fields.Length <= 0 OrElse String.IsNullOrWhiteSpace(fields(0)) Then
            Return  ' 必須チェックで既にエラーになっている
        End If
        
        Dim customerCode = fields(0)
        
        ' "C"で始まるかチェック
        If Not customerCode.StartsWith("C") Then
            result.Errors.Add(New ValidationError With {
                .LineNumber = result.LineNumber,
                .ColumnIndex = 0,
                .ColumnName = "顧客コード",
                .ErrorType = "形式",
                .ErrorMessage = "顧客コードは'C'で始まる必要があります",
                .RawValue = customerCode
            })
        End If
        
        ' 英数字のみかチェック
        If Not Regex.IsMatch(customerCode, "^[A-Za-z0-9]+$") Then
            result.Errors.Add(New ValidationError With {
                .LineNumber = result.LineNumber,
                .ColumnIndex = 0,
                .ColumnName = "顧客コード",
                .ErrorType = "形式",
                .ErrorMessage = "顧客コードは英数字のみで入力してください",
                .RawValue = customerCode
            })
        End If
    End Sub
    
    ' 支店コードのフォーマットチェック（例: 3桁の数字）
    Private Sub ValidateBranchCodeFormat(fields() As String, result As RowValidationResult)
        If fields.Length <= 1 OrElse String.IsNullOrWhiteSpace(fields(1)) Then
            Return
        End If
        
        Dim branchCode = fields(1)
        
        ' 数字のみかチェック
        If Not Regex.IsMatch(branchCode, "^\d+$") Then
            result.Errors.Add(New ValidationError With {
                .LineNumber = result.LineNumber,
                .ColumnIndex = 1,
                .ColumnName = "支店コード",
                .ErrorType = "形式",
                .ErrorMessage = "支店コードは数字のみで入力してください",
                .RawValue = branchCode
            })
        End If
    End Sub
    
    ' 電話番号のフォーマットチェック（例: 03-1234-5678）
    Private Sub ValidatePhoneNumberFormat(fields() As String, result As RowValidationResult)
        If fields.Length <= 4 OrElse String.IsNullOrWhiteSpace(fields(4)) Then
            Return  ' 空欄は許容（必須でない場合）
        End If
        
        Dim phoneNumber = fields(4)
        
        ' ハイフン区切りの電話番号形式チェック（簡易版）
        ' 例: 03-1234-5678, 090-1234-5678
        If Not Regex.IsMatch(phoneNumber, "^\d{2,4}-\d{4}-\d{4}$") Then
            result.Errors.Add(New ValidationError With {
                .LineNumber = result.LineNumber,
                .ColumnIndex = 4,
                .ColumnName = "電話番号",
                .ErrorType = "形式",
                .ErrorMessage = "電話番号の形式が不正です（例: 03-1234-5678）",
                .RawValue = phoneNumber
            })
        End If
    End Sub
    
End Class