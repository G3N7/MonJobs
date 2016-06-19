namespace MonJobs.Peek
{
    public class PeekNextOptions
    {
        public QueueId QueueId { get; set; }

        private JobAttributes _hasAttributes;
        public JobAttributes HasAttributes
        {
            get { return _hasAttributes ?? (_hasAttributes = new JobAttributes()); }
            set { _hasAttributes = value; }
        }

        public int? Limit { get; set; }
    }
}