using System;
using System.IO;
using Wisej.Web;
using System.Linq;

namespace FileManager4
{
    public partial class FrmMain : Form
    {
        public string FileName { get; set; }
        public int Reset { get; private set; }

        public FrmMain()
        {
            InitializeComponent();
            PopulateTreeView();
        }

        string MyPath = @"D:/Documents/Reonance Tunneling";

        DirectoryInfo directoryInfo = new DirectoryInfo(path: @"D:/Documents/Reonance Tunneling");

        //  Start of treeview1 code
        private void PopulateTreeView()
        {
            TreeNode rootNode;

            if (directoryInfo.Exists)
            {
                rootNode = new TreeNode(directoryInfo.Name)
                {
                    Tag = directoryInfo
                };

                GetDirectories(directoryInfo.GetDirectories(), rootNode);
                TreeView1.Nodes.Add(rootNode);
                TreeView1.ExpandAll();
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs,
            TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);

                //  The follow code is used to avoid 'Object initialization can be simplified' warning!
                aNode = new TreeNode(subDir.Name, 0, 0)
                {
                    Tag = subDir,
                    ImageKey = "folder"
                };

                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

         void TreeView1_NodeMouseClick(object sender,
            TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;           
            PopulateListView(newSelected);     
                       
            ListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);                  
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode newSelected = e.Node;
            if (newSelected.Tag.ToString() == MyPath)
            {
                BntDeleteFolder.Enabled = false;
            }
            else
            {
                BntDeleteFolder.Enabled = true;
            }
            BntCreateSubFolder.Enabled = true;
            Reset = 0;
        }
        //  End of TreeView code

        //  Begin ListView code
        private int MyImageIndex(string Ext)
        {
            switch (Ext)
            {
                case ".htm": return 1;
                case ".docx": return 2;
                case ".pdf": return 3;
                case ".ppt": return 4;
                case ".rtf": return 5;
                case ".txt": return 6;
                case ".xlsx": return 7;
                default: return 8;
            }
        }

        private void PopulateListView(TreeNode newSelected)
        {
            ListView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem item = null;

            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                double size;
                size = file.Length / 1024;
                if (size < 1)
                {
                    size = 0;
                }

                item = new ListViewItem(file.Name, 1);
                string Ext = Path.GetExtension(file.Name);
                ListView1.Items.Add(item);
                item.ImageIndex = MyImageIndex(Ext);
                item.SubItems.Add(size.ToString());
                item.SubItems.Add(file.Extension);
                item.SubItems.Add(file.LastAccessTime.ToShortDateString());
            }
            Reset = 0;
        }

