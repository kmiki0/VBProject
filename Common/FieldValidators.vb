' ==========================================
' 共通チェックロジック（必須/桁数/日付/列数）
' ==========================================
Imports System

''' <summary>
''' FileDefinition を受け取り、フィールド配列と RowValidationResult を使って
''' 共通のチェックを行うユーティリティ関数群。
''' このモジュールに移動することでチェックロジックを集中管理できる。
''' </summary>
Public Module FieldValidators

    ''' <summary> 列数チェック（期待列数が <=0 の場合はスキップ） </summary>
    Public Function ValidateColumnCount(fields() As String, fileDef As FileDefinition, result As RowValidationResult) As Boolean
        If fileDef Is Nothing Then
            Return True
        End If

        If fileDef.ExpectedColumnCount <= 0 Then
            Return True
        End If

        If fields.Length < fileDef.ExpectedColumnCount Then
            result.Errors.Add(New ValidationError With {
                .LineNumber = result.LineNumber,
                .ColumnIndex = -1,
                .ColumnName = "",
                .ErrorType = "列数不足",
                .ErrorMessage = $"列数が不足しています（期待値: {fileDef.ExpectedColumnCount}列、実際: {fields.Length}列）",
                .RawValue = ""
            })
            Return False
        End If

        Return True
    End Function

    ''' <summary> 必須チェック（1つでも未入力があれば False を返す） </summary>
    Public Function ValidateRequired(fields() As String, fileDef As FileDefinition, result As RowValidationResult) As Boolean
        If fileDef Is Nothing OrElse fileDef.RequiredColumns Is Nothing Then
            Return True
        End If

        For Each reqCol In fileDef.RequiredColumns
            If reqCol.Index >= fields.Length OrElse String.IsNullOrWhiteSpace(fields(reqCol.Index)) Then
                result.Errors.Add(New ValidationError With {
                    .LineNumber = result.LineNumber,
                    .ColumnIndex = reqCol.Index,
                    .ColumnName = reqCol.Name,
                    .ErrorType = "必須",
                    .ErrorMessage = "必須項目が未入力です",
                    .RawValue = If(reqCol.Index < fields.Length, fields(reqCol.Index), "")
                })
                Return False
            End If
        Next

        Return True
    End Function

    ''' <summary>桁数チェック（ルールごとにエラーを追加する。エラーでも継続実行）</summary>
    Public Sub ValidateLength(fields() As String, fileDef As FileDefinition, result As RowValidationResult)
        If fileDef Is Nothing OrElse fileDef.LengthRules Is Nothing Then
            Return
        End If

        For Each rule In fileDef.LengthRules
            If rule.ColumnIndex >= fields.Length Then
                Continue For
            End If

            Dim value = fields(rule.ColumnIndex)
            Dim length = If(value Is Nothing, 0, value.Length)

            ' 最大桁数チェック
            If length > rule.MaxLength Then
                result.Errors.Add(New ValidationError With {
                    .LineNumber = result.LineNumber,
                    .ColumnIndex = rule.ColumnIndex,
                    .ColumnName = rule.ColumnName,
                    .ErrorType = "桁数",
                    .ErrorMessage = $"最大{rule.MaxLength}桁（現在{length}桁）",
                    .RawValue = value
                })
            End If

            ' 最小桁数チェック
            If rule.MinLength.HasValue AndAlso length < rule.MinLength.Value AndAlso length > 0 Then
                result.Errors.Add(New ValidationError With {
                    .LineNumber = result.LineNumber,
                    .ColumnIndex = rule.ColumnIndex,
                    .ColumnName = rule.ColumnName,
                    .ErrorType = "桁数",
                    .ErrorMessage = $"最小{rule.MinLength.Value}桁（現在{length}桁）",
                    .RawValue = value
                })
            End If
        Next
    End Sub


    ''' <summary>日付形式チェック（空欄はスキップ、Date.TryParse で判定）</summary>
    Public Sub ValidateDate(fields() As String, fileDef As FileDefinition, result As RowValidationResult)
        If fileDef Is Nothing OrElse fileDef.DateColumns Is Nothing Then
            Return
        End If

        For Each dateCol In fileDef.DateColumns
            If dateCol.Index >= fields.Length Then
                Continue For
            End If

            Dim value = fields(dateCol.Index)
            If String.IsNullOrWhiteSpace(value) Then
                Continue For
            End If

            Dim dateValue As Date
            If Not Date.TryParse(value, dateValue) Then
                result.Errors.Add(New ValidationError With {
                    .LineNumber = result.LineNumber,
                    .ColumnIndex = dateCol.Index,
                    .ColumnName = dateCol.Name,
                    .ErrorType = "日付形式",
                    .ErrorMessage = "日付形式が不正です（例: 2025/01/15）",
                    .RawValue = value
                })
            End If
        Next
    End Sub

End Module
