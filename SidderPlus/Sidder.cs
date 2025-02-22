﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;

namespace SidderApp
{
    public partial class Sidder : Form
    {
        private PrincipalContext principalContext = new PrincipalContext(ContextType.Domain);
        private ListView originalListViewUVHDFiles = new ListView();

        public Sidder()
        {
            InitializeComponent();
            this.listViewUVHDFiles.ListViewItemSorter = new Sorter();
        }


        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            FolderSelect.FolderSelectDialog selectDialog = new FolderSelect.FolderSelectDialog();
            selectDialog.Title = "Select a folder containing UVHD files";
            if (!String.IsNullOrEmpty(textBoxFilePathUVHD.Text))
            {
                selectDialog.InitialDirectory = textBoxFilePathUVHD.Text + "\\";
            }
            bool result = selectDialog.ShowDialog(this.Handle);           

            if (result)
            {
                string filePathUVHD = selectDialog.FileName;
                textBoxFilePathUVHD.Text = filePathUVHD;
            }
        }

        /// <summary>
        /// Extract SID from filename and grab corresponding <domain>\<username>
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string ConvertUVHDtoUsername(string fileName)
        {
            string returnValue = String.Empty;

            fileName = fileName.ToUpper();

            try
            {
                if (fileName.Substring(0, 5) != "UVHD-" || fileName.Substring(fileName.Length - 5, 5) != ".VHDX")
                {
                    returnValue = "Filename Error";
                }
                else
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(principalContext, (fileName.Substring(5, fileName.Length - 10)));
                    returnValue = user.UserPrincipalName; 
                }

            }
            catch
            {
                returnValue = "SID Resolve Error";
            }