        private void ListView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            //  Code is used here in case the form doesn't load properly.
            Reset = ListView1.SelectedItems.Count;
            //  To prevent index = '0'
            if (Reset == 1 || Reset > 1)
            {
                FileName = ListView1.SelectedItems[0].Text;

                if (Reset == 1)
                {
                    BntViewFileDetails.Enabled = true;
                    BntDeleteFiles.Enabled = true;
                    BntRename.Enabled = true;
                    if (Path.GetExtension(FileName) == ".htm" || Path.GetExtension(FileName) == ".txt")
                    {
                        BntEdit.Enabled = true;
                    }
                    else
                    {
                        BntEdit.Enabled = false;
                    }
                }
                else
                {
                    BntViewFileDetails.Enabled = false;
                    BntRename.Enabled = false;
                    BntDeleteFiles.Enabled = true;
                    BntEdit.Enabled = false;
                }
            }
            else
            {
                BntViewFileDetails.Enabled = false;
                BntRename.Enabled = false;
                BntDeleteFiles.Enabled = false;
                BntEdit.Enabled = false;
            }
            Reset = 0;
        }
        //  End of ListView code!

        //  Begin of Button code
        private void BntRefreshFolders_Click(object sender, EventArgs e)
        {
            TreeView1.Nodes.Clear();
            PopulateTreeView();
            Reset = 0;
        }

        private void BntCreateSubFolder_Click(object sender, EventArgs e)
        {
            TreeNode newSelected = TreeView1.SelectedNode;
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            string LabelTxt = "Enter new sub-folder name.";
            string NewFolderName;
            string OldFileName = "";

            FrmFileFolderDialog subfolder;
            subfolder = new FrmFileFolderDialog(OldFileName, LabelTxt)
            {
                Text = "Create a new sub-folder"
            };
            subfolder.ShowDialog();
            NewFolderName = subfolder.NFileOrFolderName;
            string NewFolderFullName = Path.Combine(nodeDirInfo.FullName, NewFolderName);
            if (subfolder.BntOKClicked)
            {
                if (!Directory.Exists(NewFolderFullName))
                {
                    DirectoryInfo subFolderInfo = nodeDirInfo.CreateSubdirectory(NewFolderName);
                }
                else
                {
                    MessageBox.Show("Directory '" + NewFolderName + "' already exist!",
                            "Duplicate Directory Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BntDeleteFolder_Click(object sender, EventArgs e)
        {
            TreeNode newSelected = TreeView1.SelectedNode;
            string DirectoryName = newSelected.Text;
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;

            try
            {
                if (MessageBox.Show("Are you sure you want to delete Folder " + "'" + DirectoryName + "'" + " ?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (FileInfo file in nodeDirInfo.GetFiles())
                    {
                        file.Delete();
                    }

                    nodeDirInfo.Delete(true);
                    newSelected.Remove();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            ListView1.Items.Clear();

        }

        private void BntAdminstration_Click(object sender, EventArgs e)
        {

        }

        private void BntLogoff_Click(object sender, EventArgs e)
        {
            this.Hide();
            FrmLogon fl = new FrmLogon();
            fl.Show();
        }

        private void BntViewFileDetails_Click(object sender, EventArgs e)
        {

        }

        private void BntDeleteFiles_Click(object sender, EventArgs e)
        {
            TreeNode newSelected = TreeView1.SelectedNode;
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            string toDisplay;
            string toDeleteName = ListView1.SelectedItems[0].Text;
            var toDelete = ListView1.SelectedItems.ToList();

            try
            {
                if (ListView1.SelectedItems.Count > 1)
                {
                    toDisplay = ListView1.SelectedItems.Count + " Files";
                }
                else
                {
                    toDisplay = toDeleteName;
                }

                if (MessageBox.Show("Are you sure you want to delete " + toDisplay + " ?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (var item in toDelete)
                    {

                        foreach (FileInfo file in nodeDirInfo.GetFiles())
                        {
                            if (file.Name == item.Text)
                            {
                                file.Delete();
                                ListView1.Items.Remove(item);
                            }
                        }
                    }
                }
                ListView1.Items.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            PopulateListView(newSelected);
        }

        private void BntRename_Click(object sender, EventArgs e)
        {
            try
            {
                TreeNode newSelected = TreeView1.SelectedNode;
                DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
                string OldFileName = ListView1.SelectedItems[0].Text;
                string NewFileName;
                string LabelTxt = "Enter the new file name";
                string Ext = Path.GetExtension(OldFileName);
                string OldFullPath = Path.Combine(nodeDirInfo.FullName, OldFileName);

                FrmFileFolderDialog rename;
                rename = new FrmFileFolderDialog(OldFileName, LabelTxt)
                {
                    Text = "Rename File"
                };
                rename.ShowDialog();

                if (rename.BntOKClicked)
                {
                    NewFileName = rename.NFileOrFolderName;
                    string NewFullPath = Path.Combine(nodeDirInfo.FullName, NewFileName + Ext);
                    if (OldFullPath == NewFullPath)
                    {
                        MessageBox.Show("File " + NewFileName + " already exists in this folder!",
                            "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        rename.Close();
                    }
                    else
                    {
                        File.Move(OldFullPath, NewFullPath);
                    }
                    PopulateListView(newSelected);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Reset = 0;
        }
        //  Add New File Code from Split-Button Menu!
        private void HtmlFile_Click(object sender, EventArgs e)
        {
            string Ext = ".htm";
            OpenNewFile(Ext);
        }

        private void TxtFile_Click(object sender, EventArgs e)
        {
            string Ext = ".txt";
            OpenNewFile(Ext);
        }

        private void OpenNewFile (string Ext)
        {
            TreeNode newSelected = TreeView1.SelectedNode;            
            DirectoryInfo DirectoryName = (DirectoryInfo)newSelected.Tag;
            bool BntOk = false;
            string Label = "Enter the new file name";
            string NewFileName = "";            
            string OldFileName = "";

            if (Ext == ".htm")
            {
                FrmFileFolderDialog newHtmlFile;
                newHtmlFile = new FrmFileFolderDialog(OldFileName, Label)
                {
                    Text = "New Html File"
                };
                newHtmlFile.ShowDialog();
                BntOk = newHtmlFile.BntOKClicked;
                NewFileName = newHtmlFile.NFileOrFolderName;
                OpenEdit(BntOk, DirectoryName, NewFileName, Ext);
                PopulateListView(newSelected);
            }
            else
            {
                FrmFileFolderDialog newTextFile;
                newTextFile = new FrmFileFolderDialog(OldFileName, Label)
                {
                    Text = "New Text File"
                };
                newTextFile.ShowDialog();
                BntOk = newTextFile.BntOKClicked;
                NewFileName = newTextFile.NFileOrFolderName;
                OpenEdit(BntOk, DirectoryName, NewFileName, Ext);
                PopulateListView(newSelected);
            }
            Reset = 0;
        }

        private void OpenEdit(bool BntOK, DirectoryInfo DirectoryName , string NewFileName, string Ext)
        {
            string NewFileFullName = DirectoryName.FullName + "//" + NewFileName + Ext;
            if (File.Exists(NewFileFullName))
            {
                MessageBox.Show("File " + NewFileName + " already exists in this folder!",
                           "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (BntOK == true)
                {
                    using (StreamWriter writer =
                    new StreamWriter(NewFileFullName))
                    {
                        writer.Write("");
                    }
                    FrmEdit edit;
                    edit = new FrmEdit(DirectoryName, NewFileName + Ext, Ext)
                    {
                        Text = "Edit " + NewFileName
                    };
                    edit.ShowDialog();                    
                }
            }
        }
        //  End of Add New File code!

        private void BntEdit_Click(object sender, EventArgs e)
        {
            TreeNode newSelected = TreeView1.SelectedNode;
            DirectoryInfo directoryName = (DirectoryInfo)newSelected.Tag;
            string OldFileName = ListView1.SelectedItems[0].Text;
            string Ext = Path.GetExtension(OldFileName);            
           
            FrmEdit edit;
            edit = new FrmEdit(directoryName, OldFileName, Ext)
            {
                Text = "Edit " + OldFileName
            };
            edit.ShowDialog();
            PopulateListView(newSelected);
            Reset = 0;
        }
        //  End of Button code!

        public void SaveAndClose(string FileFullName, string TxtBoxInfro)
        {
            StreamWriter txtoutput = new StreamWriter(FileFullName);
            txtoutput.Write(TxtBoxInfro);
            txtoutput.Close();
        }

        public void SaveAs(string NewFullName, string BoxInfro)
        {
            StreamWriter txtoutput = new StreamWriter(NewFullName);
            txtoutput.Write(BoxInfro);
            txtoutput.Close();
            Reset = 0;
        }       
       
    }
}
