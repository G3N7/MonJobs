namespace MonJobs
{
    public class JobQuery
    {
        public QueueId QueueId { get; set; }

        private JobAttributes _hasAttributes;
        public JobAttributes HasAttributes
        {
            get { return _hasAttributes ?? (_hasAttributes = new JobAttributes()); }
            set { _hasAttributes = value; }
        }

        // mitch: these should be value types instead of nullables.
        public int? Limit { get; set; }
        public bool? HasBeenAcknowledged { get; set; }
        //public bool? HasResult { get; set; }
        //public bool? HasReports { get; set; }
    }
}