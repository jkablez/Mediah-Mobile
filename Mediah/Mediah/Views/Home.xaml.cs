using Mediah.Models;
using Mediah.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Mediah
{
    public partial class MainPage : ContentPage
    {
        static HttpClient client;
        public int page = 1;

        static MainPage()
        {
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://mediah.vercel.app");
        }

        public MainPage()
        {
            InitializeComponent();
            InitializeAsync();

        }

        private async void InitializeAsync()
        {
            FileList.IsRefreshing = true;
            await RefreshFiles();
            FileList.IsRefreshing = false;
        }

        public async Task RefreshFiles()
        {
            try
            {
                var files = await FetchFiles(page);
                FileList.ItemsSource = null;
                if (files.Count == 0)
                {
                    page -= 1;
                    InitializeAsync();
                }
                else
                {
                    FileList.ItemsSource = files;
                    FileList.IsRefreshing = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        public static async Task<List<File>> FetchFiles(int page)
        {
            try
            {
                var response = await client.GetStringAsync($"/api/files?page={page}");
                var files = JsonConvert.DeserializeObject<List<File>>(response);
                return files;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void RefreshList_Clicked(object sender, EventArgs e)
        {
            InitializeAsync();
        }

        private void FileList_Refreshing(object sender, EventArgs e)
        {
            InitializeAsync();
        }

        private void FileList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var file = FileList.SelectedItem as File;
            Navigation.PushModalAsync(new Separate(file.Id));
        }

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new SearchPage());
        }

        private void PreviousPage_Clicked(object sender, EventArgs e)
        {
            if (page > 1)
            {
                page -= 1;
                InitializeAsync();
            }
        }

        private void NextPage_Clicked(object sender, EventArgs e)
        {
            page += 1;
            InitializeAsync();
        }
    }
}