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

        public Form1()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            link = textBox1.Text;
            listBox1.Items.Clear();
            textBox2.Clear();
            progressBar1.Value = 0;
            label3.Text = string.Empty;

            if (!String.IsNullOrEmpty(link))
            {
                ShowInfo(link);
            }
            else
            {
                MessageBox.Show("Вы ничего не ввели!");
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

                    button2.Visible = true;
                    button3.Visible = true;
                    button4.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    textBox2.Visible = true;
                    progressBar1.Visible = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Это не ссылка на Youtube видео!");
                    button2.Visible = false;
                    button3.Visible = false;
                    button4.Visible = false;
                    label3.Visible = false;
                    label4.Visible = false;
                    textBox2.Visible = false;
                    progressBar1.Visible = false;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var savePath = textBox2.Text;


            if (Directory.Exists(savePath))
            {
                DownloadVideo(link, savePath);
                MessageBox.Show("Начал загружать видео!");
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


                    var progress = new Progress<double>(p =>
                    {
                        if (progressBar1.Value != p * 100)
                        {
                            progressBar1.Value = Convert.ToInt32(p * 100);
                            label3.Text = Convert.ToInt32(p * 100) + "%";
                        }
                        else
                        {
                            MessageBox.Show("Готово!");
                        }
                    }); //заполняем прогрессбар


                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(link); //получаем список доступных видео
                    var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality(); //берем видео наилучшего качества            
                    fullSavePath = $"{path}\\{name}.{streamInfo.Container}"; //сохраняем полное имя файла в отдельную переменную
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, fullSavePath, progress); //сохраняем по пути + имя файла из описания

                }
                catch (Exception)
                {
                    MessageBox.Show("Изначальная ссылка на видео не верна!");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowDialog();
            textBox2.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Загрузка будет отменена и приложение будет закрыто. Вы уверены?", "Отмена", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
                Process.GetCurrentProcess().Kill();
        }
    }
}
