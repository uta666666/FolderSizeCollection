using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSizeCollection.Models
{
    public class Log : INotifyPropertyChanged
    {
        public Log()
        {
            _logList = new List<string>();
        }

        private List<string> _logList;


        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public string Value
        {
            get
            {
                return string.Join(Environment.NewLine, _logList);
            }
        }

        public void Add(string text)
        {
            _logList.Add(text);

            RaisePropertChanged(nameof(Value));
        }

        public void Clear()
        {
            _logList.Clear();
        }
    }
}
