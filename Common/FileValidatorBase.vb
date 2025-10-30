' ==========================================
' すべてのバリデータの基底クラス（共通ロジック）
' ==========================================
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public MustInherit Class FileValidatorBase
    
    ' ==========================================
    ' フィールド
    ' ==========================================
    Protected NGResults As New List(Of RowValidationResult)
    Protected OKData As New List(Of String)
    Private uniqueKeyCache As New Dictionary(Of String, Integer)  ' キー→最初の行番号のマップ
    Private duplicateKeys As New HashSet(Of String)  ' 重複しているキーのセット
    Private inputFileInfo As FileFormatInfo
    Private currentFileDefinition As FileDefinition

    ' ==========================================
    ' 派生クラスで実装が必要な抽象メソッド
    ' ==========================================
    
    ''' <summary>ファイルレイアウト定義を取得（必須実装）</summary>
    Protected MustOverride Function GetFileDefinition() As FileDefinition
    
    ''' <summary>スキップ条件判定（オプション）</summary>
    Protected Overridable Function ShouldSkipValidation(fields() As String) As Boolean
        Return False
    End Function
    
    ''' <summary>カスタムバリデーション（オプション）</summary>
    Protected Overridable Sub ValidateCustomRules(fields() As String, result As RowValidationResult)
        ' 派生クラスで必要に応じて実装
    End Sub
    
    ' ==========================================
    ' メイン処理
    ' ==========================================
    ''' <summary>
    ''' ファイルをバリデーションして結果を出力
    ''' </summary>
    ''' <param name="inputFilePath">入力ファイルパス</param>
    ''' <param name="outputDir">出力フォルダパス</param>
    Public Sub ValidateFile(inputFilePath As String, outputDir As String)
        ' 初期化
        NGResults.Clear()
        OKData.Clear()
        uniqueKeyCache.Clear()
        
        ' ファイル定義を取得
        currentFileDefinition = GetFileDefinition()
        
        ' ファイル形式を判定
        inputFileInfo = DetectFileFormat(inputFilePath)
        
        Console.WriteLine($"ファイル形式: {inputFileInfo.Name}")
        Console.WriteLine($"処理開始...")
        
        ' ファイル読み込み
        Dim lines = ReadFile(inputFilePath)
        
        If lines.Count = 0 Then
            Console.WriteLine("警告: ファイルが空です")
            Return
        End If
        
        ' 1回目: 一意性チェック用のキー収集
        CollectUniqueKeys(lines)
        
        ' 2回目: 実際のバリデーション
        For i As Integer = 0 To lines.Count - 1
            Dim fields = SplitLine(lines(i), inputFileInfo.Delimiter)
            Dim result = ValidateRow(fields, i + 1)
            
            If result.IsValid Then
                OKData.Add(lines(i))
            Else
                NGResults.Add(result)
            End If
        Next
        
        ' 結果出力
        OutputOKData(outputDir)
        OutputNGLog(outputDir)
        
        ' サマリー表示
        Console.WriteLine()
        Console.WriteLine($"処理完了: OK={OKData.Count}件, NG={NGResults.Count}件")
    End Sub
    
    ' ==========================================
    ' ファイル形式判定
    ' ==========================================
    Private Function DetectFileFormat(filePath As String) As FileFormatInfo
        ' レイアウト定義で区切り文字が指定されているかチェック（必須）
        If String.IsNullOrEmpty(currentFileDefinition.Delimiter) Then
            Throw New Exception("レイアウト定義で区切り文字（Delimiter）が指定されていません")
        End If
        
        ' 入力ファイルの拡張子を取得
        Dim ext = Path.GetExtension(filePath).ToLower()
        
        ' 形式名を決定
        Dim formatName As String
        Select Case currentFileDefinition.Delimiter
            Case ","
                formatName = "CSV"
            Case vbTab
                formatName = "TSV"
            Case Else
                formatName = "カスタム区切り文字"
        End Select
        
        Return New FileFormatInfo With {
            .Extension = If(String.IsNullOrEmpty(ext), ".txt", ext),
            .Delimiter = currentFileDefinition.Delimiter,
            .Name = formatName
        }
    End Function

    
    
    ' ==========================================
    ' ファイル読み込み
    ' ==========================================
    Private Function ReadFile(filePath As String) As List(Of String)
        Dim lines As New List(Of String)
        
        Try
            ' UTF-8で読み込み（BOM付きにも対応）
            Using reader As New StreamReader(filePath, Encoding.UTF8, True)
                Dim line As String
                While (InlineAssignHelper(line, reader.ReadLine())) IsNot Nothing
                    lines.Add(line)
                End While
            End Using
        Catch ex As Exception
            Throw New Exception($"ファイル読み込みエラー: {ex.Message}", ex)
        End Try
        
        Return lines
    End Function
    
    ' VB.NETのインライン代入ヘルパー
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
    
    ' ==========================================
    ' 行の分割
    ' ==========================================
    
    Private Function SplitLine(line As String, delimiter As String) As String()
        If delimiter = "," Then
            ' CSV用の引用符対応パーサー
            Return ParseCsvLine(line)
        Else
            ' TSVなどはシンプルに分割
            Return line.Split(New String() {delimiter}, StringSplitOptions.None)
        End If
    End Function
    
    ' CSV専用パーサー（引用符とエスケープに対応）
    Private Function ParseCsvLine(line As String) As String()
        Dim fields As New List(Of String)
        Dim currentField As New StringBuilder()
        Dim inQuotes As Boolean = False
        Dim i As Integer = 0
        
        While i < line.Length
            Dim c = line(i)
            
            If c = """"c Then
                If inQuotes AndAlso i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                    ' エスケープされた引用符 ("" → ")
                    currentField.Append(""""c)
                    i += 1
                Else
                    ' 引用符の開始/終了
                    inQuotes = Not inQuotes
                End If
            ElseIf c = ","c AndAlso Not inQuotes Then
                ' フィールドの区切り
                fields.Add(currentField.ToString())
                currentField.Clear()
            Else
                currentField.Append(c)
            End If
            
            i += 1
        End While
        
        ' 最後のフィールド
        fields.Add(currentField.ToString())
        
        Return fields.ToArray()
    End Function
    
    ' ==========================================
    ' 一意性チェック用のキー収集
    ' ==========================================
    Private Sub CollectUniqueKeys(lines As List(Of String))
        If currentFileDefinition.UniqueColumns Is Nothing OrElse 
           currentFileDefinition.UniqueColumns.Length = 0 Then
            Return
        End If
        
        Dim keyCount As New Dictionary(Of String, Integer)  ' キー→出現回数
        
        For i As Integer = 0 To lines.Count - 1
            Dim fields = SplitLine(lines(i), inputFileInfo.Delimiter)
            Dim key = BuildUniqueKey(fields)
            
            ' 出現回数をカウント
            If keyCount.ContainsKey(key) Then
                keyCount(key) += 1
            Else
                keyCount(key) = 1
            End If
            
            ' 最初に出現した行番号を記録（エラーメッセージ用）
            If Not uniqueKeyCache.ContainsKey(key) Then
                uniqueKeyCache(key) = i + 1
            End If
        Next
        
        ' 2回以上出現したキーを重複キーとして記録
        For Each kv In keyCount
            If kv.Value > 1 Then
                duplicateKeys.Add(kv.Key)
            End If
        Next
    End Sub
    
    Private Function BuildUniqueKey(fields() As String) As String
        If currentFileDefinition.UniqueColumns Is Nothing OrElse 
           currentFileDefinition.UniqueColumns.Length = 0 Then
            Return ""
        End If
        
        Dim keyParts As New List(Of String)
        For Each colIndex In currentFileDefinition.UniqueColumns
            If colIndex < fields.Length Then
                keyParts.Add(fields(colIndex))
            Else
                keyParts.Add("")
            End If
        Next
        
        Return String.Join("_", keyParts)
    End Function

    ' ==========================================
    ' 列数チェック
    ' ==========================================
    Private Function ValidateColumnCount(fields() As String, result As RowValidationResult) As Boolean
        If currentFileDefinition.ExpectedColumnCount <= 0 Then
            Return True  ' 列数チェックが設定されていない場合はスキップ
        End If
        
        If fields.Length < currentFileDefinition.ExpectedColumnCount Then
            result.Errors.Add(New ValidationError With {
                .LineNumber = result.LineNumber,
                .ColumnIndex = -1,
                .ColumnName = "",
                .ErrorType = "列数不足",
                .ErrorMessage = $"列数が不足しています（期待値: {currentFileDefinition.ExpectedColumnCount}列、実際: {fields.Length}列）",
                .RawValue = ""
            })
            Return False
        End If
        
        Return True
    End Function

    ' ==========================================
    ' 行のバリデーション（メインロジック）
    ' ==========================================
    
    Private Function ValidateRow(fields() As String, lineNo As Integer) As RowValidationResult
        Dim result As New RowValidationResult With {
            .LineNumber = lineNo,
            .RawData = String.Join(inputFileInfo.Delimiter, fields)
        }
        
        ' キー項目を抽出
        ExtractKeyValues(fields, result)

        ' ステップ0: 列数チェック（最優先）
        If Not ValidateColumnCount(fields, result) Then
            Return result  ' 列数不足なら他のチェック不要
        End If
    
        
        ' ステップ1: スキップ判定
        If ShouldSkipValidation(fields) Then
            Return result  ' エラーなし = OKデータ扱い
        End If
        
        ' ステップ2: 一意性チェック（最優先）
        If Not ValidateUniqueness(fields, result) Then
            Return result  ' 一意性エラーなら他のチェック不要
        End If
        
        ' ステップ3: 必須チェック（早期リターン）
        If Not ValidateRequired(fields, result) Then
            Return result  ' 必須エラーなら終了
        End If
        
        ' ステップ4: 桁数・日付チェック（両方実行）
        ValidateLength(fields, result)
        ValidateDate(fields, result)
        
        ' ステップ5: カスタムルール
        ValidateCustomRules(fields, result)
        
        Return result
    End Function
    
    ' ==========================================
    ' キー項目の抽出
    ' ==========================================
    
    Private Sub ExtractKeyValues(fields() As String, result As RowValidationResult)
        If currentFileDefinition.KeyColumns Is Nothing Then
            Return
        End If
        
        For Each keyCol In currentFileDefinition.KeyColumns
            If keyCol.Index < fields.Length Then
                result.KeyValues(keyCol.Name) = fields(keyCol.Index)
            Else
                result.KeyValues(keyCol.Name) = ""
            End If
        Next
    End Sub
    
    ' ==========================================
    ' 一意性チェック
    ' ==========================================
    Private Function ValidateUniqueness(fields() As String, result As RowValidationResult) As Boolean
        If currentFileDefinition.UniqueColumns Is Nothing OrElse 
           currentFileDefinition.UniqueColumns.Length = 0 Then
            Return True
        End If
        
        Dim key = BuildUniqueKey(fields)
        
        ' このキーが重複キーセットに含まれている場合、すべてエラー
        If duplicateKeys.Contains(key) Then
            Dim firstLineNo As Integer
            uniqueKeyCache.TryGetValue(key, firstLineNo)
            
            result.Errors.Add(New ValidationError With {
                .LineNumber = result.LineNumber,
                .ColumnIndex = -1,
                .ColumnName = "",
                .ErrorType = "一意性制約",
                .ErrorMessage = $"重複データ（初出: {firstLineNo}行目）",
                .RawValue = key
            })
            Return False
        End If
        
        Return True
    End Function
    
    ' ==========================================
    ' 必須チェック
    ' ==========================================
    
    Private Function ValidateRequired(fields() As String, result As RowValidationResult) As Boolean
        If currentFileDefinition.RequiredColumns Is Nothing Then
            Return True
        End If
        
        For Each reqCol In currentFileDefinition.RequiredColumns
            If reqCol.Index >= fields.Length OrElse String.IsNullOrWhiteSpace(fields(reqCol.Index)) Then
                result.Errors.Add(New ValidationError With {
                    .LineNumber = result.LineNumber,
                    .ColumnIndex = reqCol.Index,
                    .ColumnName = reqCol.Name,
                    .ErrorType = "必須",
                    .ErrorMessage = "必須項目が未入力です",
                    .RawValue = If(reqCol.Index < fields.Length, fields(reqCol.Index), "")
                })
                Return False  ' 必須エラーで早期リターン
            End If
        Next
        
        Return True
    End Function
    
    ' ==========================================
    ' 桁数チェック
    ' ==========================================
    
    Private Sub ValidateLength(fields() As String, result As RowValidationResult)
        If currentFileDefinition.LengthRules Is Nothing Then
            Return
        End If
        
        For Each rule In currentFileDefinition.LengthRules
            If rule.ColumnIndex >= fields.Length Then
                Continue For
            End If
            
            Dim value = fields(rule.ColumnIndex)
            Dim length = value.Length
            
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
    
    ' ==========================================
    ' 日付形式チェック
    ' ==========================================
    
    Private Sub ValidateDate(fields() As String, result As RowValidationResult)
        If currentFileDefinition.DateColumns Is Nothing Then
            Return
        End If
        
        For Each dateCol In currentFileDefinition.DateColumns
            If dateCol.Index >= fields.Length Then
                Continue For
            End If
            
            Dim value = fields(dateCol.Index)
            
            ' 空欄はスキップ（必須チェックで別途チェック済み）
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
    
    ' ==========================================
    ' OKデータ出力
    ' ==========================================
    
    Private Sub OutputOKData(outputDir As String)
        If OKData.Count = 0 Then
            Console.WriteLine("OKデータ: 0件（出力なし）")
            Return
        End If
        
        Dim fileName = $"{currentFileDefinition.OutputFileName}_OK_data{inputFileInfo.Extension}"
        Dim filePath = Path.Combine(outputDir, fileName)
        
        Try
            Using writer As New StreamWriter(filePath, False, Encoding.UTF8)
                For Each line In OKData
                    writer.WriteLine(line)
                Next
            End Using
            
            Console.WriteLine($"OKデータ出力: {filePath} ({OKData.Count}件)")
        Catch ex As Exception
            Console.WriteLine($"OKデータ出力エラー: {ex.Message}")
        End Try
    End Sub
    
    ' ==========================================
    ' NGログ出力
    ' ==========================================
    
    Private Sub OutputNGLog(outputDir As String)
        If NGResults.Count = 0 Then
            Console.WriteLine("NGデータ: 0件（出力なし）")
            Return
        End If
        
        Dim fileName = $"{currentFileDefinition.OutputFileName}_NG_data.log"
        Dim filePath = Path.Combine(outputDir, fileName)
        
        Try
            Using writer As New StreamWriter(filePath, False, Encoding.UTF8)
                ' ヘッダー
                writer.WriteLine("=" & New String("="c, 70))
                writer.WriteLine($"{currentFileDefinition.Name} - エラーデータログ")
                writer.WriteLine($"生成日時: {DateTime.Now:yyyy/MM/dd HH:mm:ss}")
                writer.WriteLine($"総エラー件数: {NGResults.Count}件")
                writer.WriteLine("=" & New String("="c, 70))
                writer.WriteLine()
                
                ' 各エラーデータ
                For i As Integer = 0 To NGResults.Count - 1
                    Dim result = NGResults(i)
                    
                    writer.WriteLine($"[{i + 1}] 行番号: {result.LineNumber}")
                    
                    ' キー項目
                    If result.KeyValues.Count > 0 Then
                        writer.WriteLine("  キー項目:")
                        For Each kv In result.KeyValues
                            writer.WriteLine($"    {kv.Key}: {kv.Value}")
                        Next
                    End If
                    
                    ' エラー内容
                    writer.WriteLine("  エラー内容:")
                    For Each err In result.Errors
                        If String.IsNullOrEmpty(err.ColumnName) Then
                            writer.WriteLine($"    ・[{err.ErrorType}] {err.ErrorMessage}")
                        Else
                            writer.WriteLine($"    ・[{err.ErrorType}] {err.ColumnName}: {err.ErrorMessage}")
                        End If
                        
                        If Not String.IsNullOrEmpty(err.RawValue) Then
                            writer.WriteLine($"      値: {err.RawValue}")
                        End If
                    Next
                    
                    writer.WriteLine()
                    writer.WriteLine(New String("-"c, 70))
                    writer.WriteLine()
                Next
            End Using
            
            Console.WriteLine($"NGログ出力: {filePath} ({NGResults.Count}件)")
        Catch ex As Exception
            Console.WriteLine($"NGログ出力エラー: {ex.Message}")
        End Try
    End Sub
    
End Class