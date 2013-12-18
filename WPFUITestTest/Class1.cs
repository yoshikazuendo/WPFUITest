using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Automation;
using NUnit.Framework;

namespace WPFUITestTest
{
	/// <summary>
	/// MUIAとNUnitを使った自動テスト方法のサンプル。 
	/// </summary>
	[TestFixture]
	public class Class1
	{
		/// <summary>
		/// テスト対象のウィンドウ 
		/// </summary>
		private AutomationElement aeWindow;

		[SetUp]
		public void SetUp()
		{
			// テスト対象のアプリケーションを起動 
			Process p = Process.Start("..\\..\\..\\WPFUITest\\bin\\Debug\\WPFUITest.exe");
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
			this.aeWindow = null;
			int numWaits = 0;
			do {
				Console.WriteLine("Looking for CryptoCalc main window. . . ");
				this.aeWindow = aeDesktop.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Window1"));
				++numWaits;
				Thread.Sleep(200);
			} while (this.aeWindow == null && numWaits < 50);
			// テストアプリが取得できたか？ 
			if (this.aeWindow == null) throw new Exception("Failed to find CryptoCalc main window");
			else Console.WriteLine("Found CryptoCalc main window");
		}

		[TestCase]
		public void DESEncryptTest()
		{
			// 操作するコントロールを取得する。 
			var aeTxtInput = this.FindElement(this.aeWindow, "TextBox1");
			var aeTxtOutput = this.FindElement(this.aeWindow, "TextBox2");
			var aeButton = this.FindElement(this.aeWindow, "Button1");
			var aeRadioButton = this.FindElement(this.aeWindow, "RadioButton3");

			// 操作するコントロールパターンを取得する。 
			var vpTxtInput = (ValuePattern)aeTxtInput.GetCurrentPattern(ValuePattern.Pattern);
			var tpTxtOutput = (TextPattern)aeTxtOutput.GetCurrentPattern(TextPattern.Pattern);
			var ipButton = (InvokePattern)aeButton.GetCurrentPattern(InvokePattern.Pattern);
			var spRadioButton = (SelectionItemPattern)aeRadioButton.GetCurrentPattern(SelectionItemPattern.Pattern);

			// 操作する。 
			vpTxtInput.SetValue("Hello!");
			this.CopyScreen(this.aeWindow, "DESEncryptTest1");
			spRadioButton.Select();
			this.CopyScreen(this.aeWindow, "DESEncryptTest2");
			ipButton.Invoke();
			this.CopyScreen(this.aeWindow, "DESEncryptTest3");
			Thread.Sleep(1500);
			string value = tpTxtOutput.DocumentRange.GetText(-1);

			Assert.AreEqual("91-1E-84-41-67-4B-FF-8F", value);
		}

		[TearDown]
		public void TearDown()
		{
			// テストアプリを終了する。 
			var wpWindows = (WindowPattern)this.aeWindow.GetCurrentPattern(WindowPattern.Pattern);
			wpWindows.Close();
		}

		/// <summary>
		/// 画面のキャプチャを取ります。 
		/// </summary>
		/// <param name="targetWindow"></param>
		/// <param name="bmpFileName"></param>
		private void CopyScreen(AutomationElement targetWindow, string bmpFileName)
		{
			// 直前の処理がUIに確実に反映されるようにするために、ウェイトを入れておく。 
			Thread.Sleep(500);

			int xPoint = (int)targetWindow.Current.BoundingRectangle.X;
			int yPoint = (int)targetWindow.Current.BoundingRectangle.Y;
			int width = (int)targetWindow.Current.BoundingRectangle.Size.Width;
			int height = (int)targetWindow.Current.BoundingRectangle.Size.Height;
			Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			using (var g = Graphics.FromImage(bmp)) {
				g.CopyFromScreen(xPoint
					, yPoint
					, 0
					, 0
					, new System.Drawing.Size(width, height)
					, CopyPixelOperation.SourceCopy);
			}
			string filePath = Path.Combine(System.Environment.CurrentDirectory, bmpFileName + ".bmp");
			bmp.Save(filePath, ImageFormat.Bmp);
		}

		private AutomationElement FindElement(AutomationElement rootElement, string automationID)
		{
			var element = rootElement.FindFirst(TreeScope.Element | TreeScope.Descendants,
				new PropertyCondition(AutomationElement.AutomationIdProperty, automationID));
			return element;
		}
	}
}