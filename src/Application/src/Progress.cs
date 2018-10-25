using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GetGit
{
    public class Progress<T>
    {
        private ILog _logger;
        private int _current;
        private int _total;
        private decimal _progress;

        public Progress(ILog logger, IEnumerable<T> enumerable, string message)
        {
            _logger = logger;
            _current = 0;
            _total = enumerable.Count();
            _progress = -1;

            if(_total > 0)
            {
                _logger.Info(message + " " + _total + (_total == 1 ? " item" : " items"));
            }
        }

        public void Report()
        {
            var percentage = Math.Floor(((decimal)_current / _total) * 100);
  
            if (_progress != percentage)
            {
                _progress = percentage;
                _logger.Info(_progress + "% (" + _current + "/" + _total + ")");
            }

            _current++;
        }

        public void Done(string message)
        {
            _logger.Info(message + " (" + _current + "/" + _total + ")");
        }
    }
}
