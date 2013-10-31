using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Windows.Automation;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;

namespace Harness
{
	/// <summary>
	/// Microsoft UI オートメーション（MUIA）を使ったテスト方法のサンプル。 
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			// テスト自動化の一般的な処理として、ハーネスの最上位のtry/catch ブロックでラップして致命的なエラーに対処している。 
			try {
				Console.WriteLine("\nBegin WPF UIAutomation test run\n");
				// テストアプリを起動。 
				Process p = null;
				p = Process.Start("..\\..\\..\\WPFUITest\\bin\\Debug\\WPFUITest.exe");

				// sleep（遅延ループ） 
				int ct = 0;
				do {
					Console.WriteLine("Looking for CryptoCalc process. . . ");
					++ct;
					Thread.Sleep(100);
				} while (p == null && ct < 50);

				// 遅延ループがタイムアウトになったか、またはAUTプロセスが見つかったかを判断する。 
				if (p == null) throw new Exception("Failed to find CryptoCalc process");
				else Console.WriteLine("Found CryptoCalc process");

				// テストアプリを参照するため、全ての親WindowとなるDesktopを取得する。 
				Console.WriteLine("\nGetting Desktop");
				AutomationElement aeDesktop = null;
				aeDesktop = AutomationElement.RootElement;
				// DeskTopが取得できたか？ 
				if (aeDesktop == null) throw new Exception("Unable to get Desktop");
				else Console.WriteLine("Found Desktop\n");

				// DeskTopからテストアプリを取得する。 
				AutomationElement aeCryptoCalc = null;
				int numWaits = 0;
				do {
					Console.WriteLine("Looking for CryptoCalc main window. . . ");
					aeCryptoCalc = aeDesktop.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Window1"));
					++numWaits;
					Thread.Sleep(200);
				} while (aeCryptoCalc == null && numWaits < 50);
				// テストアプリが取得できたか？ 
				if (aeCryptoCalc == null) throw new Exception("Failed to find CryptoCalc main window");
				else Console.WriteLine("Found CryptoCalc main window");

#if false // 自動テストがてら、エビデンスのキャプチャもとっちゃおう！というコード
				// テストアプリのウィンドウ位置を取得する（キャプチャに使用） 
				var rtWindow = aeCryptoCalc.Current.BoundingRectangle;
#endif

				// テストアプリからボタンを取得する。 
				// Buttonコントロールは静的コントロールであるため、コントロールへの参照にアクセスする前に遅延ループ手法を使用する必要はない。（動的コントロールの場合は必要になる！） 
				Console.WriteLine("\nGetting all user controls");
				AutomationElement aeButton = null;
				aeButton = aeCryptoCalc.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Compute"));
				// ボタンが取得できたか？ 
				if (aeButton == null) throw new Exception("No compute button");
				else Console.WriteLine("Got Compute button");

				// テストアプリからテキストボックスを取得する。 
#if false
				// TextBoxコントロール（に限るのかな？）はNameプロパティを受け取らないため、AutomationIdPropertyを使用する。 
				AutomationElement aeTextBox1 = null;
				aeTextBox1 = aeCryptoCalc.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "TextBox1"));
#else
				// または、こんな方法もあるよ！ 
				AutomationElementCollection aeAllTextBoxes = null;
				aeAllTextBoxes = aeCryptoCalc.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
				if (aeAllTextBoxes == null) throw new Exception("No textboxes collection");
				else Console.WriteLine("Got textboxes collection");

				AutomationElement aeTextBox1 = null;
				AutomationElement aeTextBox2 = null;
				aeTextBox1 = aeAllTextBoxes[0];
				aeTextBox2 = aeAllTextBoxes[1];
				if (aeTextBox1 == null || aeTextBox2 == null) throw new Exception("TextBox1 or TextBox2 not found");
				else Console.WriteLine("Got TextBox1 and TextBox2");
