using System.Runtime.CompilerServices;

namespace MonJobs.Take
{
    public class TakeNextOptions
    {
        public QueueId QueueId { get; set; }
        
        public JobAcknowledgment Acknowledgment { get; set; }

        private JobAttributes _hasAttributes;
        public JobAttributes HasAttributes
        {
            get { return _hasAttributes ?? (_hasAttributes = new JobAttributes()); }
            set { _hasAttributes = value; }
        }
    }
}