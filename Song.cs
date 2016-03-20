using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kova
{
    public class Song : IEquatable<Song> 
    { 
        public string OriginalPath { get; protected set; }

        public string Album { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public TimeSpan Duration { get; set; }

        public string Genre { get; set; }

        public Song(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            this.OriginalPath = path;
            var audioFile = TagLib.File.Create(this.OriginalPath);

            this.Album = audioFile.Tag.Album;
            this.Artist = audioFile.Tag.FirstPerformer;
            this.Genre = audioFile.Tag.FirstGenre;
            if (audioFile.Tag.Title != null)
            {
                this.Title = audioFile.Tag.Title;
            }
            else
            {
                this.Title = System.IO.Path.GetFileName(path);
            }
          
        }

        public bool Equals(Song other)
        {
            return other != null && this.OriginalPath == other.OriginalPath;
        }
    }
}
