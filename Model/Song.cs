using System;

namespace Kova
{
    public class Song : IEquatable<Song>
    { 
        public string OriginalPath { get; private set; }

        public string Album { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Genre { get; set; }
       
        public Song(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            this.OriginalPath = path;
            var audioFile = TagLib.File.Create(this.OriginalPath);

            this.Album = audioFile.Tag.Album;
            this.Genre = audioFile.Tag.FirstGenre;
            if (audioFile.Tag.FirstPerformer != null)
            {
                this.Artist = audioFile.Tag.FirstPerformer;
            }
            else
            {
                this.Artist = "Unknown Artist";
            }
 
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

        public override string ToString()
        {
            return String.Format("{0} - {1}", this.Artist, this.Title);
        }
    }
}
