using AngleSharp.Text;
using MediaFileProcessor.Models.Video;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace YoutubeDownloader
{

    public partial class Form1 : Form
    {
        string link;
        string fullSavePath;
        bool error;

        public Form1()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            link = textBox1.Text;
            listBox1.Items.Clear();
            

            if (!String.IsNullOrEmpty(link))
            {
                error = false;
                ShowInfo(link);
            }
            else 
            {
                MessageBox.Show("Вы ничего не ввели");
            }



            async void ShowInfo(string str)
            {
                try
                {
                    var youtube = new YoutubeClient();
                    var videoUrl = str;
                    var video = await youtube.Videos.GetAsync(videoUrl);

                    var title = video.Title; // название видео
                    var author = video.Author.ChannelTitle; // название канала
                    var duration = video.Duration; // продолжительность

                    listBox1.Items.Add($"Название видео: {title}");
                    listBox1.Items.Add($"Автор: {author}");
                    listBox1.Items.Add($"Продолжительность: {duration}");

                    label4.Visible = true;
                    textBox2.Visible = true;
                    button4.Visible = true;
                }
                catch (Exception)
                {
                    error = true;
                    MessageBox.Show("Это не ссылка на Youtube или в ней содержатся русские буквы");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var savePath = textBox2.Text;

            if (Directory.Exists(savePath) && error == false)
            {
                DownloadVideo(link, savePath);
                MessageBox.Show("Начал загружать видео");
            }
            else
            {
                MessageBox.Show("Вы ничего не ввели или такое расположение не существует!");
            }


            async void DownloadVideo(string link, string path)
            {
                try
                {
                    var youtube = new YoutubeClient();

                    var video = await youtube.Videos.GetAsync(link);
                    var forbiddenSymbols = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
                    string name = video.Title;
                    foreach (var s in forbiddenSymbols)
                    {
                        name = name.Replace(s, "");
                    }
                    //var progressHandler = new Progress<double>();
                    var progress = new Progress<double>(p =>
                    {
                        progressBar1.Value = Convert.ToInt32(p * 100);
                        progressBar1.Text = Convert.ToString(p * 100) + "%";
                    });

                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(link); //получаем список доступных видео
                    var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality(); //берем видео наилучшего качества            
                    fullSavePath = $"{path}\\{name}.{streamInfo.Container}"; //прокидываем в глобальную переменную полное ия файла
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, fullSavePath, progress); //сохраняем по пути + имя файла из описания

                    
                }
                catch (Exception)
                {
                    MessageBox.Show("Изначальная ссылка на видео не верна");
                    error = true;
                }
            }
        }
        async void CheckDone()
        {
            string dir = Path.GetDirectoryName(fullSavePath);
            string file = Path.GetFileName(fullSavePath);

            Type shellAppType = Type.GetTypeFromProgID("Shell.Application");
            dynamic shell = Activator.CreateInstance(shellAppType);
            dynamic folder = shell.NameSpace(dir);
            dynamic folderItem = folder.ParseName(file);
            string value = folder.GetDetailsOf(folderItem, 27).ToString();

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(2000);
                if (int.Parse(value) > 0)
                    MessageBox.Show("Скачивание завершено");
                else
                    MessageBox.Show("Скачивание завершено с ошибкой!");
            }
        }
    }
}
