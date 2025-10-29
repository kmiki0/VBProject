' ==========================================
' Module1.vb
' 顧客マスタバリデータのエントリーポイント
' ==========================================
Imports System.IO

Module Module1
    
    Sub Main(args() As String)
        Try
            ' バナー表示
            DisplayBanner()
            
            ' コマンドライン引数チェック
            If args.Length < 2 Then
                DisplayUsage()
                Environment.Exit(1)
            End If
            
            Dim inputFile As String = args(0)
            Dim outputDir As String = args(1)
            
            ' 入力ファイルチェック
            If Not File.Exists(inputFile) Then
                Console.WriteLine()
                Console.WriteLine($"エラー: 入力ファイルが見つかりません")
                Console.WriteLine($"パス: {inputFile}")
                Environment.Exit(1)
            End If
            
            ' 出力フォルダ作成
            If Not Directory.Exists(outputDir) Then
                Directory.CreateDirectory(outputDir)
                Console.WriteLine($"出力フォルダを作成しました: {outputDir}")
            End If
            
            ' ファイル情報表示
            Console.WriteLine()
            Console.WriteLine($"入力ファイル: {inputFile}")
            Console.WriteLine($"出力フォルダ: {outputDir}")
            Console.WriteLine()
            Console.WriteLine(New String("-"c, 70))
            Console.WriteLine()
            
            ' バリデーション実行
            Dim validator As New CustomerValidator()
            validator.ValidateFile(inputFile, outputDir)
            
            ' 完了メッセージ
            Console.WriteLine()
            Console.WriteLine(New String("-"c, 70))
            Console.WriteLine()
            Console.WriteLine("処理が正常に完了しました。")
            Console.WriteLine()
            
            Environment.Exit(0)
            
        Catch ex As Exception
            ' エラーハンドリング
            Console.WriteLine()
            Console.WriteLine("=" & New String("="c, 70))
            Console.WriteLine("エラーが発生しました")
            Console.WriteLine("=" & New String("="c, 70))
            Console.WriteLine()
            Console.WriteLine($"エラー内容: {ex.Message}")
            Console.WriteLine()
            Console.WriteLine("スタックトレース:")
            Console.WriteLine(ex.StackTrace)
            Console.WriteLine()
            
            Environment.Exit(1)
        End Try
    End Sub
    
    ' バナー表示
    Private Sub DisplayBanner()
        Console.WriteLine()
        Console.WriteLine("========================================")
        Console.WriteLine("  顧客マスタ バリデーションツール")
        Console.WriteLine("  Version 1.0")
        Console.WriteLine("========================================")
        Console.WriteLine()
    End Sub
    
    ' 使用方法表示
    Private Sub DisplayUsage()
        Console.WriteLine()
        Console.WriteLine("使用方法:")
        Console.WriteLine("  CustomerValidator.exe <入力ファイル> <出力フォルダ>")
        Console.WriteLine()
        Console.WriteLine("例:")
        Console.WriteLine("  CustomerValidator.exe C:\data\customer.csv C:\output")
        Console.WriteLine("  CustomerValidator.exe C:\data\customer.tsv C:\output")
        Console.WriteLine()
        Console.WriteLine("対応形式:")
        Console.WriteLine("  - CSV (.csv)")
        Console.WriteLine("  - TSV (.tsv)")
        Console.WriteLine("  - テキスト (.txt) ※自動判定")
        Console.WriteLine()
    End Sub
    
End Module
