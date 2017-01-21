using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HDBackUp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(pathIn.Text)||File.Exists(pathIn.Text))
            {
                pathList.Items.Add(pathIn.Text);
            }
            else
            {
                pathIn.Text = "不存在的路径";
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            pathList.Items.Remove(pathList.SelectedItem);
        }

        private void backup_Click(object sender, RoutedEventArgs e)
        {
            //backupName.Content=pathList.SelectedItem.ToString();
            //backupFile();
            showState.Text = "正在备份";
            
            Thread cp = new Thread(new ThreadStart(backupFile));
            cp.Start();
        }

        int backupNums;
        int fileNum = 0;
        private void backupFile()
        {
            cped = 0;
            /*int dirNum = pathList.Items.Count;
            backupName.Content = dirNum.ToString();
            backupNums = 0;
            while (backupNums <dirNum )
            {
                CopyDirectory(pathList.Items[backupNums].ToString(), toDir.Text);
                backupNums++;
            }*/
            string dir = "";
            Dispatcher.Invoke(new Action(delegate
            {
                dir = toDir.Text;
            }));
            foreach (object each in pathList.Items)
            {
                fileNum += GetFilesCount(each.ToString());
            }
            Dispatcher.Invoke(new Action(delegate
            {
                cpProg.Maximum = fileNum;
            }));
            //数清所有文件个数
            foreach (object each in pathList.Items)
            {
                //Dispatcher.Invoke(new Action(delegate
                //{
                    CopyDirectory(each.ToString(), dir);
                //}));
            }
        }

        public int GetFilesCount(string path)
        {
            int count=0;
            //如果嵌套文件夹很多，可以开子线程去统计
            count += Directory.GetFiles(path).Length;
            foreach (var folder in Directory.GetDirectories(path))
            {
                count += GetFilesCount(folder);
            }
            return count;
        }

        int cped=0;
        private void CopyDirectory(string fromdir, string todir)
        {
            string folderName = fromdir.Substring(fromdir.LastIndexOf("\\") + 1);

            string desfolderdir = todir + "\\" + folderName;

            if (todir.LastIndexOf("\\") == (todir.Length - 1))
            {
                desfolderdir = todir + folderName;
            }
            string[] filenames = Directory.GetFileSystemEntries(fromdir);

            foreach (string file in filenames)// 遍历所有的文件和目录
            {
                if (Directory.Exists(file))// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                {

                    string currentdir = desfolderdir + "\\" + file.Substring(file.LastIndexOf("\\") + 1);
                    if (!Directory.Exists(currentdir))
                    {
                        Directory.CreateDirectory(currentdir);
                    }

                    Dispatcher.Invoke(new Action(delegate
                    {
                        backuppath.Content = pathList.Items[backupNums].ToString();
                        backupName.Content = file;
                    }));
                    CopyDirectory(file, desfolderdir);
                }

                else // 否则直接copy文件
                {
                    string srcfileName = file.Substring(file.LastIndexOf("\\") + 1);

                    srcfileName = desfolderdir + "\\" + srcfileName;


                    if (!Directory.Exists(desfolderdir))
                    {
                        Directory.CreateDirectory(desfolderdir);
                    }

                    Dispatcher.Invoke(new Action(delegate
                    {
                        backuppath.Content = pathList.Items[backupNums].ToString();
                        backupName.Content = file;
                    }));

                    cped++;
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (cpProg.Value == cpProg.Maximum)
                        {
                            showState.Text = "备份完成";
                        }
                        cpProg.Value = cped;
                        backuppath.Content = srcfileName;
                        backupName.Content = file;
                        backupNum.Content = (cped + "/" + fileNum);
                    }));
                    try
                    {
                        File.Copy(file, srcfileName);
                    }
                    catch (System.IO.IOException)
                    {
                        try//important
                        {
                            File.Delete(srcfileName);
                            File.Copy(file, srcfileName);
                        }
                        catch { }
                    }
                    
                }
            }//foreach 
        }//function end 
    }
}