            return returnValue;
        }

        private void listViewUVHDFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            Sorter s = (Sorter)listViewUVHDFiles.ListViewItemSorter;
            s.Column = e.Column;

            if (s.Order == System.Windows.Forms.SortOrder.Ascending)
            {
                s.Order = System.Windows.Forms.SortOrder.Descending;
            }
            else
            {
                s.Order = System.Windows.Forms.SortOrder.Ascending;
            }
            listViewUVHDFiles.Sort();
        }

        private void Sidder_Load(object sender, EventArgs e)
        {
            if(!String.IsNullOrEmpty(Properties.Settings.Default.userUVHDFilePath)){this.textBoxFilePathUVHD.Text = Properties.Settings.Default.userUVHDFilePath;}
            this.listViewUVHDFiles.Columns[0].Width = Properties.Settings.Default.userColumn1;
            this.listViewUVHDFiles.Columns[1].Width = Properties.Settings.Default.userColumn2;
            this.listViewUVHDFiles.Columns[2].Width = Properties.Settings.Default.userColumn3;
            this.listViewUVHDFiles.Columns[3].Width = Properties.Settings.Default.userColumn4;
            this.Width = Properties.Settings.Default.sidderWidth;
            this.Height = Properties.Settings.Default.sidderHeight;

        }

        private void Sidder_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.userUVHDFilePath = this.textBoxFilePathUVHD.Text;
            Properties.Settings.Default.userColumn1 = this.listViewUVHDFiles.Columns[0].Width;
            Properties.Settings.Default.userColumn2 = this.listViewUVHDFiles.Columns[1].Width;
            Properties.Settings.Default.userColumn3 = this.listViewUVHDFiles.Columns[2].Width;
            Properties.Settings.Default.userColumn4 = this.listViewUVHDFiles.Columns[3].Width;
            Properties.Settings.Default.sidderWidth = this.Width;
            Properties.Settings.Default.sidderHeight = this.Height;
            Properties.Settings.Default.Save();
        }

        private void textBoxFilePathUVHD_TextChanged(object sender, EventArgs e)
        {
            string filePathUVHD = textBoxFilePathUVHD.Text;

            if (filePathUVHD.ToLower() == textBoxFilePathUVHDCurrent.Text) { return; }

            refreshListBox(filePathUVHD);

            textBoxFilePathUVHDCurrent.Text = filePathUVHD.ToLower();
        }

        private void refreshListBox(string filePath)
        {
            buttonRefresh.Enabled = false;
            listViewUVHDFiles.Enabled = false;
            
            try
            {
                DirectoryInfo directory = new DirectoryInfo(filePath);
                if (!directory.Exists)
                {
                    textBoxStatus.Text = "Folder not found.";
                    return;
                }

                FileInfo[] files = directory.GetFiles("UVHD-*.vhdx");

                if (files.GetLength(0) < 1)
                {
                    textBoxStatus.Text = "No UVHD Profile disks found in current folder.";
                    return;
                }

                textBoxStatus.Text = "Refreshing..";

                listViewUVHDFiles.Items.Clear();
                originalListViewUVHDFiles.Items.Clear();

                foreach (FileInfo file in files)
                {
                    int fileLock = 0;
                    if (IsFileLocked(file)) { fileLock = 1; }
                    ListViewItem item = new ListViewItem(file.Name, fileLock);
                    item.SubItems.Add(file.LastWriteTime.ToString());
                    item.SubItems.Add(ConvertUVHDtoUsername(file.Name));
                    long fileSize = file.Length / 1024 / 1024;
                    item.SubItems.Add(fileSize.ToString());
                    item.SubItems.Add(file.FullName);

                    listViewUVHDFiles.Items.Add(item);
                }

                // This is necessary for the search
                for (var i = 0;  i < listViewUVHDFiles.Items.Count; i++)
                {
                    originalListViewUVHDFiles.Items.Add(listViewUVHDFiles.Items[i].Clone() as ListViewItem);
                }

                SearchForUsers(textBoxSearch.Text);

                textBoxStatus.Text = String.Format("Folder processed. {0} UVHD Profile disks found.", originalListViewUVHDFiles.Items.Count.ToString());
            }
            catch (Exception e)
            {
                textBoxStatus.Text = e.Message; // "Error getting UVHD files. Try restarting Sidder with administrative rights.";
            }

            buttonRefresh.Enabled = true;
            listViewUVHDFiles.Enabled = true;
            textBoxSearch.Enabled = true;
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            refreshListBox(textBoxFilePathUVHD.Text);
        }

        private void listViewUVHDFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewUVHDFiles.SelectedItems.Count < 1) 
            {
                buttonDelete.Enabled = false;
                buttonClose.Enabled = false;
                buttonCompactUPD.Enabled = false;
                return;
            }

            buttonDelete.Enabled = true;
            buttonClose.Enabled = true;
            buttonCompactUPD.Enabled = true;
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {

            ActionBox deleteBox = new ActionBox("UVHD files to delete", "Delete", "DeleteBox");
            deleteBox.listViewUVHDFiles.Items.Clear();

            foreach(ListViewItem item in listViewUVHDFiles.SelectedItems)
            {
                ListViewItem deleteItem = new ListViewItem(item.Text, item.ImageIndex);
                deleteItem.SubItems.Add(item.SubItems[2].Text);
                deleteItem.SubItems.Add(item.SubItems[4].Text);
                deleteBox.listViewUVHDFiles.Items.Add(deleteItem);
            }

            DialogResult result = deleteBox.ShowDialog();

            if (result == DialogResult.Cancel) { return; }

            if (result == DialogResult.OK)
            {
                foreach(ListViewItem item in deleteBox.listViewUVHDFiles.Items)
                {
                    try
                    {
                        File.Delete(item.SubItems[2].Text);
                    }
                    catch (Exception exeception)
                    {
                        textBoxStatus.Text = exeception.Message;
                    }
                }

                refreshListBox(textBoxFilePathUVHD.Text);
            }
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return false;
        }

        private bool IsElevated()
        {
            bool isElevated = false;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return isElevated;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            // We need admin permissions to close an open smb file.
            if (IsElevated())
            {
                PowerShell ps = PowerShell.Create();

                ActionBox closeBox = new ActionBox("UVHD files to close", "Close", "CloseBox");
                closeBox.listViewUVHDFiles.Items.Clear();

                foreach (ListViewItem item in listViewUVHDFiles.SelectedItems)
                {
                    ListViewItem closeItem = new ListViewItem(item.Text, item.ImageIndex);
                    closeItem.SubItems.Add(item.SubItems[2].Text);
                    closeItem.SubItems.Add(item.SubItems[4].Text);
                    closeBox.listViewUVHDFiles.Items.Add(closeItem);
                }

                DialogResult result = closeBox.ShowDialog();

                if (result == DialogResult.Cancel) { return; }

                if (result == DialogResult.OK)
                {
                    foreach (ListViewItem item in closeBox.listViewUVHDFiles.Items)
                    {
                        try
                        { 
                            var path = item.SubItems[2].Text;
                            if (File.Exists(path))
                            {
                                var filename = item.SubItems[0].Text;
                                // here we have to use the powershell command `Get-SmbOpenFile` to get the FileId of the selected item
                                ps.AddScript("Get-SmbOpenFile | Where-Object {$_.Path -like '*" + filename + "'} | Select-Object -Property FileId");
                                var output = ps.Invoke();
                                foreach (var psResult in output)
                                {
                                    var fileId = psResult.Properties["FileId"].Value.ToString();
                                    // powershell command to force close the SmbOpenFile
                                    ps.AddScript("Close-SmbOpenFile -FileId " + fileId + " -Force");
                                    ps.Invoke();
                                }
                            }
                        }
                        catch (Exception exeception)
                        {
                            textBoxStatus.Text = exeception.Message;
                        }
                    }

                    refreshListBox(textBoxFilePathUVHD.Text);
                }
            } else
            {
                MessageBox.Show("You need to start the program as administrator.", "Information");
            }
        }

        /// <summary>
        /// Writes the content of the listViewUVHDFiles to a csv file
        /// </summary>
        /// <returns></returns>
        private void buttonExport_Click(object sender, EventArgs e)
        {
            try { 
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
                saveFileDialog.FilterIndex = 0;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.CreatePrompt = true;
                saveFileDialog.Title = "Export data to csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter sw = new StreamWriter(saveFileDialog.FileName);
                    sw.WriteLine("uvhd file, last change, username, size, path");
                    foreach (ListViewItem item in this.listViewUVHDFiles.Items)
                    {
                        StringBuilder sb = new StringBuilder();
                        int count = 0;
                        foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                        {
                            sb.Append(subItem.Text);
                            if (count < item.SubItems.Count - 1)
                            {
                                sb.Append(",");
                            }
                            count += 1;

                        }
                        sw.WriteLine(sb.ToString());
                    }
                    sw.Close();
                    textBoxStatus.Text = "Successfully exported data to " + saveFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                textBoxStatus.Text = "Export failed: " + ex.Message;
            }
        }

        /// <summary>
        /// Searches for users in the originalListViewUVHDFiles and displays them in the listViewUVHDFiles.
        /// </summary>
        /// <returns></returns>
        private void SearchForUsers(string search)
        {
            listViewUVHDFiles.Items.Clear();
            foreach (var item in from ListViewItem item in this.originalListViewUVHDFiles.Items
                                 where item.SubItems[2].Text.ToLower().Contains(search.ToLower())
                                 select item)
            {
                listViewUVHDFiles.Items.Add(item.Clone() as ListViewItem);
            }
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            SearchForUsers(textBoxSearch.Text);
        }

        private void CopyListviewEntriesToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            foreach (ListViewItem item in listViewUVHDFiles.SelectedItems)
            {
                sb.Append(item.SubItems[0].Text);

                // Check if it's not the last selected item
                if (listViewUVHDFiles.SelectedItems.IndexOf(item) < listViewUVHDFiles.SelectedItems.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            Clipboard.SetText(sb.ToString());
            textBoxStatus.Text = "Copied to clipboard!";
        }

        private void listViewUVHDFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C && listViewUVHDFiles.SelectedItems.Count > 0)
            {
                CopyListviewEntriesToClipboard();
            }

        }

        private void CopyMenuStrip_Click(object sender, EventArgs e)
        {
            if (listViewUVHDFiles.SelectedItems.Count > 0)
            {
                CopyListviewEntriesToClipboard();
            }
        }

        private bool CompactVHDX(string vhdxPath, ref string errorMessage)
        {
            try
            {
                string diskpartScript = $"select vdisk file=\"{vhdxPath}\"\ncompact vdisk";
                string scriptFilePath = Path.GetTempFileName();

                File.WriteAllText(scriptFilePath, diskpartScript);

                Process process = new Process();
                process.StartInfo.FileName = "diskpart.exe";
                process.StartInfo.Arguments = $"/s \"{scriptFilePath}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();
                
                File.Delete(scriptFilePath);

                int exitCode = process.ExitCode;

                if (exitCode != 0)
                {
                    errorMessage = "Error executing Diskpart.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Exception: {ex.Message}";
                return false;
            }
        }

        private void buttonCompactUPD_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsElevated())
                {
                    StringBuilder results = new StringBuilder();
                    long totalReduction = 0;

                    if (!string.IsNullOrEmpty(textBoxFilePathUVHD.Text) && listViewUVHDFiles.SelectedItems.Count > 0)
                    {
                        results.AppendLine("Compaction Results:");
                        results.AppendLine("=====================");

                        ActionBox compactBox = new ActionBox("UVHD files to compact", "Compact", "CompactBox");
                        compactBox.listViewUVHDFiles.Items.Clear();

                        foreach (ListViewItem item in listViewUVHDFiles.SelectedItems)
                        {
                            ListViewItem compactItem = new ListViewItem(item.Text, item.ImageIndex);
                            compactItem.SubItems.Add(item.SubItems[2].Text);
                            compactItem.SubItems.Add(item.SubItems[4].Text);
                            compactBox.listViewUVHDFiles.Items.Add(compactItem);
                        }

                        DialogResult result = compactBox.ShowDialog();

                        if (result == DialogResult.Cancel) { return; }

                        if (result == DialogResult.OK)
                        {
                            int totalItems = compactBox.listViewUVHDFiles.Items.Count;
                            int processedItems = 0;

                            foreach (ListViewItem item in compactBox.listViewUVHDFiles.Items)
                            {
                                textBoxStatus.Text = $"Compacting {processedItems + 1} of {totalItems} files.";
                                textBoxStatus.Refresh();
                                string filePath = Path.Combine(textBoxFilePathUVHD.Text, item.SubItems[0].Text);
                                FileInfo fileInfo = new FileInfo(filePath);
                                if (!IsFileLocked(fileInfo))
                                {
                                    long sizeBeforeCompression = fileInfo.Length / 1024 / 1024;
                                    string errorMessage = string.Empty;
                                    bool isSuccess = CompactVHDX(filePath, ref errorMessage);

                                    if (!isSuccess)
                                    {
                                        results.AppendLine($"[ERROR] File: {filePath}");
                                        results.AppendLine($"Reason: {errorMessage}");
                                        results.AppendLine(new string('-', 50));
                                    }
                                    else
                                    {
                                        // Calculate the size reduction after compression
                                        FileInfo newFileInfo = new FileInfo(filePath);
                                        long fileSizeMB = newFileInfo.Length / 1024 / 1024;
                                        long reductionAmount = sizeBeforeCompression - fileSizeMB;

                                        results.AppendLine($"[SUCCESS] File: {filePath}");
                                        results.AppendLine($"Reduced by: {reductionAmount} MB");
                                        results.AppendLine(new string('-', 50));

                                        totalReduction += reductionAmount;
                                    }
                                }
                                else
                                {
                                    results.AppendLine($"[SKIPPED] File: {filePath}");
                                    results.AppendLine("Reason: File is locked.");
                                    results.AppendLine(new string('-', 50));
                                }

                                processedItems++;
                            }
                        }

                            results.AppendLine("=====================");
                            results.AppendLine($"Total Reduction: {totalReduction} MB");
                            results.AppendLine("=====================");

                        var messageBox = new ScrollMessageBox("Compaction Result", results.ToString());
                        messageBox.ShowDialog();
                        textBoxStatus.Text = $"Compaction Result: Total Reduction: {totalReduction} MB";
                    }
                    else
                    {
                        textBoxStatus.Text = "No files selected.";
                    }
                }
                else
                {
                    textBoxStatus.Text = "Please restart SidderPlus with elevated rights.";
                    buttonCompactUPD.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                textBoxStatus.Text = "Error: " + ex.Message;
            }
        }
    }
}
