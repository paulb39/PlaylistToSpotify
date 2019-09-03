using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace PlaylistToSpotify
{
    public partial class Form1 : Form
    {
        private String _playlistLocation { get; set; }
        private SpotifyWebAPI _spotifyAPI { get; set; }
        private String _clientId { get; set; }
        private List<MusicItem> _failedTofindSongs = new List<MusicItem>();
        private List<MusicItem> _completelyFailedTofindSongs = new List<MusicItem>();

        public Form1()
        {
            InitializeComponent();

            _clientId = @"ADD CLIENT ID HERE";
        }

        private void btnFileBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                _playlistLocation = openFileDialog1.FileName;
                txtFile.Text = _playlistLocation;
            }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            // do stuff with spotify
            var auth = new ImplicitGrantAuth(
                _clientId,
                "http://localhost:4002",
                "http://localhost:4002",
                Scope.UserLibraryModify
            );

            auth.Start();
            auth.OpenBrowser();

            auth.AuthReceived += async (s, payload) =>
            {
                auth.Stop();
                _spotifyAPI = new SpotifyWebAPI()
                {
                    TokenType = payload.TokenType,
                    AccessToken = payload.AccessToken
                };

                String line;
                String artist = "testing";
                Boolean found = false;
                StreamReader file = new StreamReader(_playlistLocation);

                List<MusicItem> musicItems = new List<MusicItem>();

                while ((line = file.ReadLine()) != null)
                {
                    //System.Console.WriteLine(line);
                    //D:\Users\Paul\Music\Paul Music\Plus44\When Your Heart Stops Beating\12-plus_44-chapter_xiii.mp3
                    string[] artistPlusSong = null;
                    string[] slashPlusRest = null;

                    try
                    {
                        artistPlusSong = line.Split(new string[] { "Paul Music\\" }, StringSplitOptions.None);
                        slashPlusRest = artistPlusSong[1].Split('\\');
                        var musicItem = new MusicItem(line, slashPlusRest[0], Path.GetFileNameWithoutExtension(slashPlusRest[1]));
                        musicItems.Add(musicItem);
                        artist = slashPlusRest[0].ToLower();
                    }
                    catch (System.IndexOutOfRangeException ex)
                    {
                        System.Console.WriteLine(line);
                    }
                }

                //var playlist = await _spotifyAPI.CreatePlaylistAsync(_spotifyAPI.GetPrivateProfile().Id, artist);
                foreach (var music in musicItems) // find the song on Spotify and add to playlist
                {
                    String query = String.Format("artist:{0} track:{1}", music.artist, music.title);
                    Console.WriteLine("we are searching with " + query);
                    SearchItem searchItem = await _spotifyAPI.SearchItemsEscapedAsync(query, SearchType.Track);

                    if (searchItem != null && searchItem.Tracks.Items.Any())
                    {
                        foreach (var track in searchItem.Tracks.Items) // shit code, wish I could fiqure out how to convert this to linq
                        {
                            found = false;
                            foreach (var art in track.Artists)
                            {
                                if (art.Name.Contains(music.artist))
                                {
                                    if (track.Name.Contains(music.title))
                                    {
                                        found = true;
                                        //await _spotifyAPI.AddPlaylistTrackAsync(playlist.Id, track.Uri); // we got a match, add it to playlist
                                        Console.WriteLine("We found " + music.line);
                                        break;
                                    }
                                }
                            }
                        }
                        if (!found)
                        {
                            Console.WriteLine(query + " failed to find anything");
                            _failedTofindSongs.Add(music);
                        }
                    } else
                    {
                        Console.WriteLine(query + " failed to find anything");
                        _failedTofindSongs.Add(music);
                    }
                }

                // need to try searching with tags

                foreach (var failedMusic in _failedTofindSongs)
                {
                    var tFile = TagLib.File.Create(failedMusic.line);
                    //failedMusic.artist = tFile.Tag.FirstAlbumArtist ?? tFile.Tag.FirstPerformer ?? tFile.Tag.FirstArtist ?? failedMusic.artist;
                    //failedMusic.title = tFile.Tag.Title ?? failedMusic.title;

                    if (!String.IsNullOrWhiteSpace(tFile.Tag.FirstAlbumArtist))
                    {
                        failedMusic.artist = tFile.Tag.FirstAlbumArtist;
                    } else if (!String.IsNullOrWhiteSpace(tFile.Tag.FirstPerformer))
                    {
                        failedMusic.artist = tFile.Tag.FirstPerformer;
                    } else if (!String.IsNullOrWhiteSpace(tFile.Tag.FirstArtist))
                    {
                        failedMusic.artist = tFile.Tag.FirstArtist;
                    } else
                    {
                        Console.WriteLine(failedMusic.artist + " not in tags");
                    }

                    if (!String.IsNullOrWhiteSpace(tFile.Tag.Title))
                    {
                        failedMusic.title = tFile.Tag.Title;
                    } else
                    {
                        Console.WriteLine(failedMusic.title + " not in tags");
                    }
                }

                //todo do tag lookup

            };
        }

        private class MusicItem
        {
            public string artist { get; set; }
            public string line { get; set; }
            public string title { get; set; }

            public MusicItem(string line, string artist, string title)
            {
                this.line = line;
                this.artist = artist;
                this.title = title;
            }
        }
    }
}
