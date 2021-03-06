﻿using DoubleMetaphone;
using SongRequest.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SongRequest.SongPlayer
{
    [Serializable]
    public class Song
    {
        public Song()
        {
            LastPlayTime = string.Empty;
        }

        private string _id = null;

        public string TempId
        {
            get
            {
                if (_id == null)
                {
                    using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                    {
                        _id = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(FileName))).Replace("-", "");
                    }
                }

                return _id;
            }
        }

        private static HashSet<string> _tagLibExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3",
            ".m2a",
            ".mp2",
            ".mp1"
        };

        private static bool CanReadTagsForSong(Song song)
        {
            if (_tagLibExtensions.Contains(song.Extension))
                return true;

            return false;
        }

        public bool CanReadTags
        {
            get
            {
                return CanReadTagsForSong(this);
            }
        }

        public bool IsInfinite
        {
            get
            {
                if (string.Equals(".m3u", Extension, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(".m3u8", Extension, StringComparison.OrdinalIgnoreCase))
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Artist of song
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Artist of song
        /// </summary>
        public string ArtistSearchValue { get; set; }

        /// <summary>
        /// Artist of song
        /// </summary>
        public string ArtistDoubleMetaphone { get; set; }

        /// <summary>
        /// Name/title of song
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name/title of song
        /// </summary>
        public string NameSearchValue { get; set; }

        /// <summary>
        /// Name/title of song
        /// </summary>
        public string NameDoubleMetaphone { get; set; }

        /// <summary>
        /// Album of song
        /// </summary>
        public string Album { get; set; }

        /// <summary>
        /// Album of song
        /// </summary>
        public string AlbumSearchValue { get; set; }

        /// <summary>
        /// Album of song
        /// </summary>
        public string AlbumDoubleMetaphone { get; set; }

        /// <summary>
        /// Rating of song
        /// </summary>
        public int Rating { get; set; }

        public void GenerateSearchAndDoubleMetaphone()
        {
            if (!string.IsNullOrEmpty(FileName))
                FileNameSearchValue = FileName.ToLower().ReplaceUniqueCharacters();
            else
                FileNameSearchValue = null;

            if (!string.IsNullOrEmpty(Artist))
            {
                ArtistSearchValue = Artist.ToLower().ReplaceUniqueCharacters();
                ArtistDoubleMetaphone = ArtistSearchValue.GenerateDoubleMetaphone();
            }
            else
            {
                ArtistSearchValue = null;
                ArtistDoubleMetaphone = null;
            }

            if (!string.IsNullOrEmpty(Name))
            {
                NameSearchValue = Name.ToLower().ReplaceUniqueCharacters();
                NameDoubleMetaphone = NameSearchValue.GenerateDoubleMetaphone();
            }
            else
            {
                NameSearchValue = FileNameSearchValue;

                if (!string.IsNullOrEmpty(FileNameSearchValue))
                    NameDoubleMetaphone = FileNameSearchValue.GenerateDoubleMetaphone();
                else
                    NameDoubleMetaphone = null;
            }

            if (!string.IsNullOrEmpty(Album))
            {
                AlbumSearchValue = Album.ToLower().ReplaceUniqueCharacters();
                AlbumDoubleMetaphone = AlbumSearchValue.GenerateDoubleMetaphone();
            }
            else
            {
                AlbumSearchValue = null;
                AlbumDoubleMetaphone = null;
            }
        }

        /// <summary>
        /// Extension
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File name
        /// </summary>
        public string FileNameSearchValue { get; set; }

        /// <summary>
        /// Duration in seconds
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// If true, error while reading tag
        /// </summary>
        public bool ErrorReadingTag { get; set; }

        /// <summary>
        /// If true, tag is read
        /// </summary>
        public bool TagRead { get; set; }

        /// <summary>
        /// File creation date
        /// </summary>
        public string DateCreated { get; set; }

        /// <summary>
        /// Genre of song
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// Year of song
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Last time song is played
        /// </summary>
        public string LastPlayTime { get; private set; }

        /// <summary>
        /// If true, tag is read
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Name of last requester
        /// </summary>
        public string LastRequester { get; set; }

        /// <summary>
        /// Name of last requester
        /// </summary>
        private string _skippedBy;

        /// <summary>
        /// Name of last requester
        /// </summary>
        public string SkippedBy
        {
            get { return _skippedBy; }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Equals("randomizer", StringComparison.OrdinalIgnoreCase))
                    _skippedBy = string.Empty;
                else
                    _skippedBy = value;
            }
        }

        /// <summary>
        /// Last play time of song
        /// </summary>
        public DateTime? LastPlayDateTime
        {
            get
            {
                return _lastPlayDateTime;
            }
            set
            {
                _lastPlayDateTime = value;
                if (_lastPlayDateTime != null)
                {
                    IsDirty = true;
                    LastPlayTime = _lastPlayDateTime.Value.ToString("yyyy-MM-dd HH:mm");
                }
                else if (!string.IsNullOrEmpty(LastPlayTime))
                {
                    IsDirty = true;
                    LastPlayTime = string.Empty;
                }
            }
        }
        private DateTime? _lastPlayDateTime;

        /// <summary>
        /// Get artist & title combined
        /// </summary>
        public string GetArtistAndTitle()
        {
            if (!string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Name))
                return Artist + " - " + Name;

            if (!string.IsNullOrEmpty(Name))
                return Name;

            return FileName;
        }

        public MemoryStream GetImageStream(int size)
        {
            FileInfo fileInfo = new FileInfo(FileName);

            List<FileInfo> coverFiles = new List<FileInfo>();
            if (fileInfo.Exists && fileInfo.Directory.Exists)
            {
                coverFiles.AddRange(fileInfo.Directory.GetFiles("*Cover.*", SearchOption.TopDirectoryOnly));
                coverFiles.AddRange(fileInfo.Directory.GetFiles("*Artwork.*", SearchOption.TopDirectoryOnly));
                coverFiles.AddRange(fileInfo.Directory.GetFiles("*Front.*", SearchOption.TopDirectoryOnly));
                string fileNameWithoutExtension = fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
                coverFiles.AddRange(fileInfo.Directory.GetFiles(fileNameWithoutExtension + ".*", SearchOption.TopDirectoryOnly));
            }

            HashSet<string> possibleExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".bmp",
                ".gif",
                ".jpg",
                ".jpeg",
                ".png",
                ".tiff"
            };

            // for saving data
            byte[] thumbnailData = null;

            // get possible cover files
            List<FileInfo> possibleCoverFiles = coverFiles.Where(x =>
                possibleExtensions.Any(ext =>
                    ext.Equals(x.Extension, StringComparison.OrdinalIgnoreCase))).ToList();

            // possible cover file
            FileInfo imageFileInfo = possibleCoverFiles.FirstOrDefault();
            if (imageFileInfo != null)
            {
                // create thumbnail and return it
                Image fileImage = Image.FromFile(imageFileInfo.FullName);
                thumbnailData = CreateThumbnail(fileImage, size);
            }

            // check if needed
            if (thumbnailData == null || thumbnailData.Length == 0)
            {
                if (File.Exists(FileName))
                {
                    if (CanReadTags)
                    {
                        // try embedded file
                        using (TagLib.File taglibFile = TagLib.File.Create(FileName))
                        {
                            if (taglibFile.Tag.Pictures.Length >= 1)
                            {
                                TagLib.IPicture picture = taglibFile.Tag.Pictures[0];

                                if (picture.Data != null && picture.Data.Data != null && picture.Data.Data.Length > 0)
                                {
                                    thumbnailData = CreateThumbnail(picture.Data.Data, size);
                                }
                            }
                        }
                    }
                }
            }

            // return nothing, if nothing found
            if (thumbnailData == null || thumbnailData.Length == 0)
            {
                return null;
            }

            // return image
            MemoryStream stream = new MemoryStream(thumbnailData);
            return stream;
        }

        private static byte[] CreateThumbnail(byte[] input, int maxSize)
        {
            using (MemoryStream memoryStream = new MemoryStream(input))
            {
                return CreateThumbnail(memoryStream, maxSize);
            }
        }

        private static byte[] CreateThumbnail(MemoryStream imageStream, int maxSize)
        {
            Image image;
            try
            {
                image = Image.FromStream(imageStream);
            }
            catch (Exception)
            {
                image = null;
            }

            if (image != null)
            {
                byte[] thumbnailBytes = CreateThumbnail(image, maxSize);

                image.Dispose();
                image = null;

                return thumbnailBytes;
            }

            return null;
        }

        private static byte[] CreateThumbnail(Image image, int maxSize)
        {
            // create tumbnail
            int width = maxSize;
            int maxHeight = maxSize;

            // Prevent using images internal thumbnail
            image.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            image.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (image.Width <= width)
            {
                width = image.Width;
            }

            int height = image.Height * width / image.Width;
            if (height > maxHeight)
            {
                // Resize with height instead
                width = image.Width * maxHeight / image.Height;
                height = maxHeight;
            }

            System.Drawing.Image thumbnail = image.GetThumbnailImage(width, height, null, IntPtr.Zero);

            byte[] bytes;
            using (MemoryStream stream = new MemoryStream())
            {
                thumbnail.Save(stream, ImageFormat.Png);

                bytes = stream.ToArray();
            }

            return bytes;
        }
    }
}