#endif

				// テストアプリからラジオボタンを取得する。 
				AutomationElement aeRadioButton3 = null;
				aeRadioButton3 = aeCryptoCalc.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "DES Encrypt"));
				if (aeRadioButton3 == null) throw new Exception("No RadioButton3");
				else Console.WriteLine("Got RadioButton3");


				// テキストボックス1にHello!を入力する。 
				// 直接参照するのではなく、ValuePatternオブジェクト経由のSetValueで入力する。 
				Console.WriteLine("\nSetting input to 'Hello!'");
				ValuePattern vpTextBox1 = (ValuePattern)aeTextBox1.GetCurrentPattern(ValuePattern.Pattern);
				vpTextBox1.SetValue("Hello!");

#if false // 自動テストがてら、エビデンスのキャプチャもとっちゃおう！というコード
				// キャプチャをとる。 
				Bitmap bmp = new Bitmap((int)rtWindow.Width, (int)rtWindow.Height, PixelFormat.Format32bppArgb);
				using (var g = Graphics.FromImage(bmp)) {
					g.CopyFromScreen(int.Parse(rtWindow.X.ToString())
						, int.Parse(rtWindow.Y.ToString())
						, 0
						, 0
						, new System.Drawing.Size((int)rtWindow.Size.Width, (int)rtWindow.Size.Height)
						, CopyPixelOperation.SourceCopy);
				}
				string filePath = @"c:\screen.bmp";
				bmp.Save(filePath, ImageFormat.Bmp);
#endif

				// ラジオボタンにチェックを入れる。 
				Console.WriteLine("Selecting 'DES Encrypt' ");
				SelectionItemPattern spSelectRadioButton3 = (SelectionItemPattern)aeRadioButton3.GetCurrentPattern(SelectionItemPattern.Pattern);
				spSelectRadioButton3.Select();

				// ボタンをクリックする。 
				Console.WriteLine("\nClicking on Compute button");
				InvokePattern ipClickButton1 = (InvokePattern)aeButton.GetCurrentPattern(InvokePattern.Pattern);
				ipClickButton1.Invoke();
				Thread.Sleep(1500);

				// テキストボックス2の値を確認する。 
				Console.WriteLine("\nChecking TextBox2 for '91-1E-84-41-67-4B-FF-8F'");
				TextPattern tpTextBox2 = (TextPattern)aeTextBox2.GetCurrentPattern(TextPattern.Pattern);
				string result = tpTextBox2.DocumentRange.GetText(-1);
				// これでもOK 
				//string result = (string)aeTextBox2.GetCurrentPropertyValue(ValuePattern.ValueProperty);
				if (result == "91-1E-84-41-67-4B-FF-8F") {
					Console.WriteLine("Found it");
					Console.WriteLine("\nTest scenario: Pass");
				} else {
					Console.WriteLine("Did not find it");
					Console.WriteLine("\nTest scenario: *FAIL*");
				}


				// Menuコントロールを調べ、テストアプリを終了する。 
				Console.WriteLine("\nClicking on File-Exit item in 5 seconds");
				Thread.Sleep(5000);
				AutomationElement aeFile = null;
				aeFile = aeCryptoCalc.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "File"));
				if (aeFile == null)
					throw new Exception("Could not find File menu");
				else
					Console.WriteLine("Got File menu");

				// [File]項目を展開する。 
				Console.WriteLine("Clicking on 'File'");
				ExpandCollapsePattern expClickFile = (ExpandCollapsePattern)aeFile.GetCurrentPattern(ExpandCollapsePattern.Pattern);
				expClickFile.Expand();

				// [Exit]をクリックする。 
				AutomationElement aeFileExit = null;
				aeFileExit = aeCryptoCalc.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Exit"));
				if (aeFileExit == null) throw new Exception("Could not find File-Exit");
				else Console.WriteLine("Got File-Exit");
				InvokePattern ipFileExit = (InvokePattern)aeFileExit.GetCurrentPattern(InvokePattern.Pattern);
				ipFileExit.Invoke();
				Console.WriteLine("\nEnd automation\n");


				// 
				Console.WriteLine("\nEnd automation\n");
			} catch (Exception ex) {
				Console.WriteLine("Fatal: " + ex.Message);
			}
		}
	}
}