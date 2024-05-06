using Android.OS;
using Android.Provider;
using Mediah.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;

namespace Mediah.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Separate : ContentPage
    {
        static HttpClient client;
        public int page = 1;
        public SeperateFile seperateFile;
        const int ChunkSize = 4000000;

        static Separate()
        {
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://mediah.vercel.app");
        }

        public Separate(int id)
        {
            InitializeComponent();
            _ = InitializePage(id);
        }

        public async Task InitializePage(int id)
        {
            FileParts.IsRefreshing = true;
            var file = await InitializeAsync(id);
            seperateFile = file as SeperateFile;
            FileParts.IsRefreshing = false;
        }

        private async Task<SeperateFile> InitializeAsync(int id)
        {
            var file = await GetFile(id);
            BindingContext = file;
            return file;
        }

        public async Task<SeperateFile> GetFile(int id)
        {
            var response = await client.GetStringAsync($"/api/files/{id}");
            var file = JsonConvert.DeserializeObject<SeperateFile>(response);
            return file;
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private async void FileParts_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var file = FileParts.SelectedItem as SeperateFileFilePart;

            if (file != null)
            {
                var result = await DisplayAlert("Part Webhook", file.Url, "Copy URL", "Cancel");

                if (result)
                {
                    await Clipboard.SetTextAsync(file.Url.ToString());
                    await DisplayAlert("URL Copied", "URL has been copied to clipboard.", "OK");
                }
            }

            FileParts.SelectedItem = null;
        }

        public async Task<string> FetchFilePart(string url, int start, int end)
        {
            var response = await client.GetStringAsync($"/api/download?url={url}&start={start}&end={end}");
            var part = response.ToString();
            return part;
        }

        public string ConvertFileName(string filename)
        {
            return filename.ToLower().Replace(" ", "-");
        }

        private async void DownloadButton_Clicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert("Mediah - Downloader", $"Are you sure you want to download '{seperateFile.Filename}'?", "Download", "Cancel");

            if (result)
            {
                List<byte[]> totalParts = new List<byte[]>();

                FileParts.IsRefreshing = true;
                foreach (var part in seperateFile.Parts)
                {
                    var start = 0;
                    while (start < part.Size)
                    {
                        var end = Math.Min(start + ChunkSize, part.Size);
                        var body = await FetchFilePart(part.Url, start, end);
                        var decodedBody = Convert.FromBase64String(body);
                        totalParts.Add(decodedBody);
                        start = end;
                    }
                }

                byte[] combinedBytes = totalParts.SelectMany(part => part).ToArray();
                var fileName = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(type: Android.OS.Environment.DirectoryDownloads).AbsolutePath, ConvertFileName(seperateFile.Filename));

                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    fs.Write(combinedBytes, 0, combinedBytes.Length);
                }

                FileParts.IsRefreshing = false;
                var open = await DisplayAlert("Done", $"File downloaded successfully to {fileName}", "Open File", "Close");

                if (open)
                {
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(fileName)
                    });
                }
            }
        }
    }
}