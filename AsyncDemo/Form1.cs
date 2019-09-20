using System;
using System.IO;
using System.Windows.Forms;

namespace AsyncDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void btnCopyFiles_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtSource.Text))
            {
                MessageBox.Show("Invalid Input Folder: " + txtSource.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnCopyFiles.Enabled = false;
            btnExit.Enabled = false;

            if (!Directory.Exists(txtDestination.Text))
            {
                Directory.CreateDirectory(txtDestination.Text);
            }

            var ProgressPercent = new Progress<int>(percent =>
            {
                lblProgress.Text = percent + "%";
                ProgressBar1.Value = percent;
            });

            MyFileCopy myFileCopy = new MyFileCopy(ProgressPercent);

            try
            {
                await myFileCopy.CopyAll(txtSource.Text, txtDestination.Text);
            }
            catch(Exception Ex)
            {
                MessageBox.Show("Error in Copying : " + Ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCopyFiles.Enabled = true;
                btnExit.Enabled = true;
            }
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FolderDlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = false,
                Description = "Select Source Folder"
            };

            // Show the FolderBrowserDialog.  
            DialogResult result = FolderDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                txtSource.Text = FolderDlg.SelectedPath;
            }
        }

        private void btnBrowseDestination_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FolderDlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "Select Destination Folder"
            };

            // Show the FolderBrowserDialog.  
            DialogResult result = FolderDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                txtDestination.Text = FolderDlg.SelectedPath;
            }
        }
    }
}
