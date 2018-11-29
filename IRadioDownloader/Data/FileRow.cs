﻿using System.Collections.Generic;
using System.Windows.Media;
using vt.Extensions;

namespace RadioOwl.Data
{
    /// <summary>
    /// trida radku downloadu
    /// </summary>
    public class FileRow :  PropertyChangedBase
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IList<FileRow> ParentList;

        private string _urlPage;
        /// <summary>
        /// main html page with links (for parsing)
        /// </summary>
        public string UrlPage
        {
            get { return _urlPage; }
            set
            {
                _urlPage = value;
                OnPropertyChanged();
            }
        }

        private string _urlMp3Download;
        /// <summary>
        /// url for mp3 file
        /// </summary>
        public string UrlMp3Download
        {
            get { return _urlMp3Download; }
            set
            {
                _urlMp3Download = value;
                OnPropertyChanged();
            }
        }

        private int _urlMpcDownloadNo;
        public int UrlMp3DownloadNo
        {
            get { return _urlMpcDownloadNo; }
            set
            {
                _urlMpcDownloadNo = value;
                OnPropertyChanged();
            }
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }


        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged();
            }
        }


        private string _id3Name;
        public string Id3Name
        {
            get { return _id3Name; }
            set
            {
                _id3Name = value;
                OnPropertyChanged();
            }
        }


        private string _id3NamePart;
        public string Id3NamePart
        {
            get { return _id3NamePart; }
            set
            {
                _id3NamePart = value;
                OnPropertyChanged();
            }
        }


        private string _id3NameSite;
        public string Id3NameSite
        {
            get { return _id3NameSite; }
            set
            {
                _id3NameSite = value;
                OnPropertyChanged();
            }
        }


        private FileRowState _state;
        public FileRowState State
        {
            get { return _state; }
            set
            {
                _state = value;
                StateColor = new SolidColorBrush(SetStateColor());
                OnPropertyChanged();
            }
        }


        private Brush _stateColor;
        /// <summary>
        /// barva se ve WPF binduje na Brush!
        /// </summary>
        public Brush StateColor
        {
            get { return _stateColor; }
            set
            {
                _stateColor = value;
                OnPropertyChanged();
            }
        }


        private int _progress;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
                OnPropertyChanged("ProgressPercent");
            }
        }


        private long _bytesReceived;
        public long BytesReceived
        {
            get { return _bytesReceived; }
            set
            {
                _bytesReceived = value;
                OnPropertyChanged();
                OnPropertyChanged("ProgressPercent");
            }
        }


        public string ProgressPercent
        {
            get { return string.Format("{0}%  {1}", Progress, BytesReceived.ToFileSize()); }
        }


        private List<string> _logList;
        public List<string> LogList
        {
            get { return _logList; }
            set
            {
                _logList = value;
                OnPropertyChanged();
            }
        }


        private int _logListIndex;
        public int LogListIndex
        {
            get { return _logListIndex; }
            set
            {
                _logListIndex = value;
                OnPropertyChanged();
            }
        }

        private string _savedFileName;
        public string SavedFileName
        {
            get { return _savedFileName; }
            set
            {
                _savedFileName = value;
                OnPropertyChanged();
            }
        }


        public FileRow(IList<FileRow> parentList, string urlPage)
        {
            ParentList = parentList;
            State = FileRowState.Started;
            LogList = new List<string>();
            UrlPage = urlPage;
        }

        public FileRow(IList<FileRow> parentList, StreamUrlRow streamUrlRow) : this(parentList, streamUrlRow?.Url)
        {
            Id3NamePart = streamUrlRow?.Title;
        }


        public void AddLog(string log)
        {
            LogList.Add(log);
            LogListIndex = LogList.Count - 1;
        }


        public void AddLog(string log, FileRowState fileRowState)
        {
            AddLog(log);
            State = fileRowState;
        }


        
        private Color SetStateColor()
        {
            switch (State)   
            {
                case FileRowState.Started:
                    return Colors.Orange;
                case FileRowState.Finnished:
                    return Colors.LightGreen;
                case FileRowState.Error:
                    return Colors.Red;
                default:
                    return Colors.Blue;
            }
        }
    }
}
