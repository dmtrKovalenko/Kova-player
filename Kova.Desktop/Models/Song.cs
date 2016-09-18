using System;
using System.IO;

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

            var audioFile = TagLib.File.Create(this.OriginalPath);

            this.OriginalPath = path;
            this.Album = audioFile.Tag.Album;
            this.Genre = audioFile.Tag.FirstGenre;
            this.Artist = audioFile.Tag.FirstPerformer ?? "Unknown Artist";
            this.Title = audioFile.Tag.Title ?? Path.GetFileNameWithoutExtension(path);
        }

        public bool Equals(Song other)
        {
            return other != null && this.OriginalPath == other.OriginalPath;
        }

        public override string ToString()
        {
            return $"{this.Artist} - {this.Title}";
        }
    }
}
