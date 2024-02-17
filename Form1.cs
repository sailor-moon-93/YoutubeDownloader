using AngleSharp.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using static System.Net.WebRequestMethods;

namespace YoutubeDownloader
{
    public partial class Form1 : Form
    {
        string link;

        public Form1()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            link = textBox1.Text;

            if (!String.IsNullOrEmpty(link))
            {
                ShowInfo(link);
                label3.Visible = true;
                button2.Visible = true;
                button3.Visible = true;
            }

            async void ShowInfo(string str)
            {
                var youtube = new YoutubeClient();
                var videoUrl = str;
                var video = await youtube.Videos.GetAsync(videoUrl);

                var title = video.Title; // название видео
                var author = video.Author.ChannelTitle; // название канала
                var duration = video.Duration; // продолжительность
                var ID = video.Id;

                listBox1.Items.Add($"Название видео: {title}");
                listBox1.Items.Add($"Автор: {author}");
                listBox1.Items.Add($"Продолжительность: {duration}");
                listBox1.Items.Add($"ID: {ID}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label4.Visible = true;
            textBox2.Visible = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var savePath = textBox2.Text;

            if (Directory.Exists(savePath))
            {
                DownloadVideo(link, savePath);
            }
            else
            {
                MessageBox.Show("Вы ничего не ввели или такое расположение не существует!");
            }

            async void DownloadVideo(string link, string path)
            {
                var youtube = new YoutubeClient();

                var video = await youtube.Videos.GetAsync(link);
                var forbiddenSymbols = new string[] {"\\", "/", ":", "*", "?", "\"", "<", ">"};
                string name = video.Title;
                foreach (var s in forbiddenSymbols)
                {
                    name = name.Replace(s, "");
                }

                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(link); //получаем список доступных видео
                var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality(); //берем видео наилучшего качества              
                await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{path}\\{name}.{streamInfo.Container}"); //сохраняем по пути + имя файла из описания
            }
        }
    }
}
