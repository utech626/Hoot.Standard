using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RaptorDB;
using RaptorDB.Filters;

namespace SampleApp
{
	public partial class Form1 : Form
	{
		private Hoot hoot;
		private DateTime _indextime;
		private BackgroundWorker backgroundWorker1;

		public Form1()
		{
			InitializeComponent();

			backgroundWorker1 = new BackgroundWorker();
			backgroundWorker1.WorkerReportsProgress = true;
			backgroundWorker1.WorkerSupportsCancellation = true;
			backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
			backgroundWorker1.ProgressChanged += BackgroundWorker1_ProgressChanged;
			backgroundWorker1.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
		}

		private void groupBox3_Enter(object sender, EventArgs e)
		{

		}
		/// <summary>
		/// Count number of works in index
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button2_Click(object sender, EventArgs e)
		{
			if (hoot == null)
				loadhoot();
			MessageBox.Show("Words = " + hoot.WordCount.ToString("#,#") + "\r\nDocuments = " + hoot.DocumentCount.ToString("#,#"));
		}
		/// <summary>
		/// Save the Index
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button4_Click(object sender, EventArgs e)
		{
			if (hoot != null)
				hoot.Save();
		}
		/// <summary>
		/// Free Memory
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button3_Click(object sender, EventArgs e)
		{
			if (hoot == null)
				loadhoot();
			else
				return;
			// free memory
			hoot.FreeMemory();
			GC.Collect(GC.MaxGeneration);
		}
		/// <summary>
		/// Save Words
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button5_Click(object sender, EventArgs e)
		{
			if (hoot == null)
				loadhoot();

			var sb = new StringBuilder();
			var w = hoot.Words.ToList();
			w.Sort();
			w.ForEach(x => sb.AppendLine(x));
			
			File.WriteAllText(Path.Combine(hoot.HootConfOptions.IndexPath, $"{hoot.HootConfOptions.FileName}_words.txt"), sb.ToString(), Encoding.UTF8);
		}
		/// <summary>
		/// perform a search
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSearch_Click(object sender, EventArgs e)
		{
			if (hoot == null)
				loadhoot();

			listBox1.Items.Clear();
			DateTime dt = DateTime.Now;
			listBox1.BeginUpdate();

			foreach (var d in hoot.FindDocumentFileNames(txtSearch.Text))
			{
				listBox1.Items.Add(d);
			}
			listBox1.EndUpdate();
			lblStatus.Text = "Search = " + listBox1.Items.Count + " items, " + DateTime.Now.Subtract(dt).TotalSeconds + " s";
		}
		/// <summary>
		/// Set Folder to Index
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button7_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();

			fbd.SelectedPath = txtWhere.Text;
			if (fbd.ShowDialog() == DialogResult.OK)
				txtWhere.Text = fbd.SelectedPath;
		}
		/// <summary>
		/// Start the indexing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnStart_Click(object sender, EventArgs e)
		{
			if (txtIndexFolder.Text == "" || txtWhere.Text == "")
			{
				MessageBox.Show("Please supply the index storage folder and the where to start indexing from.");
				return;
			}

			btnStart.Enabled = false;
			btnStop.Enabled = true;
			loadhoot();

			string[] files = Directory.GetFiles(txtWhere.Text, "*", SearchOption.AllDirectories);
			_indextime = DateTime.Now;
			backgroundWorker1.RunWorkerAsync(files);
		}
		/// <summary>
		/// Select Folder for index file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button6_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();

			fbd.SelectedPath = Directory.GetCurrentDirectory();
			if (fbd.ShowDialog() == DialogResult.OK)
				txtIndexFolder.Text = fbd.SelectedPath;
		}
		/// <summary>
		/// Background Worker processor
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			string[] files = e.Argument as string[];
			BackgroundWorker wrk = sender as BackgroundWorker;
			int i = 0;

			SetButton1Visiblablity(false);
			foreach (string fn in files)
			{
				if (wrk.CancellationPending)
				{
					e.Cancel = true;
					break;
				}
				backgroundWorker1.ReportProgress(1, fn);
				try
				{
					if (hoot.IsIndexed(fn) == false)
					{
						using (TextReader tf = File.OpenText(fn))
						{
							string s = "";

							if (tf != null)
								s = tf.ReadToEnd();

							if (s != "")
							{
								if (Path.GetExtension(fn).Equals(".html", StringComparison.OrdinalIgnoreCase))
									hoot.Index(new myDoc(new FileInfo(fn), s), true, new HtmlFilter());
								else
									hoot.Index(new myDoc(new FileInfo(fn), s), true);
							}
						}
					}
				}
				catch
				{
				}
				i++;
				if (i > 1000)
				{
					i = 0;
					hoot.Save();
				}
			}
			hoot.Save();
			SetButton1Visiblablity(true);
			//hoot.OptimizeIndex();
		}

		private delegate void SafeCallDelegate(bool isVisible);

		/// <summary>
		/// Set Load Hoot Button Visible
		/// </summary>
		/// <param name="isVisable"></param>
		private void SetButton1Visiblablity(bool isVisible)
		{
			if (button1.InvokeRequired)
			{
				var _d = new SafeCallDelegate(SetButton1Visiblablity);
				button1.Invoke(_d, new object[] { isVisible });
			}
			else
				button1.Visible = isVisible;
		}
		/// <summary>
		/// Progress Changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			lblIndexer.Text = "" + e.UserState;
		}
		/// <summary>
		/// Background Worker completed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			btnStart.Enabled = true;
			btnStop.Enabled = false;
			lblIndexer.Text = "" + DateTime.Now.Subtract(_indextime).TotalSeconds + " sec";
			MessageBox.Show("Indexing done : " + DateTime.Now.Subtract(_indextime).TotalSeconds + " sec");
		}
		/// <summary>
		/// Stop The Indexer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnStop_Click(object sender, EventArgs e)
		{
			backgroundWorker1.CancelAsync();
		}

		private void txtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				btnSearch_Click(null, null);
		}
		/// <summary>
		/// Load Hoot
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button1_Click(object sender, EventArgs e)
		{
			loadhoot();
		}
		/// <summary>
		/// Load Hoot
		/// </summary>
		private void loadhoot()
		{
			if (txtIndexFolder.Text == "")
			{
				MessageBox.Show("Please supply the index storage folder.");
				return;
			}

			HootConfig _config = new HootConfig
			{
				IndexPath = Path.GetFullPath(txtIndexFolder.Text),
				FileName = "index",
				DocMode = true,
				UseStopList = cbUseStopList.Checked,
				IgnoreNumerics = cbIgnoreNumeric.Checked
			};

			hoot = new Hoot(_config);
			button1.Enabled = false;
		}
		/// <summary>
		/// Double Click on Results list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void listBox1_DoubleClick(object sender, EventArgs e)
		{
			String _ext = Path.GetExtension(listBox1.SelectedItem.ToString().ToLower());
			if (String.IsNullOrEmpty(_ext) || (_ext.Equals(".txt")))
				Process.Start("notepad", listBox1.SelectedItem.ToString());
			else
				Process.Start("edge", listBox1.SelectedItem.ToString());
		}
		/// <summary>
		/// The form is closing shutdown hoot indexer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (hoot != null)
				hoot.Shutdown();
		}
	}
}
