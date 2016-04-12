using System;
using System.Collections.ObjectModel;

namespace Kova
{
    public sealed class Playlist
    {
        private ObservableCollection<Song> _songs;

        public Playlist()
        {

        }
 
        public ObservableCollection<Song> PlayList
        {
            get
            {
                if (_songs == null)
                    throw new NullReferenceException();
                return _songs;
            }
            set
            {
                _songs = value;
            }
        }

        public Song Next(Song current)
        {
            if (_songs.Contains(current) && _songs.IndexOf(current) != _songs.Count - 1)
            {
                return _songs[_songs.IndexOf(current) + 1];
            }
            else
            {
                return null;
            }
        }

        public Song Previous(Song current)
        {
            if (_songs.Contains(current) && _songs.IndexOf(current) != 0)
            {
                return _songs[_songs.IndexOf(current) - 1];
            }
            else
            {
                return null;
            }
        }

        public Song Repeat(Song current)
        {
            return current;
        }
    }
}
