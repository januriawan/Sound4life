using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace SoundCloudAPI
{
    public sealed partial class MainPage : Page
    {
        private string soundcloudAPI = "df2df7e28eaeda8e747f1d79a59beaaa";
        public string responseText;
        private int SCUserID = 0;
        private int SCPlayListID = 0;
        private string SCLink = "http://api.soundcloud.com/";
        public string SCAPIUsers = "users/";
        public string SCAPITracks = "tracks/";
        public string SCAPIPlaylist = "playlists/";
        public int cuuredTrackId = 0;
        public int MainPlaylistID = 0;
        List<SCPlaylistObject> playlistObject;

        public MainPage() 
        {
            this.InitializeComponent();
            txtStatus.Text = "Application is ready for your command.";
        }

        private void Button_Click(object sender, RoutedEventArgs e) 
        {
            if (SCNameField.Text != "")
            {
                PBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                getUserID();
                txtStatus.Text = "Fetching user details!";
            }
            else
            {
                MessageDialog showMessgae = new MessageDialog("Please provide a username");
                showMessgae.ShowAsync();
                SCNameField.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            }
        }

          public async void getUserID() 
        {
            try { 
            responseText = await GetjsonStream();

            SCUserObject userObject = JsonConvert.DeserializeObject<SCUserObject>(responseText);

            SCUserID = userObject.id;
            txtStatus.Text = "Users details fetched successfully. Please wait!";

            
            getPlaylistID();

            }
            catch (Exception ex)
            {
                MessageDialog showMessgae = new MessageDialog("There was problem connecting. Please check your parameter values");
                showMessgae.ShowAsync();
                PBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                txtStatus.Text = "There was problem connecting. Please try again!";
            }
           
            

        }

       
        public async Task<string> GetjsonStream()
        {
            HttpClient client = new HttpClient();
            string url = SCLink + SCAPIUsers + SCNameField.Text + ".json?client_id=" + soundcloudAPI;
            HttpResponseMessage response = await client.GetAsync(url);
            HttpResponseMessage v = new HttpResponseMessage();
            return await response.Content.ReadAsStringAsync();
        }



  

        public async void getPlaylistID() 
        {
         try {
            responseText = await GetUserPlayList();
         

            playlistObject = JsonConvert.DeserializeObject<List<SCPlaylistObject>>(responseText);


            AllPlayListTracks.ItemsSource = playlistObject;

            testList.DataContext = playlistObject[MainPlaylistID].tracks;
            AllPlayListTracks.SelectedIndex = 0;
            testList.SelectedIndex = 0;
            plybutton.IsEnabled = true;
            stopButton.IsEnabled = true;
             PBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            txtStatus.Text = "Playlists have been loaded. Playlist 0 has been set has playlist for now.";
           }
          catch (Exception ex)
          {
              MessageDialog showMessgae = new MessageDialog("There was problem connecting. Please check your parameter values");
           showMessgae.ShowAsync();
           PBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
           txtStatus.Text = "There was problem connecting. Please try again!";
         }



        }

        public void UpdatePlayListData() 
        {
            testList.DataContext = playlistObject[MainPlaylistID].tracks;
            testList.SelectedIndex = 0;
            PBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

        }


        public async Task<string> GetUserPlayList() 
        {
            HttpClient client = new HttpClient();
            string url = SCLink + SCAPIUsers + SCUserID + "/playlists.json?client_id=" + soundcloudAPI;
            HttpResponseMessage response = await client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

      

        private void Button_Click_2(object sender, RoutedEventArgs e) 
        {

            playMusic();

        }

       

        public void playMusic(){ 

            cuuredTrackId = cuuredTrackId + 1;

             if (cuuredTrackId >= playlistObject[MainPlaylistID].tracks.Count)
            {
                cuuredTrackId = 0;

                if (playlistObject[MainPlaylistID].tracks[cuuredTrackId].streamable=="True"){
                myPlayer.Source = new Uri(playlistObject[MainPlaylistID].tracks[cuuredTrackId].stream_url + "?client_id=" + soundcloudAPI);
                txtStatus.Text = "Currently Playing: "+ playlistObject[MainPlaylistID].tracks[cuuredTrackId].title ;
            }else{
                txtStatus.Text = playlistObject[MainPlaylistID].tracks[cuuredTrackId].title + " track can not be streamed. Next Track is playing";
                playMusic();
                }
            }
            else
            {
                

                if (playlistObject[MainPlaylistID].tracks[cuuredTrackId].streamable == "True")
                {
                    myPlayer.Source = new Uri(playlistObject[MainPlaylistID].tracks[cuuredTrackId].stream_url + "?client_id=" + soundcloudAPI);
                    txtStatus.Text = "Currently Playing: " + playlistObject[MainPlaylistID].tracks[cuuredTrackId].title;
                }
                else
                {
                    txtStatus.Text = playlistObject[MainPlaylistID].tracks[cuuredTrackId].title+ " track can not be streamed. Next Track is playing";
                    playMusic();
                }
            }

             testList.SelectedIndex = cuuredTrackId;
             

        }

        public void playMusic(Track demoTrack) //Playmusic on the bases of selection of 
        {

            

            if (demoTrack.streamable=="True")
            {

                myPlayer.Source = new Uri(demoTrack.stream_url + "?client_id=" + soundcloudAPI);
                txtStatus.Text = "Currently Playing: " + demoTrack.title;
              

            }
            else
            {
                playMusic();
            }
          


        }

        private void Button_Click_4(object sender, RoutedEventArgs e) 
        {
            myPlayer.Stop();
            txtStatus.Text = "Music Player Stopped";
        }

        private void musicchanged(object sender, SelectionChangedEventArgs e) 
        {
            var item = testList.SelectedItem as Track;
            
            playMusic(item);
            cuuredTrackId = testList.SelectedIndex;
            
        }

        private void mainPlayList(object sender, SelectionChangedEventArgs e) 
        {
           MainPlaylistID = AllPlayListTracks.SelectedIndex;
           UpdatePlayListData();
        }

        private void mediaEndedEvent(object sender, RoutedEventArgs e) 
        {
        
            playMusic();
        
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
