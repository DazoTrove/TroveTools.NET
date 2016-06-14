using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroveTools.NET.Framework
{
    class NotifyAppender : AppenderSkeleton, INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _lastMessage;
        private StringWriter _writer = new StringWriter(CultureInfo.InvariantCulture);
        private SimpleLayout _simpleLayout = new SimpleLayout();

        #region INotifyPropertyChanged Implementation
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties
        public string LastMessage
        {
            get { return _lastMessage?.Substring(0, _lastMessage.IndexOf(Environment.NewLine)); }
            set
            {
                _lastMessage = value;
                RaisePropertyChanged("LastMessage");
            }
        }

        public string Messages
        {
            get { return _writer.ToString(); }
        }
        #endregion

        #region log4net Appender Implementation
        /// <summary>
        /// Append the log information to the Messages
        /// </summary>
        /// <param name="loggingEvent">The log event.</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                _simpleLayout.Format(writer, loggingEvent);
                LastMessage = writer.ToString();
            }
            Layout.Format(_writer, loggingEvent);
            RaisePropertyChanged("Messages");
        }

        public static NotifyAppender Appender
        {
            get
            {
                foreach (ILog logger in LogManager.GetCurrentLoggers())
                {
                    foreach (IAppender appender in logger.Logger.Repository.GetAppenders())
                    {
                        if (appender is NotifyAppender) return appender as NotifyAppender;
                    }
                }
                return null;
            }
        }
        #endregion
    }
}
