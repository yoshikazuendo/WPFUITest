using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.IO;

namespace WPFUITest
{
	/// <summary>
	/// Window1.xaml の相互作用ロジック
	/// </summary>
	public partial class Window1 : Window
	{
		public Window1()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Button1_Click Event 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button1_Click(object sender, RoutedEventArgs e)
		{
			// Todo：エラーチェックを入れる。 
			string input = this.TextBox1.Text;
			byte[] inputBytes = Encoding.UTF8.GetBytes(input);

			if (this.RadioButton1.IsChecked == true) {
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
				byte[] hashedBytes = md5.ComputeHash(inputBytes);
				this.TextBox2.Text = BitConverter.ToString(hashedBytes);
			} else if (this.RadioButton2.IsChecked == true) {
				SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
				byte[] hashedBytes = sha.ComputeHash(inputBytes);
				this.TextBox2.Text = BitConverter.ToString(hashedBytes);
			} else if (this.RadioButton3.IsChecked == true) {
				DESCryptoServiceProvider des = new DESCryptoServiceProvider();
				byte[] blanks = System.Text.Encoding.UTF8.GetBytes("        "); // 8 spaces 
				des.Key = blanks;
				des.IV = blanks;
				des.Padding = PaddingMode.Zeros;
				MemoryStream ms = new MemoryStream();
				CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
				cs.Write(inputBytes, 0, inputBytes.Length);
				cs.Close();
				byte[] encryptedBytes = ms.ToArray();
				ms.Close();
				this.TextBox2.Text = BitConverter.ToString(encryptedBytes);
			}
		}

		private void FileExit_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
